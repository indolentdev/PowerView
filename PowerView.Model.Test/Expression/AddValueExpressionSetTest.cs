﻿using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test.Expression
{
  [TestFixture]
  public class AddValueExpressionSetTest
  {
    [Test]
    public void ConstructorThrows() 
    {
      // Arrange
      var ves = new ValueExpressionSet(new CoarseTimeRegisterValue[0]);

      // Act & Assert
      Assert.That(() => new AddValueExpressionSet(null, ves), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new AddValueExpressionSet(ves, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void EvaluateCalculationOld()
    {
      // Arrange
      var cdt = new DateTime(2016, 1, 22, 00, 00, 00, DateTimeKind.Utc);
      var trv1 = new TimeRegisterValue("1", new DateTime(2016, 1, 22, 21, 45, 00, DateTimeKind.Utc), 100, Unit.WattHour);
      var ctrv1 = new CoarseTimeRegisterValue(cdt, trv1);
      var trv2 = new TimeRegisterValue("2", new DateTime(2016, 1, 22, 22, 15, 00, DateTimeKind.Utc), 150, Unit.WattHour);
      var ctrv2 = new CoarseTimeRegisterValue(cdt, trv2);
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { ctrv1 }), new ValueExpressionSet(new[] { ctrv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values.Count, Is.EqualTo(1));
      var exp = new CoarseTimeRegisterValue(cdt, new TimeRegisterValue("0", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), 250, Unit.WattHour));
      Assert.That(values.First(), Is.EqualTo(exp));
    }

    [Test]
    public void EvaluateNoMatchingCoarseTimeOld()
    {
      // Arrange
      var cdt = new DateTime(2016, 1, 22, 00, 00, 00, DateTimeKind.Utc);
      var trv1 = new TimeRegisterValue("1", new DateTime(2016, 1, 22, 21, 45, 00, DateTimeKind.Utc), 100, Unit.WattHour);
      var ctrv1 = new CoarseTimeRegisterValue(cdt, trv1);
      var trv2 = new TimeRegisterValue("1", new DateTime(2016, 1, 22, 22, 15, 00, DateTimeKind.Utc), 150, Unit.WattHour);
      var ctrv2 = new CoarseTimeRegisterValue(cdt.AddDays(1), trv2);
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { ctrv1 }), new ValueExpressionSet(new[] { ctrv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values.Count, Is.EqualTo(0));
    }

    [Test]
    public void EvaluateNoMatchingOld()
    {
      // Arrange
      var cdt = new DateTime(2016, 1, 22, 00, 00, 00, DateTimeKind.Utc);
      var trv1 = new TimeRegisterValue("1", new DateTime(2016, 1, 22, 21, 45, 00, DateTimeKind.Utc), 100, Unit.WattHour);
      var ctrv1 = new CoarseTimeRegisterValue(cdt, trv1);
      var trv2 = new TimeRegisterValue("1", new DateTime(2016, 1, 22, 22, 15, 00, DateTimeKind.Utc), 150, Unit.Joule);
      var ctrv2 = new CoarseTimeRegisterValue(cdt, trv2);
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { ctrv1 }), new ValueExpressionSet(new[] { ctrv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values.Count, Is.EqualTo(0));
    }

    [Test]
    public void EvaluateMultipleEntriesOld()
    {
      // Arrange
      var cdt1 = new DateTime(2016, 1, 22, 00, 00, 00, DateTimeKind.Utc);
      var trv1 = new TimeRegisterValue("1", new DateTime(2016, 1, 22, 21, 45, 00, DateTimeKind.Utc), 100, Unit.WattHour);
      var ctrv1 = new CoarseTimeRegisterValue(cdt1, trv1);
      var cdt2 = new DateTime(2016, 2, 23, 00, 00, 00, DateTimeKind.Utc);
      var trv2 = new TimeRegisterValue("1", new DateTime(2016, 2, 23, 22, 15, 00, DateTimeKind.Utc), 150, Unit.WattHour);
      var ctrv2 = new CoarseTimeRegisterValue(cdt2, trv2);
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { ctrv1, ctrv2 }), new ValueExpressionSet(new[] { ctrv1, ctrv2 }));

      // Act
      var values = target.Evaluate();

      // Assert
      Assert.That(values.Count, Is.EqualTo(2));
    }

    [Test]
    public void EvaluateCalculation()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", new DateTime(2016, 1, 22, 21, 00, 00, DateTimeKind.Utc), 100, Unit.WattHour), utcNow);
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("2", new DateTime(2016, 1, 22, 23, 00, 00, DateTimeKind.Utc), 150, Unit.WattHour), utcNow);
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { trv1 }), new ValueExpressionSet(new[] { trv2 }));

      // Act
      var values = target.Evaluate2();

      // Assert
      Assert.That(values, Is.EqualTo(new[] { new NormalizedTimeRegisterValue(new TimeRegisterValue("0", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), 250, Unit.WattHour), utcNow) }));
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
      var values = target.Evaluate2();

      // Assert
      Assert.That(values, Is.Empty);
    }

    [Test]
    public void EvaluateMatchingMultiple()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", new DateTime(2016, 1, 22, 22, 00, 00, DateTimeKind.Utc), 100, Unit.WattHour), utcNow);
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("2", new DateTime(2016, 1, 22, 22, 10, 00, DateTimeKind.Utc), 150, Unit.WattHour), utcNow.AddHours(1));
      var target = new AddValueExpressionSet(new ValueExpressionSet(new[] { trv1, trv2 }), new ValueExpressionSet(new[] { trv1, trv2 }));

      // Act
      var values = target.Evaluate2();

      // Assert
      Assert.That(values.Count, Is.EqualTo(2));
    }

  }
}

