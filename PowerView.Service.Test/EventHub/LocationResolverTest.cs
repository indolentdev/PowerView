using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Moq;
using PowerView.Service.EventHub;
using PowerView.Model.Repository;

namespace PowerView.Service.Test.EventHub
{
    [TestFixture]
    public class LocationResolverTest
    {
        private Mock<IHttpClientFactory> httpClientFactory;
        private Mock<ISettingRepository> settingRepository;

        [SetUp]
        public void SetUp()
        {
            httpClientFactory = new Mock<IHttpClientFactory>();
            settingRepository = new Mock<ISettingRepository>();
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var logger = new NullLogger<LocationResolver>();

            // Act & Assert
            Assert.That(() => new LocationResolver(null, httpClientFactory.Object, settingRepository.Object), Throws.ArgumentNullException);
            Assert.That(() => new LocationResolver(logger, null, settingRepository.Object), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new LocationResolver(logger, httpClientFactory.Object, null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ResolveToDatabaseHttpError()
        {
            // Arrange
            var httpMessageHandler = SetupHttpClientFactory();
            httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Throws(new HttpRequestException());
            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResolveToDatabaseJsonError()
        {
            // Arrange
            SetupHttpClientFactory("Bad json object");
            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResolveToDatabaseEmptyContentError()
        {
            // Arrange
            SetupHttpClientFactory(string.Empty);
            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResolveToDatabaseContentError()
        {
            // Arrange
            SetupHttpClientFactory("{\"status\":\"failed\"}");
            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResolveToDatabaseSuccessNoProperties()
        {
            // Arrange
            SetupHttpClientFactory("{\"status\":\"success\"}");
            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResolveToDatabaseTimeZone()
        {
            // Arrange
            var timeZoneId = "TheTimeZone";
            SetupHttpClientFactory("{\"status\":\"success\",\"timezone\":\"" + timeZoneId + "\"}");

            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(r => r.Upsert(Settings.TimeZoneId, timeZoneId));
        }

        [Test]
        public void ResolveToDatabaseCountryToOneCultureInfo()
        {
            // Arrange
            SetupHttpClientFactory("{\"status\":\"success\",\"country\":\"Czechia\"}");

            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(r => r.Upsert(Settings.CultureInfoName, "cs-CZ"));
        }

        [Test]
        public void ResolveToDatabaseCountryToNoCultureInfo()
        {
            // Arrange
            SetupHttpClientFactory("{\"status\":\"success\",\"country\":\"CountryThatDoesNotExist\"}");

            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResolveToDatabaseCountryToMoreThanOneCultureInfo()
        {
            // Arrange
            SetupHttpClientFactory("{\"status\":\"success\",\"country\":\"Denmark\"}");

            var target = CreateTarget();

            // Act
            target.ResolveToDatabase();

            // Assert
            settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private void SetupHttpClientFactory(string responseContent)
        {
            var httpMessageHandler = SetupHttpClientFactory();
            httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
                  { Content = new StringContent(responseContent, Encoding.UTF8, "application/json") } );
        }

        private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory()
        {
            var func = new Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>>();
            var stub = new HttpMessageHandlerStub(func.Object);
            var httpClient = new HttpClient(stub);
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            return func;
        }

        private LocationResolver CreateTarget()
        {
            return new LocationResolver(new NullLogger<LocationResolver>(), httpClientFactory.Object, settingRepository.Object);
        }

    }
}

