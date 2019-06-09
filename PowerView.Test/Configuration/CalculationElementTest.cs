using System.Configuration;
using NUnit.Framework;
using PowerView.Configuration;
using PowerView.Model;
using PowerView.Model.Expression;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class CalculationElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new CalculationElement { Label=string.Empty, ObisCode="1.2.3.4.5.6", Template="X:1.2.3.4.5.6" }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());
      Assert.That(() => new CalculationElement { Label=null, ObisCode="1.2.3.4.5.6", Template="X:1.2.3.4.5.6" }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());

      Assert.That(() => new CalculationElement { Label="Label", ObisCode=string.Empty, Template="X:1.2.3.4.5.6" }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());
      Assert.That(() => new CalculationElement { Label="Label", ObisCode=null, Template="X:1.2.3.4.5.6" }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());
      Assert.That(() => new CalculationElement { Label="Label", ObisCode="BadObis", Template="X:1.2.3.4.5.6" }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());

      Assert.That(() => new CalculationElement { Label="Label", ObisCode="1.2.3.4.5.6", Template=string.Empty }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());
      Assert.That(() => new CalculationElement { Label="Label", ObisCode="1.2.3.4.5.6", Template=null }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());
      Assert.That(() => new CalculationElement { Label="Label", ObisCode="1.2.3.4.5.6", Template="Bad" }.Validate(), Throws.TypeOf<ConfigurationErrorsException>());
    }

    [Test]
    public void GetObisCode()
    {
      // Arrange
      var target = new CalculationElement { Label="Label", ObisCode="1.2.3.4.5.6", Template="X:1.2.3.4.5.6" };

      // Act
      var obisCode = target.GetObisCode();

      // Assert
      Assert.That(obisCode, Is.TypeOf<ObisCode>());
    }

    [Test]
    public void GetTemplateExpression()
    {
      // Arrange
      var target = new CalculationElement { Label="Label", ObisCode="1.2.3.4.5.6", Template="X:1.2.3.4.5.6" };

      // Act
      var templateExpression = target.GetTemplateExpression();

      // Assert
      Assert.That(templateExpression, Is.Not.Null);
    }

  }
}
