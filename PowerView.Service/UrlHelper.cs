using System.Runtime.CompilerServices;

namespace PowerView.Service;

public static class UrlHelper
{
    public static Uri EncodeUrlParameters(Uri url, FormattableString query)
    {
        var s = FormattableStringFactory.Create(
            query.Format,
            query.GetArguments()
                .Select(a => Uri.EscapeDataString(a?.ToString() ?? ""))
                .ToArray());
        return new Uri(url, s.ToString());
    }
}