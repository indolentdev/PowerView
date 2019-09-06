using System;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test.Expression
{
  [TestFixture]
  public class SubtractValueExpressionSetTest
  {
    [Test]
    public void ConstructorThrows() 
    {
      // Arrange
      var ves = new ValueExpressionSet(new NormalizedTimeRegisterValue[0]);

      // Act & Assert
      Assert.That(() => new SubtractValueExpressionSet(null, ves), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SubtractValueExpressionSet(ves, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    [TestCase(150, 100, 50)]
    [TestCase(100, 150, -50)]
    public void EvaluateCalculation(double v1, double v2, double expected)
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", new DateTime(2016, 1, 22, 21, 00, 00, DateTimeKind.Utc), v1, Unit.WattHour), utcNow);
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("2", new DateTime(2016, 1, 22, 23, 00, 00, DateTimeKind.Utc), v2, Unit.WattHour), utcNow);
      var target = new SubtractValueExpressionSet(new ValueExpressionSet(new[] { trv1 }), new ValueExpressionSet(new[] { trv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values, Is.EqualTo(new[] { new NormalizedTimeRegisterValue(new TimeRegisterValue("0", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), expected, Unit.WattHour), utcNow) }));
    }

    [Test]
    public void EvaluateNoMatching()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), 150, Unit.WattHour), utcNow);
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("2", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), 100, Unit.WattHour), utcNow.AddHours(4));
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { trv1 }), new ValueExpressionSet(new[] { trv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values, Is.Empty);
    }

    [Test]
    public void EvaluateMatchingMultiple()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), 100, Unit.WattHour), utcNow);
      var trv2 = new NormalizedTimeRegisterValue(new  TimeRegisterValue("2", new DateTime(2016, 1, 22, 22, 10, 00, DateTimeKind.Utc), 150, Unit.WattHour), utcNow.AddHours(1));
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { trv1, trv2 }), new ValueExpressionSet(new[] { trv1, trv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values.Count, Is.EqualTo(2));
    }

  }
}

