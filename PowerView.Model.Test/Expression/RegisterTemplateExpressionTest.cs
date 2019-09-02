using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test.Expression
{
  [TestFixture]
  public class RegisterTemplateExpressionTest
  {
    [Test]
    public void ConstructorThrows() 
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new RegisterTemplateExpression(null), Throws.TypeOf<ArgumentNullException>());

      Assert.That(() => new RegisterTemplateExpression("AbsentColon"), Throws.TypeOf<TemplateExpressionException>());
      Assert.That(() => new RegisterTemplateExpression("Excessive:Colons:"), Throws.TypeOf<TemplateExpressionException>());
      Assert.That(() => new RegisterTemplateExpression("BadObisCode:1.2.3.4.5"), Throws.TypeOf<TemplateExpressionException>());
    }

    [Test]
    public void ConstructorAndProperties() 
    {
      // Arrange
      const string label = "MyLabel";
      ObisCode obisCode = "1.2.3.4.5.6";

      // Act
      var target = new RegisterTemplateExpression(label + ":" + obisCode);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
      Assert.That(target.ObisCode, Is.EqualTo(obisCode));
    }

    [Test]
    public void IsSatisfied() 
    {
      // Arrange
      var target = new RegisterTemplateExpression("MyLabel:1.2.3.4.5.6");
      var labelAndObisCodes = new Dictionary<string, ICollection<ObisCode>> 
      {
        { "MyLabel".ToLowerInvariant(), new [] { (ObisCode)"1.2.3.4.5.6" } }
      };

      // Act
      var isSatisfied = target.IsSatisfied(labelAndObisCodes);

      // Assert
      Assert.That(isSatisfied, Is.True);
    }

    [Test]
    public void IsSatisfiedWrongLabel() 
    {
      // Arrange
      var target = new RegisterTemplateExpression("MyLabel:1.2.3.4.5.6");
      var labelAndObisCodes = new Dictionary<string, ICollection<ObisCode>> 
      {
        { "MyLabel", new [] { (ObisCode)"1.2.3.4.5.6" } }
      };

      // Act
      var isSatisfied = target.IsSatisfied(labelAndObisCodes);

      // Assert
      Assert.That(isSatisfied, Is.False);
    }

    [Test]
    public void IsSatisfiedWrongObisCode() 
    {
      // Arrange
      var target = new RegisterTemplateExpression("MyLabel:1.2.3.4.5.6");
      var labelAndObisCodes = new Dictionary<string, ICollection<ObisCode>> 
      {
        { "MyLabel".ToLowerInvariant(), new [] { (ObisCode)"255.2.3.4.5.6" } }
      };

      // Act
      var isSatisfied = target.IsSatisfied(labelAndObisCodes);

      // Assert
      Assert.That(isSatisfied, Is.False);
    }

    [Test]
    public void GetValueExpressionSetOld()
    {
      // Arrange
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var trv1 = new TimeRegisterValue("1", start, 1, Unit.Watt);
      var trv2 = new TimeRegisterValue("1", start.AddMonths(4), 2, Unit.Watt);
      var trv3 = new TimeRegisterValue("1", start.AddMonths(9), 3, Unit.Watt);
      var labelProfile = new LabelProfile("MyLabel", start, new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { { "1.2.3.4.5.6", new[] { trv1, trv2, trv3 } } });
      var labelProfileSet = new LabelProfileSet(start, new[] { labelProfile });
      Func<DateTime, DateTime> timeDivider = dt => new DateTime(dt.Year, 1, 1, 1, 1, 1, dt.Kind);
      var target = new RegisterTemplateExpression("MyLabel:1.2.3.4.5.6");

      // Act
      var valueExpressionSet = target.GetValueExpressionSet(labelProfileSet, timeDivider);

      // Assert
      Assert.That(valueExpressionSet.Evaluate().Count, Is.EqualTo(2));
      Assert.That(valueExpressionSet.Evaluate().First(), Is.EqualTo(new CoarseTimeRegisterValue(timeDivider(start), trv2)));
      Assert.That(valueExpressionSet.Evaluate().Last(), Is.EqualTo(new CoarseTimeRegisterValue(timeDivider(start.AddMonths(9)), trv3)));
    }

    [Test]
    public void GetValueExpressionSetWrongLabelOld()
    {
      // Arrange
      var start = DateTime.UtcNow;
      var timeRegisterValue = new TimeRegisterValue("1", start, 1, Unit.Watt);
      var labelProfile = new LabelProfile("MyLabel", start, new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { { "1.2.3.4.5.6", new[] { timeRegisterValue } } });
      var labelProfileSet = new LabelProfileSet(start, new[] { labelProfile });
      Func<DateTime, DateTime> timeDivider = dt => new DateTime(dt.Year, 1, 1, 1, 1, 1, dt.Kind);
      var target = new RegisterTemplateExpression("WrongLabel:1.2.3.4.5.6");

      // Act & Assert
      Assert.That(() => target.GetValueExpressionSet(labelProfileSet, timeDivider), Throws.TypeOf<ValueExpressionSetException>());
    }

    [Test]
    public void GetValueExpressionSetWrongObisCodeOld()
    {
      // Arrange
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var trv1 = new TimeRegisterValue("1", start, 1, Unit.Watt);
      var labelProfile = new LabelProfile("MyLabel", start, new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { { "1.2.3.4.5.6", new[] { trv1 } } });
      var labelProfileSet = new LabelProfileSet(start, new[] { labelProfile });
      Func<DateTime, DateTime> timeDivider = dt => new DateTime(dt.Year, 1, 1, 1, 1, 1, dt.Kind);
      var target = new RegisterTemplateExpression("MyLabel:255.2.3.4.5.6");

      // Act
      var valueExpressionSet = target.GetValueExpressionSet(labelProfileSet, timeDivider);

      // Assert
      Assert.That(valueExpressionSet.Evaluate().Count, Is.EqualTo(0));
    }

    [Test]
    public void GetValueExpressionSet()
    {
      // Arrange
      var timeDivider = DateTimeResolutionDivider.GetResolutionDivider("1-days");
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start, 1, Unit.Watt), timeDivider(start));
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start.AddMonths(4), 2, Unit.Watt), timeDivider(start.AddMonths(4)));
      var trv3 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start.AddMonths(9), 3, Unit.Watt), timeDivider(start.AddMonths(9)));
      var normalizedimeRegisterValues = new[] { trv1, trv2, trv3 };
      var labelSeries = new LabelSeries<NormalizedTimeRegisterValue>("MyLabel", new Dictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>> { { "1.2.3.4.5.6", normalizedimeRegisterValues } });
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(start, start.AddMonths(11), new[] { labelSeries });
      var target = new RegisterTemplateExpression("MyLabel:1.2.3.4.5.6");

      // Act
      var valueExpressionSet = target.GetValueExpressionSet(labelSeriesSet);

      // Assert
      Assert.That(valueExpressionSet.Evaluate2(), Is.EqualTo(normalizedimeRegisterValues));
    }

    [Test]
    public void GetValueExpressionSetWrongLabel()
    {
      // Arrange
      var timeDivider = DateTimeResolutionDivider.GetResolutionDivider("1-days");
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start, 1, Unit.Watt), timeDivider(start));
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start.AddMonths(4), 2, Unit.Watt), timeDivider(start.AddMonths(4)));
      var trv3 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start.AddMonths(9), 3, Unit.Watt), timeDivider(start.AddMonths(9)));
      var normalizedimeRegisterValues = new[] { trv1, trv2, trv3 };
      var labelSeries = new LabelSeries<NormalizedTimeRegisterValue>("MyLabel", new Dictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>> { { "1.2.3.4.5.6", normalizedimeRegisterValues } });
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(start, start.AddMonths(11), new[] { labelSeries });
      var target = new RegisterTemplateExpression("WrongLabel:1.2.3.4.5.6");

      // Act & Assert
      Assert.That(() => target.GetValueExpressionSet(labelSeriesSet), Throws.TypeOf<ValueExpressionSetException>());
    }

    [Test]
    public void GetValueExpressionSetWrongObisCode()
    {
      // Arrange
      var timeDivider = DateTimeResolutionDivider.GetResolutionDivider("1-days");
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var trv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start, 1, Unit.Watt), timeDivider(start));
      var trv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start.AddMonths(4), 2, Unit.Watt), timeDivider(start.AddMonths(4)));
      var trv3 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", start.AddMonths(9), 3, Unit.Watt), timeDivider(start.AddMonths(9)));
      var normalizedimeRegisterValues = new[] { trv1, trv2, trv3 };
      var labelSeries = new LabelSeries<NormalizedTimeRegisterValue>("MyLabel", new Dictionary<ObisCode, IEnumerable<NormalizedTimeRegisterValue>> { { "1.2.3.4.5.6", normalizedimeRegisterValues } });
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(start, start.AddMonths(11), new[] { labelSeries });
      var target = new RegisterTemplateExpression("MyLabel:255.2.3.4.5.6");

      // Act
      var valueExpressionSet = target.GetValueExpressionSet(labelSeriesSet);

      // Assert
      Assert.That(valueExpressionSet.Evaluate2(), Is.Empty);
    }

  }
}
