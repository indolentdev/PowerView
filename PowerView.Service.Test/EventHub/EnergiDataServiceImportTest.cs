using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
    [TestFixture]
    public class EnergiDataServiceImportTest
    {
        private Mock<IIntervalTrigger> intervalTrigger;
        private Mock<IServiceScope> serviceScope;
        private Mock<IServiceProvider> serviceProvider;
        private Mock<IEnergiDataServiceImporter> importer;

        [SetUp]
        public void SetUp()
        {
            intervalTrigger = new Mock<IIntervalTrigger>();
            serviceScope = new Mock<IServiceScope>();
            serviceProvider = new Mock<IServiceProvider>();
            serviceScope.Setup(ss => ss.ServiceProvider).Returns(serviceProvider.Object);

            importer = new Mock<IEnergiDataServiceImporter>();

            serviceProvider.Setup(sp => sp.GetService(typeof(IEnergiDataServiceImporter))).Returns(importer.Object);
        }

        [Test]
        public void Constructor()
        {
            // Arrange

            // Act
            var target = CreateTarget();

            // Assert
            intervalTrigger.Verify(it => it.Setup(TimeSpan.FromMinutes(0), TimeSpan.FromHours(1)));
        }

        [Test]
        public async Task HourlyCheck()
        {
            // Arrange
            var dateTime = DateTime.UtcNow;
            var target = CreateTarget();
            intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);

            // Act
            await target.Import(serviceScope.Object, dateTime);

            // Assert
            intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
            serviceProvider.Verify(sp => sp.GetService(typeof(IEnergiDataServiceImporter)));
            importer.Verify(dc => dc.Import(dateTime));
            intervalTrigger.Verify(it => it.Advance(dateTime));
        }

        [Test]
        public async Task HourlyCheckNoTrigger()
        {
            // Arrange
            var dateTime = DateTime.UtcNow;
            var target = CreateTarget();
            intervalTrigger.Setup(it => it.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);

            // Act
            await target.Import(serviceScope.Object, dateTime);

            // Assert
            intervalTrigger.Verify(it => it.IsTriggerTime(dateTime));
            serviceProvider.Verify(sp => sp.GetService(typeof(IEnergiDataServiceImporter)), Times.Never);
            intervalTrigger.Verify(it => it.Advance(dateTime), Times.Never);
        }

        private EnergiDataServiceImport CreateTarget()
        {
            return new EnergiDataServiceImport(new NullLogger<EnergiDataServiceImport>(), intervalTrigger.Object);
        }
    }
}
