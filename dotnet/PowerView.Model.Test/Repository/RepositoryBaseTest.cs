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

    [Test]
    public void GetPageCount()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(target.GetPageCount(0), Is.EqualTo(50));
      Assert.That(target.GetPageCount(101), Is.EqualTo(100));
      Assert.That(target.GetPageCount(66), Is.EqualTo(66));
    }

    private RepositoryBase CreateTarget()
    {
      return new RepositoryBase(DbContext);
    }

  }
}

