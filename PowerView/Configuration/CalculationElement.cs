using System;
using System.Configuration;
using PowerView.Model;
using PowerView.Model.Expression;

namespace PowerView.Configuration
{
  public class CalculationElement : ConfigurationElement, IConfigurationValidatable
  {
    private const string LabelString = "label";
    private const string ObisCodeString = "obiscode";
    private const string TemplateString = "template";

    [ConfigurationProperty(LabelString, IsRequired = true, IsKey = false)]
    public string Label
    {
      get { return (string)this[LabelString]; }
      set { this[LabelString] = value; }
    }

    [ConfigurationProperty(ObisCodeString, IsRequired = true, IsKey = false)]
    public string ObisCode
    {
      get { return (string)this[ObisCodeString]; }
      set { this[ObisCodeString] = value; } 
    }

    [ConfigurationProperty(TemplateString, IsRequired = true, IsKey = false)]
    public string Template
    {
      get { return (string)this[TemplateString]; }
      set { this[TemplateString] = value; } 
    }

    public void Validate()
    {
      Validate(LabelString, () => Label);
      Validate(ObisCodeString, () => ObisCode);
      Validate(TemplateString, () => Template);

      try
      {
        GetObisCode();
      }
      catch (ArgumentException e)
      {
        throw new ConfigurationErrorsException(ObisCodeString + " value attribute is not valid:" + ObisCode, e);
      }

      try
      {
        GetTemplateExpression();
      }
      catch (ArgumentException e)
      {
        throw new ConfigurationErrorsException(TemplateString + " value attribute is not valid:" + Template, e);
      }
      catch (TemplateExpressionException e)
      {
        throw new ConfigurationErrorsException(TemplateString + " value attribute is not valid:" + Template, e);
      }
    }
      
    private static void Validate(string attributeName, Func<string> property)
    {
      if ( string.IsNullOrEmpty(property()) )
      {
        throw new ConfigurationErrorsException(attributeName + " attribute is empty or absent");
      }
    }

    public ObisCode GetObisCode()
    {
      return (ObisCode)ObisCode;
    }

    public ITemplateExpression GetTemplateExpression()
    {
      return new TemplateExpressionFactory().Create(Template);
    }

  }
}

