using System;
using System.Data;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DbContextTest
  {
    private Mock<IDbConnection> connection;
    private DbContext target;

    [SetUp]
    public void SetUp()
    {
      connection = new Mock<IDbConnection>();

      target = new DbContext(connection.Object);
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new DbContext(null), Throws.TypeOf<ArgumentNullException>());

      // Assert
      Assert.That(target.Connection, Is.SameAs(connection.Object));
    }

    [Test]
    public void Properties()
    {
      // Arrange

      // Act

      // Assert
      Assert.That(target.Connection, Is.SameAs(connection.Object));
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
    public void DisposeDisposes()
    {
      // Arrange

      // Act
      target.Dispose();

      // Assert
      connection.Verify(c => c.Close());
      connection.Verify(c => c.Dispose());
    }
  }
}
