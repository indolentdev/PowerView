using System;
using System.Linq;
using System.Net.NetworkInformation;
using NUnit.Framework;

namespace PowerView.Service.Test
{
    [TestFixture]
    public class UrlProviderTest
    {
        [Test]
        public void GetEventsUrlSuffix()
        {
            // Arrange
            var options = new ServiceOptions { BaseUrl = "http://localhost" };
            var target = new UrlProvider(options);

            // Act
            var eventsUrl = target.GetEventsUrl();

            // Assert
            Assert.That(eventsUrl.ToString(), Does.Not.EndWith("Content/index.html#!/events"));
        }

        [Test]
        [TestCase("localhost")]
        [TestCase("127.0.0.1")]
        public void GetEventsUrlChangeLoopbackHost(string host)
        {
            // Arrange
            if (!NetworkInterface.GetAllNetworkInterfaces().Any(x => x.OperationalStatus == OperationalStatus.Up))
            {
                Assert.Inconclusive("Requires at least one IPv4 interface to be up and running");
            }

            var options = new ServiceOptions { BaseUrl = "http://" + host };
            var target = new UrlProvider(options);

            // Act
            var eventsUrl = target.GetEventsUrl();

            // Assert
            Assert.That(eventsUrl.ToString(), Does.Not.Contain(host));
        }

        [Test]
        [TestCase("nonloopback")]
        [TestCase("192.168.1.123")]
        public void GetEventsUrlPassthroughHost(string host)
        {
            // Arrange
            var options = new ServiceOptions { BaseUrl = "http://" + host };
            var target = new UrlProvider(options);

            // Act
            var eventsUrl = target.GetEventsUrl();

            // Assert
            Assert.That(eventsUrl.ToString(), Does.Contain(host));
        }


    }
}
