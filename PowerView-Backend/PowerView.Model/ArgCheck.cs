using System.Runtime.CompilerServices;

namespace PowerView.Model;

public static class ArgCheck
{
    public static void ThrowIfNullOrEmpty(string argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);
        ArgumentOutOfRangeException.ThrowIfEqual(argument, string.Empty, paramName);
    }

    public static void ThrowIfNotUtc(DateTime argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        if (argument.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(paramName, $"Must be UTC. Was:{argument.Kind}");
    }
}