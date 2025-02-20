using Microsoft.Data.Sqlite;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DataStoreExceptionFactoryTest
  {
    [Test]
    public void CreateDataStoreExceptionFromNull()
    {
      // Arrange

      // Act
      var dse = DataStoreExceptionFactory.Create(null);

      // Assert
      Assert.That(dse.GetType(), Is.EqualTo(typeof(DataStoreException)));
      Assert.That(dse.InnerException, Is.Null);
    }

    [Test]
    public void CreateDataStoreExceptionWithMessage()
    {
      // Arrange
      const string message = "non default exception message!!";

      // Act
      var dse = DataStoreExceptionFactory.Create(null, message);

      // Assert
      Assert.That(dse.Message, Is.EqualTo(message));
    }

    [Test]
    public void CreateDataStoreException()
    {
      // Arrange
      var sqliteException = new SqliteException("Ehh", 44); // Something unspecified..

      // Act
      var dse = DataStoreExceptionFactory.Create(sqliteException);

      // Assert
      Assert.That(dse.GetType(), Is.EqualTo(typeof(DataStoreException)));
      Assert.That(dse.InnerException, Is.SameAs(sqliteException));
    }

    [Test]
    public void CreateDataStoreBusyException()
    {
      // Arrange
      var sqliteException = new SqliteException("Stuff", 5);

      // Act
      var dse = DataStoreExceptionFactory.Create(sqliteException);

      // Assert
      Assert.That(dse.GetType(), Is.EqualTo(typeof(DataStoreBusyException)));
      Assert.That(dse.InnerException, Is.SameAs(sqliteException));
    }

    [Test]
    public void CreateDataStoreCorruptException()
    {
      // Arrange
      var sqliteException = new SqliteException("stuff", 11);

      // Act
      var dse = DataStoreExceptionFactory.Create(sqliteException);

      // Assert
      Assert.That(dse.GetType(), Is.EqualTo(typeof(DataStoreCorruptException)));
      Assert.That(dse.InnerException, Is.SameAs(sqliteException));
    }

    [Test]
    public void CreateDataStoreUniqueConstraintExceptionFromPrimaryKey()
    {
      // Arrange
      var sqliteException = new SqliteException("stuff", 19, 1555);

      // Act
      var dse = DataStoreExceptionFactory.Create(sqliteException);

      // Assert
      Assert.That(dse.GetType(), Is.EqualTo(typeof(DataStoreUniqueConstraintException)));
      Assert.That(dse.InnerException, Is.SameAs(sqliteException));
    }

    [Test]
    public void CreateDataStoreUniqueConstraintExceptionFromIndex()
    {
      // Arrange
      var sqliteException = new SqliteException("stuff", 19, 2067);

      // Act
      var dse = DataStoreExceptionFactory.Create(sqliteException);

      // Assert
      Assert.That(dse.GetType(), Is.EqualTo(typeof(DataStoreUniqueConstraintException)));
      Assert.That(dse.InnerException, Is.SameAs(sqliteException));
    }

  }
}

