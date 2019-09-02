using System;
using System.Collections.Generic;
using NUnit.Framework;
using PowerView.Model.Expression;
using Moq;

namespace PowerView.Model.Test.Expression
{
  [TestFixture]
  public class OperationTemplateExpressionTest
  {
    [Test]
    public void ConstructorThrows() 
    {
      // Arrange
      var templateExpression = new Mock<ITemplateExpression>();
      const string op = "+";

      // Act & Assert
      Assert.That(() => new OperationTemplateExpression(null, op, templateExpression.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new OperationTemplateExpression(templateExpression.Object, "", templateExpression.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new OperationTemplateExpression(templateExpression.Object, null, templateExpression.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new OperationTemplateExpression(templateExpression.Object, op, null), Throws.TypeOf<ArgumentNullException>());

      Assert.That(() => new OperationTemplateExpression(templateExpression.Object, "_", templateExpression.Object), Throws.TypeOf<TemplateExpressionException>());
    }

    [Test]
    public void ConstructorAndProperties() 
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "+";

      // Act
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);

      // Assert
      Assert.That(target.Left, Is.SameAs(templateExpressionLeft.Object));
      Assert.That(target.Operator, Is.EqualTo(op));
      Assert.That(target.Right, Is.SameAs(templateExpressionRight.Object));
    }

    [Test]
    public void IsSatisfied() 
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "+";
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);
      var labelsAndObisCodes = new Dictionary<string, ICollection<ObisCode>>();
      templateExpressionLeft.Setup(te => te.IsSatisfied(labelsAndObisCodes)).Returns(true);
      templateExpressionRight.Setup(te => te.IsSatisfied(labelsAndObisCodes)).Returns(true);

      // Act
      var isSatisfied = target.IsSatisfied(labelsAndObisCodes);

      // Assert
      Assert.That(isSatisfied, Is.True);
      templateExpressionLeft.VerifyAll();
      templateExpressionRight.VerifyAll();
    }

    [Test]
    public void IsSatisfiedFalse() 
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "+";
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);
      var labelsAndObisCodes = new Dictionary<string, ICollection<ObisCode>>();
      templateExpressionLeft.Setup(te => te.IsSatisfied(labelsAndObisCodes)).Returns(true);
      templateExpressionRight.Setup(te => te.IsSatisfied(labelsAndObisCodes)).Returns(false);

      // Act
      var isSatisfied = target.IsSatisfied(labelsAndObisCodes);

      // Assert
      Assert.That(isSatisfied, Is.False);
    }

    [Test]
    public void GetValueExpressionSetAddOld()
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "+";
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);
      var labelProfileSet = new LabelProfileSet(DateTime.UtcNow, new LabelProfile[0]);
      Func<DateTime, DateTime> timeDivider = dt => new DateTime(dt.Year, 1, 1, 1, 1, 1, dt.Kind);
      var valueExpressionSet = new Mock<IValueExpressionSet>();
      templateExpressionLeft.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelProfileSet>(), It.IsAny<Func<DateTime,DateTime>>())).Returns(valueExpressionSet.Object);
      templateExpressionRight.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelProfileSet>(), It.IsAny<Func<DateTime, DateTime>>())).Returns(valueExpressionSet.Object);

      // Act
      var valExprSet = target.GetValueExpressionSet(labelProfileSet, timeDivider);

      // Assert
      Assert.That(valExprSet, Is.TypeOf<AddValueExpressionSet>());
      templateExpressionLeft.Verify(te => te.GetValueExpressionSet(labelProfileSet, timeDivider));
      templateExpressionRight.Verify(te => te.GetValueExpressionSet(labelProfileSet, timeDivider));
    }

    [Test]
    public void GetValueExpressionSetSubtractOld()
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "-";
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);
      var labelProfileSet = new LabelProfileSet(DateTime.UtcNow, new LabelProfile[0]);
      Func<DateTime, DateTime> timeDivider = dt => new DateTime(dt.Year, 1, 1, 1, 1, 1, dt.Kind);
      var valueExpressionSet = new Mock<IValueExpressionSet>();
      templateExpressionLeft.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelProfileSet>(), It.IsAny<Func<DateTime, DateTime>>())).Returns(valueExpressionSet.Object);
      templateExpressionRight.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelProfileSet>(), It.IsAny<Func<DateTime, DateTime>>())).Returns(valueExpressionSet.Object);

      // Act
      var valExprSet = target.GetValueExpressionSet(labelProfileSet, timeDivider);

      // Assert
      Assert.That(valExprSet, Is.TypeOf<SubtractValueExpressionSet>());
      templateExpressionLeft.Verify(te => te.GetValueExpressionSet(labelProfileSet, timeDivider));
      templateExpressionRight.Verify(te => te.GetValueExpressionSet(labelProfileSet, timeDivider));
    }

    [Test]
    public void GetValueExpressionSetAdd()
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "+";
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(start, start.AddMonths(11), new LabelSeries<NormalizedTimeRegisterValue>[0]);
      var valueExpressionSet = new Mock<IValueExpressionSet>();
      templateExpressionLeft.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelSeriesSet<NormalizedTimeRegisterValue>>())).Returns(valueExpressionSet.Object);
      templateExpressionRight.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelSeriesSet<NormalizedTimeRegisterValue>>())).Returns(valueExpressionSet.Object);

      // Act
      var valExprSet = target.GetValueExpressionSet(labelSeriesSet);

      // Assert
      Assert.That(valExprSet, Is.TypeOf<AddValueExpressionSet>());
      templateExpressionLeft.Verify(te => te.GetValueExpressionSet(labelSeriesSet));
      templateExpressionRight.Verify(te => te.GetValueExpressionSet(labelSeriesSet));
    }

    [Test]
    public void GetValueExpressionSetSubtract()
    {
      // Arrange
      var templateExpressionLeft = new Mock<ITemplateExpression>();
      var templateExpressionRight = new Mock<ITemplateExpression>();
      const string op = "-";
      var target = new OperationTemplateExpression(templateExpressionLeft.Object, op, templateExpressionRight.Object);
      var start = new DateTime(2017, 6, 7, 8, 9, 10, DateTimeKind.Utc);
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(start, start.AddMonths(11), new LabelSeries<NormalizedTimeRegisterValue>[0]);
      var valueExpressionSet = new Mock<IValueExpressionSet>();
      templateExpressionLeft.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelSeriesSet<NormalizedTimeRegisterValue>>())).Returns(valueExpressionSet.Object);
      templateExpressionRight.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelSeriesSet<NormalizedTimeRegisterValue>>())).Returns(valueExpressionSet.Object);

      // Act
      var valExprSet = target.GetValueExpressionSet(labelSeriesSet);

      // Assert
      Assert.That(valExprSet, Is.TypeOf<SubtractValueExpressionSet>());
      templateExpressionLeft.Verify(te => te.GetValueExpressionSet(labelSeriesSet));
      templateExpressionRight.Verify(te => te.GetValueExpressionSet(labelSeriesSet));
    }

  }
}

