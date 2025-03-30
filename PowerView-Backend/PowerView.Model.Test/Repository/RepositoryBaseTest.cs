using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    public class RepositoryBaseTest : DbTestFixture
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var wrongDbContext = new Moq.Mock<IDbContext>();

            // Act & Assert
            Assert.That(() => new RepositoryBase(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new RepositoryBase(wrongDbContext.Object), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange

            // Act 
            var target = CreateTarget();

            // Assert
            Assert.That(target.DbContext, Is.SameAs(DbContext));
        }

        private RepositoryBase CreateTarget()
        {
            return new RepositoryBase(DbContext);
        }

    }
}

