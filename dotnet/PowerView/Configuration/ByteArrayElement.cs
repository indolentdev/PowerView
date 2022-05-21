using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace PowerView.Configuration
{
  public class ByteArrayElement : StringElement, IEnumerable<byte>
  {
    public override void Validate(string attributeName)
    {
      base.Validate(attributeName);

      foreach (var item in GetStringItems())
      {
        byte res;
        if (!byte.TryParse(item, NumberStyles.Integer, CultureInfo.InvariantCulture, out res))
        {
          throw new ConfigurationErrorsException(attributeName + " value attribute has item which is not a valid (byte) number");
        }
      }
    }

    private IEnumerable<string> GetStringItems()
    {
      var items = Value.Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
      return items;
    }

    #region IEnumerable implementation
    public IEnumerator<byte> GetEnumerator()
    {
      foreach (var item in GetStringItems())
      {
        var b = byte.Parse(item, NumberStyles.Integer, CultureInfo.InvariantCulture);
        yield return b;
      }

    }
    #endregion

    #region IEnumerable implementation
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }
}

