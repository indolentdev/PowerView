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
    private Mock<IImportRepository> importRepository;
    private Mock<IEnergiDataServiceClient> energiDataServiceClient;
    private Mock<IReadingAccepter> readingAccepter;

    [SetUp]
    public void SetUp()
    {
        logger = new NullLogger<EnergiDataServiceImporter>();
        importRepository = new Mock<IImportRepository>();
        energiDataServiceClient = new Mock<IEnergiDataServiceClient>();
        readingAccepter = new Mock<IReadingAccepter>();
    }

    [Test]
    public void ConstructorThrows()
    {
        // Arrange

        // Act & Assert
        Assert.That(() => new EnergiDataServiceImporter(null, importRepository.Object, energiDataServiceClient.Object, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, null, energiDataServiceClient.Object, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, importRepository.Object, null, readingAccepter.Object), Throws.ArgumentNullException);
        Assert.That(() => new EnergiDataServiceImporter(logger, importRepository.Object, energiDataServiceClient.Object, null), Throws.ArgumentNullException);
    }

    [Test]
    [TestCase(DateTimeKind.Unspecified)]
    [TestCase(DateTimeKind.Local)]
    public void ImportThrows(DateTimeKind dateTimeKind)
    {
        // Arrange
        var target = CreateTarget();
        var dateTime = DateTime.SpecifyKind(DateTime.Now, dateTimeKind);

        // Act & Assert
        Assert.That(() => target.Import(dateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public async Task ImportsAbsent()
    {
        // Arrange
        SetupImportRepositoryGetImports();

        var target = CreateTarget();

        // Act
        await target.Import(DateTime.UtcNow);

        // Assert
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        importRepository.Verify(x => x.SetCurrentTimestamp(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportsDisabled()
    {
        // Arrange
        var import = new Import("label", "channel", Unit.Eur, DateTime.UtcNow, null, false);
        SetupImportRepositoryGetImports(import);

        var target = CreateTarget();

        // Act
        await target.Import(DateTime.UtcNow);

        // Assert
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        importRepository.Verify(x => x.SetCurrentTimestamp(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportNewPosition()
    {
        // Arrange
        var dateTime = new DateTime(2023, 9, 28, 22, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import(dateTime);

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.Is<DateTime>(p => p == dateTime && p.Kind == dateTime.Kind),
          It.IsAny<TimeSpan>(), It.IsAny<string>()));
    }

    [Test]
    public async Task ImportExistingPosition()
    {
        // Arrange
        var dateTime = new DateTime(2023, 9, 28, 22, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, dateTime.AddDays(1), true);
        SetupImportRepositoryGetImports(import);
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import(dateTime);

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.Is<DateTime>(p => p == dateTime.AddDays(1) && p.Kind == dateTime.Kind),
          It.IsAny<TimeSpan>(), It.IsAny<string>()));
    }

    [Test]
    public async Task ImportLargeChunk()
    {
        // Arrange
        var dateTime = new DateTime(2023, 9, 28, 22, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import(dateTime.AddDays(4));

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.IsAny<DateTime>(),
          It.Is<TimeSpan>(p => p == TimeSpan.FromDays(3)), It.IsAny<string>()));
    }

    [Test]
    public async Task ImportSmallChunk()
    {
        // Arrange
        var dateTime = new DateTime(2023, 9, 28, 22, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import(dateTime.AddDays(2));

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.IsAny<DateTime>(),
          It.Is<TimeSpan>(p => p == TimeSpan.FromHours(6)), It.IsAny<string>()));
    }

    [Test]
    public async Task ImportChannel()
    {
        // Arrange
        var dateTime = new DateTime(2023, 9, 28, 22, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);
        SetUpEnergiDataServiceClientGetElectricityAmounts();
        var target = CreateTarget();

        // Act
        await target.Import(dateTime);

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.IsAny<DateTime>(),
          It.IsAny<TimeSpan>(), It.Is<string>(p => p == import.Channel)));
    }


    [Test]
    public async Task ImportEnergiDataServiceClientThrows()
    {
        // Arrange
        var dateTime = new DateTime(2023, 9, 28, 22, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);
        energiDataServiceClient.Setup(x => x.GetElectricityAmounts(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
          .Throws(new EnergiDataServiceClientException("Drugs are bad. M'Kay"));
        var target = CreateTarget();

        // Act
        await target.Import(dateTime);

        // Assert
        energiDataServiceClient.Verify(x => x.GetElectricityAmounts(It.IsAny<DateTime>(),
          It.IsAny<TimeSpan>(), It.IsAny<string>()));
        readingAccepter.Verify(x => x.Accept(It.IsAny<IList<Reading>>()), Times.Never);
        importRepository.Verify(x => x.SetCurrentTimestamp(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task ImportDkkAmounts()
    {
        // Arrange
        var dateTime = new DateTime(2023, 2, 28, 23, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Dkk, dateTime, null, true);
        SetupImportRepositoryGetImports(import);

        var dateTime1 = new DateTime(2023, 5, 15, 21, 29, 0, DateTimeKind.Utc);
        var dateTime2 = dateTime1.AddHours(1);
        SetUpEnergiDataServiceClientGetElectricityAmounts(
            new KwhAmount { Start = dateTime1, AmountDkk = 12 },
            new KwhAmount { Start = dateTime2, AmountDkk = 15 });
        var target = CreateTarget();

        // Act
        await target.Import(dateTime);

        // Assert
        readingAccepter.Verify(x => x.Accept(It.Is<IList<Reading>>(p => p.Count == 2 &&
          p.First().Label == import.Label && p.First().DeviceId == "EnergiDataService" && p.First().Timestamp == dateTime1 && p.First().Timestamp.Kind == dateTime1.Kind &&
          p.First().GetRegisterValues().Count == 1 && p.First().GetRegisterValues().First().Value == 12 && p.First().GetRegisterValues().First().Scale == 0 &&
          p.First().GetRegisterValues().First().Unit == Unit.Dkk && p.First().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.First().GetRegisterValues().First().Tag == RegisterValueTag.Import &&
          p.Last().Label == import.Label && p.Last().DeviceId == "EnergiDataService" && p.Last().Timestamp == dateTime2 && p.Last().Timestamp.Kind == dateTime2.Kind &&
          p.Last().GetRegisterValues().Count == 1 && p.Last().GetRegisterValues().First().Value == 15 && p.Last().GetRegisterValues().First().Scale == 0 &&
          p.Last().GetRegisterValues().Last().Unit == Unit.Dkk && p.Last().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.Last().GetRegisterValues().Last().Tag == RegisterValueTag.Import)));

        importRepository.Verify(x => x.SetCurrentTimestamp(It.Is<string>(p => p == import.Label), It.Is<DateTime>(p => p == dateTime2.AddHours(1) && p.Kind == dateTime2.Kind)));
    }

    [Test]
    public async Task ImportEurAmounts()
    {
        // Arrange
        var dateTime = new DateTime(2023, 2, 28, 23, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);

        var dateTime1 = new DateTime(2023, 5, 15, 21, 29, 0, DateTimeKind.Utc);
        var dateTime2 = dateTime1.AddHours(1);
        SetUpEnergiDataServiceClientGetElectricityAmounts(
            new KwhAmount { Start = dateTime1, AmountEur = 12 },
            new KwhAmount { Start = dateTime2, AmountEur = 15 });
        var target = CreateTarget();

        // Act
        await target.Import(dateTime);

        // Assert
        readingAccepter.Verify(x => x.Accept(It.Is<IList<Reading>>(p => p.Count == 2 &&
          p.First().Label == import.Label && p.First().DeviceId == "EnergiDataService" && p.First().Timestamp == dateTime1 && p.First().Timestamp.Kind == dateTime1.Kind &&
          p.First().GetRegisterValues().Count == 1 && p.First().GetRegisterValues().First().Value == 12 && p.First().GetRegisterValues().First().Scale == 0 &&
          p.First().GetRegisterValues().First().Unit == Unit.Eur && p.First().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.First().GetRegisterValues().First().Tag == RegisterValueTag.Import &&
          p.Last().Label == import.Label && p.Last().DeviceId == "EnergiDataService" && p.Last().Timestamp == dateTime2 && p.Last().Timestamp.Kind == dateTime2.Kind &&
          p.Last().GetRegisterValues().Count == 1 && p.Last().GetRegisterValues().First().Value == 15 && p.Last().GetRegisterValues().First().Scale == 0 &&
          p.Last().GetRegisterValues().Last().Unit == Unit.Eur && p.Last().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpense &&
          p.Last().GetRegisterValues().Last().Tag == RegisterValueTag.Import)));

        importRepository.Verify(x => x.SetCurrentTimestamp(It.Is<string>(p => p == import.Label), It.Is<DateTime>(p => p == dateTime2.AddHours(1) && p.Kind == dateTime2.Kind)));
    }

    [Test]
    public async Task ImportReadingAccepterThrows()
    {
        // Arrange
        var dateTime = new DateTime(2023, 2, 28, 23, 0, 0, DateTimeKind.Utc);
        var import = new Import("label", "channel", Unit.Eur, dateTime, null, true);
        SetupImportRepositoryGetImports(import);
        SetUpEnergiDataServiceClientGetElectricityAmounts(new KwhAmount { Start = new DateTime(2023, 5, 15, 21, 29, 0, DateTimeKind.Utc), AmountDkk = 13 });
        readingAccepter.Setup(x => x.Accept(It.IsAny<IList<Reading>>())).Throws(new DataStoreBusyException());
        var target = CreateTarget();

        // Act
        await target.Import(DateTime.UtcNow);

        // Assert
        importRepository.Verify(x => x.SetCurrentTimestamp(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
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
 
    private Import[] SetupImportRepositoryGetImports(params Import[] imports)
    {
        importRepository.Setup(x => x.GetImports()).Returns(imports);
        return imports;
    }

    private KwhAmount[] SetUpEnergiDataServiceClientGetElectricityAmounts(params KwhAmount[] kwhAmounts)
    {
        energiDataServiceClient.Setup(x => x.GetElectricityAmounts(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
        .ReturnsAsync(kwhAmounts.ToList());
        return kwhAmounts;
    }

    private EnergiDataServiceImporter CreateTarget()
    {
        return new EnergiDataServiceImporter(logger, importRepository.Object, energiDataServiceClient.Object, readingAccepter.Object);
    }

}