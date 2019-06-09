using System;
using System.Configuration;

namespace PowerView.Configuration
{
  public class UIntElement : StringElement
  {
    public override void Validate(string attributeName)
    {
      base.Validate(attributeName);

      uint res;
      if (!UInt32.TryParse(Value, out res))
      {
        throw new ConfigurationErrorsException(attributeName + " value attribute is not a valid number");
      }
    }

    public uint GetValueAsUInt()
    {
      return UInt32.Parse(Value);
    }
  }
}

