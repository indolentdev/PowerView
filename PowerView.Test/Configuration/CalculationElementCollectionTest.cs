using System.Linq;
using PowerView.Configuration;
using PowerView.Model;
using NUnit.Framework;


namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class CalculationElementCollectionTest
  {
    [Test]
    public void GetLabelObisCodeTemplates()
    {
      // Arrange
      var target = new CalculationElementCollection();
      target.Add(new CalculationElement{ Label="Label1", ObisCode="1.2.3.4.5.6", Template="A:1.1.1.1.1.1" });
      target.Add(new CalculationElement{ Label="Label1", ObisCode="6.5.4.3.2.1", Template="A:1.1.1.1.1.1" });
      target.Add(new CalculationElement{ Label="Label2", ObisCode="1.2.3.4.5.7", Template="A:1.1.1.1.1.1" });

      // Act
      var labelObisCodeTemplates = target.GetLabelObisCodeTemplates();

      // Assert
      Assert.That(labelObisCodeTemplates.Count, Is.EqualTo(2));
      var label1 = labelObisCodeTemplates.FirstOrDefault(x => x.Label == "Label1");
      Assert.That(label1, Is.Not.Null);
      Assert.That(label1.ObisCodeTemplates.Count, Is.EqualTo(2));
      Assert.That(label1.ObisCodeTemplates.Select(x => x.ObisCode), Is.EqualTo(new ObisCode[] { "1.2.3.4.5.6", "6.5.4.3.2.1" } ));
      var label2 = labelObisCodeTemplates.FirstOrDefault(x => x.Label == "Label2");
      Assert.That(label2, Is.Not.Null);
      Assert.That(label2.ObisCodeTemplates.Count, Is.EqualTo(1));
      Assert.That(label2.ObisCodeTemplates.Select(x => x.ObisCode), Is.EqualTo(new ObisCode[] { "1.2.3.4.5.7" }));
    }
  }
}

