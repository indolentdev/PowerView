using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;
using PowerView.Service.EnergiDataService;
using PowerView.Model;
using PowerView.Model.Repository;
using System.Net.Http;
using Microsoft.Extensions.Http;

namespace PowerView.Service.Test.EnergiDataService;

[TestFixture]
public class EnergiDataServiceClientTest
{
    private EnergiDataServiceClientOptions options;
    private Mock<IHttpClientFactory> httpClientFactory;

    [SetUp]
    public void SetUp()
    {
        options = new EnergiDataServiceClientOptions();
        httpClientFactory = new Mock<IHttpClientFactory>();
    }

    [Test]
    public void ConstructorThrows()
    {
        // Arrange

        // Act & Assert
        Assert.That(() => new EnergiDataServiceClient(null, httpClientFactory.Object), Throws.TypeOf<ArgumentNullException>());
        Assert.That(() => new EnergiDataServiceClient(options, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetElectricityAmountsThrows()
    {
        // Arrange
        var dateTime = new DateTime(2023, 04, 18, 22, 13, 22, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromHours(3);
        const string priceArea = "DK9";
        var handler = SetupHttpClientFactory("{\"records\":[]}");

        var target = CreateTarget();

        // Act
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => target.GetElectricityAmounts(DateTime.Now, timeSpan, priceArea));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => target.GetElectricityAmounts(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), timeSpan, priceArea));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => target.GetElectricityAmounts(dateTime, TimeSpan.Zero, priceArea));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => target.GetElectricityAmounts(dateTime, TimeSpan.FromMilliseconds(-1), priceArea));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => target.GetElectricityAmounts(dateTime, TimeSpan.FromDays(7).Add(TimeSpan.FromMilliseconds(1)), priceArea));
        Assert.ThrowsAsync<ArgumentNullException>(() => target.GetElectricityAmounts(dateTime, timeSpan, null));
        Assert.ThrowsAsync<ArgumentException>(() => target.GetElectricityAmounts(dateTime, timeSpan, string.Empty));
    }

    [Test]
    public async Task GetElectricityAmountsRequestQuery()
    {
        // Arrange
        var dateTime = new DateTime(2023, 04, 18, 22, 13, 22, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromHours(3);
        const string priceArea = "DK9";
        var handler = SetupHttpClientFactory("{\"records\":[]}");

        var target = CreateTarget();

        // Act
        await target.GetElectricityAmounts(dateTime, timeSpan, priceArea);

        // Assert
        handler.Verify(x => x(It.Is<HttpRequestMessage>(a => a.RequestUri.AbsolutePath == "/dataset/elspotprices"), It.IsAny<CancellationToken>()));
        handler.Verify(x => x(It.Is<HttpRequestMessage>(a => a.RequestUri.Query.Contains("start=2023-04-18T22%3A13")), It.IsAny<CancellationToken>()));
        handler.Verify(x => x(It.Is<HttpRequestMessage>(a => a.RequestUri.Query.Contains("end=2023-04-19T01%3A13")), It.IsAny<CancellationToken>()));
        handler.Verify(x => x(It.Is<HttpRequestMessage>(a => a.RequestUri.Query.Contains("filter=%7B%22PriceArea%22%3A%5B%22DK9%22%5D%7D")), It.IsAny<CancellationToken>()));
        handler.Verify(x => x(It.Is<HttpRequestMessage>(a => a.RequestUri.Query.Contains("timezone=UTC")), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task GetElectricityAmountsRecordsEmpty()
    {
        // Arrange
        var dateTime = new DateTime(2023, 04, 18, 22, 13, 22, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromHours(3);
        const string priceArea = "DK9";
        var handler = SetupHttpClientFactory("{\"records\":[]}");

        var target = CreateTarget();

        // Act
        var amounts = await target.GetElectricityAmounts(dateTime, timeSpan, priceArea);

        // Assert
        Assert.That(amounts, Is.Empty);
    }

    [Test]
    public async Task GetElectricityAmountsRecords()
    {
        // Arrange
        var dateTime = new DateTime(2023, 04, 18, 22, 13, 22, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromHours(3);
        const string priceArea = "DK9";
        var handler = SetupHttpClientFactory("{\"total\": 54,\"records\":[{\"HourUTC\": \"2023-04-17T20:00:00\",\"HourDK\": \"2023-04-17T22:00:00\",\"PriceArea\": \"DK2\",\"SpotPriceDKK\": 915.02002,\"SpotPriceEUR\": 122.82}]}");

        var target = CreateTarget();

        // Act
        var amounts = await target.GetElectricityAmounts(dateTime, timeSpan, priceArea);

        // Assert
        Assert.That(amounts.Count, Is.EqualTo(1));
        Assert.That(amounts[0].Start, Is.EqualTo(new DateTime(2023, 4, 17, 20, 0, 0)));
        Assert.That(amounts[0].Start.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(amounts[0].Duration, Is.EqualTo(TimeSpan.FromHours(1)));
        Assert.That(amounts[0].AmountDkk, Is.EqualTo(0.91502002));
        Assert.That(amounts[0].AmountEur, Is.EqualTo(0.12282));
    }

    [Test]
    public void GetElectricityAmountsRecordsHttpErrorThrows()
    {
        // Arrange
        var dateTime = new DateTime(2023, 04, 18, 22, 13, 22, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromHours(3);
        const string priceArea = "DK9";
        var handler = SetupHttpClientFactory(new HttpRequestException("Drugs are bad. M'kay"));

        var target = CreateTarget();

        // Act & Assert
        Assert.ThrowsAsync<EnergiDataServiceClientException>(() => target.GetElectricityAmounts(dateTime, timeSpan, priceArea));
    }

    [Test]
    public void GetElectricityAmountsRecordsBadJsonThrows()
    {
        // Arrange
        var dateTime = new DateTime(2023, 04, 18, 22, 13, 22, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromHours(3);
        const string priceArea = "DK9";
        var handler = SetupHttpClientFactory("BAD JSON");

        var target = CreateTarget();

        // Act & Assert
        Assert.ThrowsAsync<EnergiDataServiceClientException>(() => target.GetElectricityAmounts(dateTime, timeSpan, priceArea));
    }

    private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory(string responseContent)
    {
        var httpMessageHandler = SetupHttpClientFactory();
        httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
              { Content = new StringContent(responseContent, Encoding.UTF8, "application/json") } );

        return httpMessageHandler;
    }

    private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory(Exception exception)
    {
        var httpMessageHandler = SetupHttpClientFactory();
        httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Throws(exception);

        return httpMessageHandler;
    }

    private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory()
    {
        var func = new Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>>();
        var stub = new HttpMessageHandlerStub(func.Object);
        var httpClient = new HttpClient(stub);
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return func;
    }

    private EnergiDataServiceClient CreateTarget()
    {
        return new EnergiDataServiceClient(options, httpClientFactory.Object);
    }


    [Test]
    public async Task T()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var httpClient = new HttpClient();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var target = new EnergiDataServiceClient(new EnergiDataServiceClientOptions(), httpClientFactory.Object);

        var start = new DateTime(2023, 02, 27, 23, 00, 0, DateTimeKind.Utc);
        var timeSpan = TimeSpan.FromDays(1);

        // Act
        var x = await target.GetElectricityAmounts(start, timeSpan, "DK1");

        // Assert
    }

}