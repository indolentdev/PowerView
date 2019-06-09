using System;
using NUnit.Framework;
using PowerView.Model.Expression;

namespace PowerView.Model.Test.Expression
{
  [TestFixture]
  public class TemplateExpressionFactoryTest
  {
    [Test]
    public void CreateThrows() 
    {
      // Arrange
      var target = new TemplateExpressionFactory();

      // Act & Assert
      Assert.That(() => target.Create(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void CreateRegisterTemplateExpression() 
    {
      // Arrange
      var target = new TemplateExpressionFactory();
      const string template = "MyLabel:1.2.3.4.5.6";

      // Act
      var templateExpression = target.Create(template);

      // Assert
      Assert.That(templateExpression, Is.TypeOf<RegisterTemplateExpression>());
    }

    [Test]
    public void CreateOperationTemplateExpression() 
    {
      // Arrange
      var target = new TemplateExpressionFactory();
      const string template = "MyLabel:1.2.3.4.5.6+MyLabel2:6.5.4.3.2.1";

      // Act
      var templateExpression = target.Create(template);

      // Assert
      Assert.That(templateExpression, Is.TypeOf<OperationTemplateExpression>());
      var operationTemplateExpression = (OperationTemplateExpression)templateExpression;
      Assert.That(operationTemplateExpression.Left, Is.TypeOf<RegisterTemplateExpression>());
      Assert.That(operationTemplateExpression.Operator, Is.EqualTo("+"));
      Assert.That(operationTemplateExpression.Right, Is.TypeOf<RegisterTemplateExpression>());
    }

    [Test]
    public void CreateOperationTemplateExpressionRecursive() 
    {
      // Arrange
      var target = new TemplateExpressionFactory();
      const string template = "MyLabel:1.2.3.4.5.6+MyLabel2:6.5.4.3.2.1-MyLabel3:1.1.2.2.3.3";

      // Act
      var templateExpression = target.Create(template);

      // Assert
      Assert.That(templateExpression, Is.TypeOf<OperationTemplateExpression>());
      var operationTemplateExpression = (OperationTemplateExpression)templateExpression;
      Assert.That(operationTemplateExpression.Left, Is.TypeOf<OperationTemplateExpression>());
      Assert.That(operationTemplateExpression.Operator, Is.EqualTo("-"));
      Assert.That(operationTemplateExpression.Right, Is.TypeOf<RegisterTemplateExpression>());
      operationTemplateExpression = (OperationTemplateExpression)operationTemplateExpression.Left;
      Assert.That(operationTemplateExpression.Left, Is.TypeOf<RegisterTemplateExpression>());
      Assert.That(operationTemplateExpression.Operator, Is.EqualTo("+"));
      Assert.That(operationTemplateExpression.Right, Is.TypeOf<RegisterTemplateExpression>());
    }

    [Test]
    public void CreateStripsWhiteSpace() 
    {
      // Arrange
      var target = new TemplateExpressionFactory();
      const string template = "\tMy Label : 1 . 2 .3.4.5.6   ";

      // Act
      var templateExpression = target.Create(template);

      // Assert
      var registerTemplateExpression = (RegisterTemplateExpression)templateExpression;
      Assert.That(registerTemplateExpression.Label, Is.EqualTo("MyLabel"));
      Assert.That(registerTemplateExpression.ObisCode.ToString(), Is.EqualTo("1.2.3.4.5.6"));
    }

  }
}

