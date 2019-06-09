using System;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  public static class ElementExtensions
  {
    public static StringElement ToStringElement(this string str)
    {
      return new StringElement { Value = str };
    }

    public static EnumElement ToEnumElement(this string str)
    {
      return new EnumElement { Value = str };
    }

    public static ByteArrayElement ToByteArrayElement(this string str)
    {
      return new ByteArrayElement { Value = str };
    }

    public static EnumArrayElement ToEnumArrayElement(this string str)
    {
      return new EnumArrayElement { Value = str };
    }

    public static IntElement ToIntElement(this int i)
    {
      return new IntElement { Value = i.ToString(System.Globalization.CultureInfo.InvariantCulture) };
    }

  }
}

