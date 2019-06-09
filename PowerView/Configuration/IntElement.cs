using System;
using System.Configuration;

namespace PowerView.Configuration
{
  public class IntElement : StringElement
  {
    public override void Validate(string attributeName)
    {
      base.Validate(attributeName);

      int res;
      if (!Int32.TryParse(Value, out res) || res < 0)
      {
        throw new ConfigurationErrorsException(attributeName + " value attribute is not a valid number");
      }
    }

    public int GetValueAsInt()
    {
      return Int32.Parse(Value);
    }
  }
}

