using System.Globalization;

namespace PowerView.Service.Controllers;

internal class ExportPeriod : IEquatable<ExportPeriod>, IComparable<ExportPeriod>
{
    public ExportPeriod(DateTime from, DateTime to)
    {
        From = from;
        To = to;
    }

    public DateTime From { get; }
    public DateTime To { get; }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "Period [From:{0}, To:{1}]", From.ToString("o"), To.ToString("o"));
    }

    public int CompareTo(ExportPeriod other)
    {
        if (From != other.From)
        {
            return From.CompareTo(other.From);
        }
        else
        {
            return To.CompareTo(other.To);
        }
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ExportPeriod);
    }

    public bool Equals(ExportPeriod other)
    {
        return other != null && From == other.From && To == other.To;
    }

    public override int GetHashCode()
    {
        var hashCode = -1781160927;
        hashCode = hashCode * -1521134295 + From.GetHashCode();
        hashCode = hashCode * -1521134295 + To.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(ExportPeriod period1, ExportPeriod period2)
    {
        return EqualityComparer<ExportPeriod>.Default.Equals(period1, period2);
    }

    public static bool operator !=(ExportPeriod period1, ExportPeriod period2)
    {
        return !(period1 == period2);
    }
}
