using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class LabelRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new LabelRepository(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetLablesByTimestampEmpty()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            var labels = target.GetLabelsByTimestamp();

            // Assert
            Assert.That(labels, Is.Empty);
        }

        [Test]
        public void GetLablesByTimestamp()
        {
            // Arrange
            var reading1 = new Db.LiveReading { LabelId = 1, DeviceId = 1, Timestamp = DateTime.UtcNow };
            var reading2 = new Db.LiveReading { LabelId = 2, DeviceId = 1, Timestamp = DateTime.UtcNow + TimeSpan.FromSeconds(2) };
            var reading3 = new Db.LiveReading { LabelId = 3, DeviceId = 1, Timestamp = DateTime.UtcNow + TimeSpan.FromSeconds(4) };
            var insert = DbContext.InsertReadings(reading1, reading2, reading3);
            var target = CreateTarget();

            // Act
            var labels = target.GetLabelsByTimestamp();

            // Assert
            Assert.That(labels, Is.EquivalentTo(insert.Labels.Reverse().ToList()));
        }

        private LabelRepository CreateTarget()
        {
            return new LabelRepository(DbContext);
        }

    }
}
