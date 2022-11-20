using System;
using System.Data;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class DbContextTest : DbTestFixturePlain
    {
        private DbContext target;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            target = new DbContext(Connection);
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new DbContext(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Properties()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(target.Connection, Is.SameAs(Connection));
        }

        [Test]
        public void GetDateTime()
        {
            // Arrange
            const long unixTimestamp = 1472502100;

            // Act
            var dateTime = target.GetDateTime(unixTimestamp);

            // Assert
            Assert.That(dateTime, Is.EqualTo(new DateTime(2016, 08, 29, 20, 21, 40)));
            Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void QueryTransaction()
        {
            // Arrange

            // Act
            var version = target.QueryTransaction<string>("SELECT sqlite_version();");

            // Assert
            Assert.That(version.Count, Is.EqualTo(1));
        }

        [Test]
        public void DisposeDisposes()
        {
            // Arrange

            // Act
            target.Dispose();

            // Assert
            Assert.That(Connection.State, Is.EqualTo(ConnectionState.Closed));
        }
    }
}
