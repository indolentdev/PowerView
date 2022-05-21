using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PowerView.Configuration
{
  public class EnumArrayElement : StringElement
  {
    public void Validate<TEnum>(string attributeName)
    {
      base.Validate(attributeName);

      foreach (var item in GetStringItems())
      {
        if (! Enum.IsDefined(typeof(TEnum), item))
        {
          throw new ConfigurationErrorsException(attributeName + " value attribute as invalid value. Must be one of:" + 
                                                 string.Join(", ", Enum.GetValues(typeof(TEnum))));
        }
      }
    }

    public IEnumerable<TEnum> GetItems<TEnum>()
    {
      foreach (var item in GetStringItems())
      {
        yield return (TEnum)Enum.Parse(typeof(TEnum), item, true);
      }
    }

    private IEnumerable<string> GetStringItems()
    {
      var items = Value.Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
      return items;
    }

  }
}

