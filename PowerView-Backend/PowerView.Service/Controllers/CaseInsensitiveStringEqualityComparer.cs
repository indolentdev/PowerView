using System;
using System.Collections.Generic;

namespace PowerView.Service.Controllers;

public class CaseInsensitiveStringEqualityComparer : IEqualityComparer<string>
{
    #region IEqualityComparer implementation

    public bool Equals(string x, string y)
    {
#pragma warning disable CA1309 // Use ordinal string comparison
        return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
#pragma warning restore CA1309 // Use ordinal string comparison
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCode();
    }

    #endregion IEqualityComparer implementation
}
