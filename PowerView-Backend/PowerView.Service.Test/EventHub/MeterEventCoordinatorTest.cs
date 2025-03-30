using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
    [TestFixture]
    public class MeterEventCoordinatorTest
    {
        private Mock<IIntervalTrigger> intervalTrigger;
        private Mock<IServiceScope> serviceScope;
        private Mock<IServiceProvider> serviceProvider;

        [SetUp]
        public void SetUp()
        {
            intervalTrigger = new Mock<IIntervalTrigger>();
            serviceScope = new Mock<IServiceScope>();
            serviceProvider = new Mock<IServiceProvider>();
            serviceScope.Setup(ss => ss.ServiceProvider).Returns(serviceProvider.Object);
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new MeterEventCoordinator(null, intervalTrigger.Object), Throws.ArgumentNullException);
            Assert.That(() => new MeterEventCoordinator(new NullLogger<MeterEventCoordinator>(), null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor()
        {
            // Arrange

            // Act
            var target = CreateTarget();

            // Assert
            intervalTrigger.Verify(it => it.Setup(new TimeSpan(6, 0, 0), TimeSpan.FromDays(1)));
        }

        [Test]
        public void DetectAndNotify()
        {
            // Arrange
            intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
            var meterEventDetector = new Mock<IMeterEventDetector>();
            serviceProvider.Setup(sp => sp.GetService(typeof(IMeterEventDetector))).Returns(meterEventDetector.Object);
            var meterEventNotifier = new Mock<IMeterEventNotifier>();
            serviceProvider.Setup(sp => sp.GetService(typeof(IMeterEventNotifier))).Returns(meterEventNotifier.Object);
            var dateTime = DateTime.UtcNow;
            var target = CreateTarget();

            // Act
            target.DetectAndNotify(serviceScope.Object, dateTime);

            // Assert
            intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
            serviceProvider.Verify(sp => sp.GetService(typeof(IMeterEventDetector)));
            meterEventDetector.Verify(med => med.DetectMeterEvents(It.Is<DateTime>(x => x == dateTime && x.Kind == dateTime.Kind)));
            serviceProvider.Verify(sp => sp.GetService(typeof(IMeterEventNotifier)));
            meterEventNotifier.Verify(men => men.NotifyEmailRecipients());
            intervalTrigger.Verify(it => it.Advance(dateTime));
        }

        [Test]
        public void DetectAndNotifyNoTrigger()
        {
            // Arrange
            intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);
            var dateTime = DateTime.UtcNow;
            var target = CreateTarget();

            // Act
            target.DetectAndNotify(serviceScope.Object, dateTime);

            // Assert
            intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
            serviceProvider.Verify(sp => sp.GetService(typeof(IMeterEventDetector)), Times.Never);
            intervalTrigger.Verify(it => it.Advance(dateTime), Times.Never);
        }

        private MeterEventCoordinator CreateTarget()
        {
            return new MeterEventCoordinator(new NullLogger<MeterEventCoordinator>(), intervalTrigger.Object);
        }

    }
}
