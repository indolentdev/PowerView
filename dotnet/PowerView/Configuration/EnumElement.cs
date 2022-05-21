using System;
using System.Configuration;

namespace PowerView.Configuration
{
  public class EnumElement : StringElement
  {
    public TEnum GetValueAs<TEnum>()
    {
      return (TEnum)Enum.Parse(typeof(TEnum), Value, true);
    }

    public virtual void Validate<TEnum>(string attributeName)
    {
      base.Validate(attributeName);

      if (! Enum.IsDefined(typeof(TEnum), Value))
      {
        throw new ConfigurationErrorsException(attributeName + " value attribute as invalid value. Must be one of:" + 
                                               string.Join(", ", Enum.GetValues(typeof(TEnum))));
      }
    }
  }
}

