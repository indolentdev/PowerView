using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class LabelsSeriesFromTemplatesGeneratorTest
  {
    [Test]
    public void GenerateFromTemplates()
    {
      // Arrange
      var baseTime = DateTime.UtcNow;
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(baseTime, baseTime + TimeSpan.FromMinutes(30), new LabelSeries<NormalizedTimeRegisterValue>[0]);
      var templateExpression = new Mock<ITemplateExpression>();
      ObisCode obisCode = "1.2.3.4.5.255";
      var labelObisCodeTemplates = new[] { new LabelObisCodeTemplate("NewLabel", new[] { new ObisCodeTemplate(obisCode, templateExpression.Object) }) };
      templateExpression.Setup(te => te.IsSatisfied(It.IsAny<IDictionary<string, ICollection<ObisCode>>>())).Returns(true);
      var valueExpressionSet = new Mock<IValueExpressionSet>();
      templateExpression.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelSeriesSet<NormalizedTimeRegisterValue>>())).Returns(valueExpressionSet.Object);
      var ntrv1 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", baseTime.AddMinutes(1), 7, Unit.WattHour), baseTime);
      var ntrv2 = new NormalizedTimeRegisterValue(new TimeRegisterValue("1", baseTime.AddMinutes(20), 9, Unit.WattHour), baseTime.AddMinutes(10));
      valueExpressionSet.Setup(sve => sve.Evaluate2()).Returns(new[] { ntrv2, ntrv1 });
      var target = new LabelSeriesFromTemplatesGenerator(labelObisCodeTemplates);

      // Act
      var result = target.Generate(labelSeriesSet);

      // Assert
      templateExpression.Verify(te => te.GetValueExpressionSet(It.Is<LabelSeriesSet<NormalizedTimeRegisterValue>>(x => x == labelSeriesSet)));
      Assert.That(result.Count, Is.EqualTo(1));
      var newLabelSeries = result.First();
      Assert.That(newLabelSeries.Label, Is.EqualTo("NewLabel"));
      Assert.That(newLabelSeries.ToArray(), Is.EqualTo(new[] { obisCode }));
      Assert.That(newLabelSeries[obisCode].Count(), Is.EqualTo(2));
      Assert.That(newLabelSeries[obisCode], Is.EqualTo(new[] { ntrv1, ntrv2 }));
    }

    [Test]
    public void GenerateFromTemplatesIsSatisfiedFalse()
    {
      // Arrange
      var baseTime = DateTime.UtcNow;
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(baseTime, baseTime + TimeSpan.FromMinutes(30), new LabelSeries<NormalizedTimeRegisterValue>[0]);
      var templateExpression = new Mock<ITemplateExpression>();
      var labelObisCodeTemplates = new[] { new LabelObisCodeTemplate("NewLabel", new[] { new ObisCodeTemplate("1.100.1.8.0.255", templateExpression.Object) }) };
      templateExpression.Setup(te => te.IsSatisfied(It.IsAny<IDictionary<string, ICollection<ObisCode>>>())).Returns(false);
      var target = new LabelSeriesFromTemplatesGenerator(labelObisCodeTemplates);

      // Act
      var result = target.Generate(labelSeriesSet);

      // Assert
      Assert.That(result, Is.Empty);
    }

    [Test]
    public void GenerateFromTemplatesGetValueExpressionSetThrows()
    {
      // Arrange
      var baseTime = DateTime.UtcNow;
      var labelSeriesSet = new LabelSeriesSet<NormalizedTimeRegisterValue>(baseTime, baseTime + TimeSpan.FromMinutes(30), new LabelSeries<NormalizedTimeRegisterValue>[0]);
      var templateExpression = new Mock<ITemplateExpression>();
      var labelObisCodeTemplates = new[] { new LabelObisCodeTemplate("NewLabel", new[] { new ObisCodeTemplate("1.100.1.8.0.255", templateExpression.Object) }) };
      templateExpression.Setup(te => te.IsSatisfied(It.IsAny<IDictionary<string, ICollection<ObisCode>>>())).Returns(true);
      templateExpression.Setup(te => te.GetValueExpressionSet(It.IsAny<LabelSeriesSet<NormalizedTimeRegisterValue>>())).Throws(new ValueExpressionSetException());
      var target = new LabelSeriesFromTemplatesGenerator(labelObisCodeTemplates);

      // Act
      var result = target.Generate(labelSeriesSet);

      // Assert
      Assert.That(result, Is.Empty);
    }

  }
}
