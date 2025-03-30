using System;
using System.Data;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class DbContextFactoryTest : DbTestFixture
    {
        [Test]
        public void CreateContext()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(DbContext.Connection.State, Is.EqualTo(ConnectionState.Open));
        }
    }
}

