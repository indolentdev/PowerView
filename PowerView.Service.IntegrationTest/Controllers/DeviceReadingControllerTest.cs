using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Controllers;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

public class DeviceReadingControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IReadingAccepter> readingAccepter;
    private Mock<IReadingHistoryRepository> readingHistoryRepository;

    [SetUp]
    public void Setup()
    {
        readingAccepter = new Mock<IReadingAccepter>();
        readingHistoryRepository = new Mock<IReadingHistoryRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(readingAccepter.Object);
                    sc.AddSingleton(readingHistoryRepository.Object);
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        application?.Dispose();
    }

    [Test]
    public async Task PostApiDevicesLivereadings()
    {
        // Arrange
        var liveReadingSetDto = GetLiveReadingSetDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/livereadings", JsonContent.Create(liveReadingSetDto));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task PostApiDevicesLivereadings_CallsReadingAccepter()
    {
        // Arrange
        var liveReadingDto = GetLiveReadingDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/livereadings", JsonContent.Create(new LiveReadingSetDto { Items = new [] { liveReadingDto } } ));

        // Assert
        readingAccepter.Verify(ra => ra.Accept(It.Is<IList<Reading>>(x => x.Count == 1 &&
            x[0].Label == liveReadingDto.Label && x[0].DeviceId == liveReadingDto.DeviceId && x[0].Timestamp == liveReadingDto.Timestamp &&
            x[0].GetRegisterValues().Count == 1 && 
            x[0].GetRegisterValues()[0].ObisCode == liveReadingDto.RegisterValues[0].ObisCode &&
            x[0].GetRegisterValues()[0].Value == liveReadingDto.RegisterValues[0].Value &&
            x[0].GetRegisterValues()[0].Scale == liveReadingDto.RegisterValues[0].Scale &&
            x[0].GetRegisterValues()[0].Unit == liveReadingDto.RegisterValues[0].Unit
         )));
    }

    [Test]
    public async Task PostApiDevicesLivereadings_CallsReadingAccepter_SerialNumber()
    {
        // Arrange
        var liveReadingDto = GetLiveReadingDto();
        liveReadingDto.DeviceId = null;
        liveReadingDto.SerialNumber = 12345678;

        // Act
        var response = await httpClient.PostAsync("api/devices/livereadings", JsonContent.Create(new LiveReadingSetDto { Items = new[] { liveReadingDto } }));

        // Assert
        readingAccepter.Verify(ra => ra.Accept(It.Is<IList<Reading>>(x => x.Count == 1 &&
            x[0].Label == liveReadingDto.Label && x[0].DeviceId == liveReadingDto.SerialNumber.ToString() && x[0].Timestamp == liveReadingDto.Timestamp &&
            x[0].GetRegisterValues().Count == 1 &&
            x[0].GetRegisterValues()[0].ObisCode == liveReadingDto.RegisterValues[0].ObisCode &&
            x[0].GetRegisterValues()[0].Value == liveReadingDto.RegisterValues[0].Value &&
            x[0].GetRegisterValues()[0].Scale == liveReadingDto.RegisterValues[0].Scale &&
            x[0].GetRegisterValues()[0].Unit == liveReadingDto.RegisterValues[0].Unit
         )));
    }

    [Test]
    public async Task PostApiDevicesLivereadings_DataStoreBusy()
    {
        // Arrange
        readingAccepter.Setup(ra => ra.Accept(It.IsAny<IList<Reading>>())).Throws<DataStoreBusyException>();
        var liveReadingSetDto = GetLiveReadingSetDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/livereadings", JsonContent.Create(liveReadingSetDto));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public async Task PostApiDevicesLivereadings_JsonError()
    {
        // Arrange

        // Act
        var response = await httpClient.PostAsync("api/devices/livereadings", new StringContent("{Bad json", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostApiDevicesLivereadings_ModelValidationError()
    {
        // Arrange

        // Act
        var response = await httpClient.PostAsync("api/devices/livereadings", JsonContent.Create(new object()));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostApiDevicesManualRegister()
    {
        // Arrange
        var dto = GetPostCrudeValueDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/manualregisters", JsonContent.Create(dto));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task PostApiDevicesManualRegister_CallsReadingAccepter()
    {
        // Arrange
        var dto = GetPostCrudeValueDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/manualregisters", JsonContent.Create(dto));

        // Assert
        readingAccepter.Verify(ra => ra.Accept(It.Is<IList<Reading>>(x => x.Count == 1 &&
            x[0].Label == dto.Label && x[0].DeviceId == dto.DeviceId && x[0].Timestamp == dto.Timestamp &&
            x[0].GetRegisterValues().Count == 1 &&
            x[0].GetRegisterValues()[0].ObisCode == dto.ObisCode &&
            x[0].GetRegisterValues()[0].Value == dto.Value &&
            x[0].GetRegisterValues()[0].Scale == dto.Scale &&
            x[0].GetRegisterValues()[0].Unit == UnitMapper.Map(dto.Unit)
         )));
    }

    [Test]
    public async Task PostApiDevicesManualRegister_CallsReadingHistoryRepository()
    {
        // Arrange
        var dto = GetPostCrudeValueDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/manualregisters", JsonContent.Create(dto));

        // Assert
        readingHistoryRepository.Verify(x => x.ClearDayMonthYearHistory());
    }

    [Test]
    public async Task PostApiDevicesManualRegister_Conflict()
    {
        // Arrange
        readingAccepter.Setup(ra => ra.Accept(It.IsAny<IList<Reading>>())).Throws<DataStoreUniqueConstraintException>();
        var dto = GetPostCrudeValueDto();

        // Act
        var response = await httpClient.PostAsync("api/devices/manualregisters", JsonContent.Create(dto));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task PostApiDevicesManualRegister_JsonError()
    {
        // Arrange

        // Act
        var response = await httpClient.PostAsync("api/devices/manualregisters", new StringContent("{Bad json", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostApiDevicesManualRegister_ModelValidationError()
    {
        // Arrange

        // Act
        var response = await httpClient.PostAsync("api/devices/manualregisters", JsonContent.Create(new object()));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static LiveReadingSetDto GetLiveReadingSetDto()
    {
        return new LiveReadingSetDto
        {
            Items = new[] { GetLiveReadingDto() }
        };
    }

    private static LiveReadingDto GetLiveReadingDto()
    {
        return new LiveReadingDto
        {
            Label = "TheLabel",
            DeviceId = "TheDeviceId",
            Timestamp = DateTime.UtcNow,
            RegisterValues = new[] { new RegisterValueDto { ObisCode = "1.2.3.4.5.6", Value = 1234, Scale = -1, Unit = Model.Unit.CubicMetre } }
        };
    }

    private static PostCrudeValueDto GetPostCrudeValueDto()
    {
        return new PostCrudeValueDto
        {
            Label = "TheLabel",
            Timestamp = DateTime.UtcNow,
            ObisCode = "1.2.3.4.5.6",
            Value = 1234,
            Scale = -2,
            Unit = "W",
            DeviceId = "TheDeviceId",
        };
    }

}
