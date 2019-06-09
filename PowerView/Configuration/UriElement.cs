using System;
using System.Configuration;

namespace PowerView.Configuration
{
  public class UriElement : StringElement
  {
    public override void Validate(string attributeName)
    {
      base.Validate(attributeName);

      Uri res;
      if (!Uri.TryCreate(Value, UriKind.Absolute, out res) || res == null)
      {
        throw new ConfigurationErrorsException(attributeName + " value attribute is not a valid URL");
      }
    }

    public Uri GetValueAsUri()
    {
      return new Uri(Value);
    }
  }
}

