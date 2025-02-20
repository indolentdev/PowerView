using System;
using NUnit.Framework;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Mappers
{
  [TestFixture]
  public class DateTimeMapperTest
  {
    [Test]
    public void MapThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => DateTimeMapper.Map(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Map()
    {
      // Arrange
      var dt = new DateTime(2015, 12, 25, 18, 55, 12, 1, DateTimeKind.Utc);

      // Act
      var timeString = DateTimeMapper.Map(dt);

      // Assert
      Assert.That(timeString, Is.EqualTo("2015-12-25T18:55:12Z"));
    }

    [Test]
    public void MapNullableThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => DateTimeMapper.Map((DateTime?)DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void MapNullable()
    {
      // Arrange
      DateTime? dt = new DateTime(2015, 12, 25, 18, 55, 12, 1, DateTimeKind.Utc);

      // Act
      var timeString = DateTimeMapper.Map(dt);

      // Assert
      Assert.That(timeString, Is.EqualTo("2015-12-25T18:55:12Z"));
    }

    [Test]
    public void MapNullableNull()
    {
      // Arrange
      DateTime? dt = null;

      // Act
      var timeString = DateTimeMapper.Map(dt);

      // Assert
      Assert.That(timeString, Is.Null);
    }

  }
}

