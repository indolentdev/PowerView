using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class EnvironmentRepositoryTest : DbTestFixture
    {
        [Test]
        public void GetSqliteVersion()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            var version = target.GetSqliteVersion();

            // Assert
            Assert.That(version, Is.Not.Null);
        }


        private EnvironmentRepository CreateTarget()
        {
            return new EnvironmentRepository(DbContext);
        }

    }
}
