using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EnergiDataService;
using PowerView.Service.EventHub;
using Moq;

namespace PowerView.Service.Test.EventHub;

[TestFixture]
public class EnergiDataServiceImporterTest
{
    private ILogger<EnergiDataServiceImporter> logger;
    private Mock<ISettingRepository> settingRepository;
    private Mock<ILocationContext> locationContext;
    private Mock<IEnergiDataServiceClient> energiDataServiceClient;
    private Mock<IReadingAccepter> readingAccepter;

    [SetUp]
    public void SetUp()
    {
        logger = new NullLogger<EnergiDataServiceImporter>();
        settingRepository = new Mock<ISettingRepository>();
        locationContext = new Mock<ILocationContext>();
        energiDataServiceClient = new Mock<IEnergiDataServiceClient>();
        readingAccepter = new Mock<IReadingAccepter>();
    }

    [Test]
    public void ConstructorThrows()
    {
        // Arrange

        // Act & Assert
        Assert.That(() => new EnergiDataServiceImporter(null, settingRepository.Object, locationContext.Object, energiDataServiceClient.Object, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, null, locationContext.Object, energiDataServiceClient.Object, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, settingRepository.Object, null, energiDataServiceClient.Object, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, settingRepository.Object, locationContext.Object, null, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, settingRepository.Object, locationContext.Object, energiDataServiceClient.Object, null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task ImportConfigThrows()
    {
        // Arrange
        settingRepository.Setup(x => x.GetEnergiDataServiceImportConfig()).Throws(new DomainConstraintException("Drugs are baad. M'kay"));

        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportConfigDisabled()
    {
        // Arrange
        SetUpSettingRepositoryGetEnergiDataServiceImportConfig(false);

        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportNewStart()
    {
        // Arrange
        var config = SetUpSettingRepositoryGetEnergiDataServiceImportConfig();
        var start = SetUpLocationContextConvertTimeToUtc();
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        locationContext.Verify(x => x.ConvertTimeToUtc(DateTime.Today));
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.Is<DateTime>(p => p == start && p.Kind == start.Kind),
          It.Is<TimeSpan>(p => p == config.TimeSpan), It.Is<string>(p => p == config.PriceArea)));
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportExistingStart()
    {
        // Arrange
        var config = SetUpSettingRepositoryGetEnergiDataServiceImportConfig();
        var start = SetUpSettingRepositoryGetEnergiDataServiceImportPosition();
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        locationContext.Verify(x => x.ConvertTimeToUtc(It.IsAny<DateTime>()), Times.Never);
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.Is<DateTime>(p => p == start && p.Kind == start.Kind),
          It.Is<TimeSpan>(p => p == config.TimeSpan), It.Is<string>(p => p == config.PriceArea)));
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportEnergiDataServiceClientThrows()
    {
        // Arrange
        var config = SetUpSettingRepositoryGetEnergiDataServiceImportConfig();
        var start = SetUpSettingRepositoryGetEnergiDataServiceImportPosition();
        energiDataServiceClient.Setup(x => x.GetElectricityAmounts(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
          .Throws(new EnergiDataServiceClientException("Drugs are bad. M'Kay"));
        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.Is<DateTime>(p => p == start && p.Kind == start.Kind),
          It.Is<TimeSpan>(p => p == config.TimeSpan), It.Is<string>(p => p == config.PriceArea)));
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportDkkAmounts()
    {
        // Arrange
        var config = SetUpSettingRepositoryGetEnergiDataServiceImportConfig(currency: Unit.Dkk);
        SetUpSettingRepositoryGetEnergiDataServiceImportPosition();
        var dateTime1 = new DateTime(2023, 5, 15, 21, 29, 0, DateTimeKind.Utc);
        var dateTime2 = dateTime1.AddHours(1);
        SetUpEnergiDataServiceClientGetElectricityAmounts(
            new KwhAmount { Start = dateTime1, AmountDkk = 12 },
            new KwhAmount { Start = dateTime2, AmountDkk = 15 } );
        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        readingAccepter.Verify(x => x.Accept(It.Is<IList<Reading>>(p => p.Count == 2 &&
          p.First().Label == config.Label && p.First().DeviceId == "EnergiDataService" && p.First().Timestamp == dateTime1 && p.First().Timestamp.Kind == dateTime1.Kind &&
          p.First().GetRegisterValues().Count == 1 && p.First().GetRegisterValues().First().Value == 12 && p.First().GetRegisterValues().First().Scale == 0 &&
          p.First().GetRegisterValues().First().Unit == Unit.Dkk && p.First().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.First().GetRegisterValues().First().Tag == RegisterValueTag.Import &&
          p.Last().Label == config.Label && p.Last().DeviceId == "EnergiDataService" && p.Last().Timestamp == dateTime2 && p.Last().Timestamp.Kind == dateTime2.Kind &&
          p.Last().GetRegisterValues().Count == 1 && p.Last().GetRegisterValues().First().Value == 15 && p.Last().GetRegisterValues().First().Scale == 0 &&
          p.Last().GetRegisterValues().Last().Unit == Unit.Dkk && p.Last().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.Last().GetRegisterValues().Last().Tag == RegisterValueTag.Import )));
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.Is<DateTime>(p => p == dateTime2 && p.Kind == dateTime2.Kind)));
    }

    [Test]
    public async Task ImportEurAmounts()
    {
        // Arrange
        var config = SetUpSettingRepositoryGetEnergiDataServiceImportConfig(currency: Unit.Eur);
        SetUpSettingRepositoryGetEnergiDataServiceImportPosition();
        var dateTime1 = new DateTime(2023, 5, 15, 21, 29, 0, DateTimeKind.Utc);
        var dateTime2 = dateTime1.AddHours(1);
        SetUpEnergiDataServiceClientGetElectricityAmounts(
            new KwhAmount { Start = dateTime1, AmountEur = 13 },
            new KwhAmount { Start = dateTime2, AmountEur = 16 });
        var target = CreateTarget();

        // Act
        await target.Import();

        // Assert
        readingAccepter.Verify(x => x.Accept(It.Is<IList<Reading>>(p => p.Count == 2 &&
          p.First().Label == config.Label && p.First().DeviceId == "EnergiDataService" && p.First().Timestamp == dateTime1 && p.First().Timestamp.Kind == dateTime1.Kind &&
          p.First().GetRegisterValues().Count == 1 && p.First().GetRegisterValues().First().Value == 13 && p.First().GetRegisterValues().First().Scale == 0 &&
          p.First().GetRegisterValues().First().Unit == Unit.Eur && p.First().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.First().GetRegisterValues().First().Tag == RegisterValueTag.Import &&
          p.Last().Label == config.Label && p.Last().DeviceId == "EnergiDataService" && p.Last().Timestamp == dateTime2 && p.Last().Timestamp.Kind == dateTime2.Kind &&
          p.Last().GetRegisterValues().Count == 1 && p.Last().GetRegisterValues().First().Value == 16 && p.Last().GetRegisterValues().First().Scale == 0 &&
          p.Last().GetRegisterValues().Last().Unit == Unit.Eur && p.Last().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.Last().GetRegisterValues().Last().Tag == RegisterValueTag.Import)));
        settingRepository.Verify(x => x.UpsertEnergiDataServiceImportPosition(It.Is<DateTime>(p => p == dateTime2 && p.Kind == dateTime2.Kind)));
    }


    [Test]
    [TestCase(1, 1, 0)]
    [TestCase(12, 12, 0)]
    [TestCase(123, 123, 0)]
    [TestCase(123.4, 1234000000, -7)]
    [TestCase(123.45, 1234500000, -7)]
    [TestCase(123.456, 1234560000, -7)]
    [TestCase(0.76148999, 7614899, -7)]
    [TestCase(-1, -1, 0)]
    [TestCase(-123.4, -1234000000, -7)]
    public void LossyToRegisterValue(double amount, int expectedValue, short expectedScale)
    {
        // Arrange

        // Act
        var (value, scale) = EnergiDataServiceImporter.LossyToRegisterValue(amount);

        // Assert
        Assert.That(value, Is.EqualTo(expectedValue));
        Assert.That(scale, Is.EqualTo(expectedScale));
    }

    private EnergiDataServiceImportConfig SetUpSettingRepositoryGetEnergiDataServiceImportConfig(bool importEnabled = true, TimeSpan? timeSpan = null, string priceArea = "DK1", string label = "Main", Unit currency = Unit.Dkk)
    {
        if (timeSpan == null)
        {
            timeSpan = TimeSpan.FromHours(12);
        }
        var config = new EnergiDataServiceImportConfig(importEnabled, timeSpan.Value, priceArea, label, currency);
        settingRepository.Setup(x => x.GetEnergiDataServiceImportConfig()).Returns(config);
        return config;
    }

    private DateTime SetUpSettingRepositoryGetEnergiDataServiceImportPosition(DateTime? dateTime = null)
    {
        if (dateTime == null)
        {
            dateTime = DateTime.UtcNow;
        }
        settingRepository.Setup(x => x.GetEnergiDataServiceImportPosition()).Returns(dateTime);
        return dateTime.Value;
    }

    private DateTime SetUpLocationContextConvertTimeToUtc(DateTime? dateTime = null)
    {
        if (dateTime == null)
        {
            dateTime = DateTime.UtcNow;
        }
        locationContext.Setup(x => x.ConvertTimeToUtc(It.IsAny<DateTime>())).Returns(dateTime.Value);
        return dateTime.Value;
    }

    private KwhAmount[] SetUpEnergiDataServiceClientGetElectricityAmounts(params KwhAmount[] kwhAmounts)
    {

        energiDataServiceClient.Setup(x => x.GetElectricityAmounts(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
        .ReturnsAsync(kwhAmounts.ToList());
        return kwhAmounts;
    }


    private EnergiDataServiceImporter CreateTarget()
    {
        return new EnergiDataServiceImporter(logger, settingRepository.Object, locationContext.Object, energiDataServiceClient.Object, readingAccepter.Object);
    }

}