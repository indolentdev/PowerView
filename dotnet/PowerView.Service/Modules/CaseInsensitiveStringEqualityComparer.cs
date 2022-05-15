using System;
using System.Collections.Generic;

namespace PowerView.Service.Modules
{
  public class CaseInsensitiveStringEqualityComparer : IEqualityComparer<string>
  {
    #region IEqualityComparer implementation

    public bool Equals(string x, string y)
    {
      return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(string s)
    {
      return s.GetHashCode();
    }

    #endregion IEqualityComparer implementation
  }
}

