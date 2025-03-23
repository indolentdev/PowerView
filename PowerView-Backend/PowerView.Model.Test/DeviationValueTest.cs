using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DeviationValueTest
  {
    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange

      // Act
      var target = new DeviationValue(10000, 2, 20);

      // Assert
      Assert.That(target.Value, Is.EqualTo(10000));
      Assert.That(target.DurationBasedDeviationMinValue, Is.EqualTo(2));
      Assert.That(target.DurationBasedDeviationMaxValue, Is.EqualTo(20));
    }

    [Test]
    public void ToStringTest()
    {
      // Arrange

      // Act
      var target = new DeviationValue(100, 2, 20);

      // Assert
      Assert.That(target.ToString(), Is.EqualTo("[Value=100, DurationBasedDeviationMinValue=2, DurationBasedDeviationMaxValue=20]"));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var t1 = new DeviationValue(1, 2, 20);
      var t2 = new DeviationValue(1, 2, 20);
      var t3 = new DeviationValue(2, 2, 20);
      var t4 = new DeviationValue(1, 1, 20);
      var t5 = new DeviationValue(1, 2, 21);

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1, Is.EqualTo((object)t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t4));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t5));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t5.GetHashCode()));
    }

    [Test]
    public void Equeality()
    {
      // Arrange
      var t1 = new DeviationValue(1, 2, 20);
      var t2 = new DeviationValue(1, 2, 20);
      var t3 = new DeviationValue(2, 2, 20);
      var t4 = new DeviationValue(1, 1, 20);
      var t5 = new DeviationValue(1, 2, 21);

      // Act & Assert
      Assert.That(t1 == t2, Is.True);
      Assert.That(t1 == t3, Is.False);
      Assert.That(t1 == t4, Is.False);
      Assert.That(t1 == t5, Is.False);
    }

  }
}

