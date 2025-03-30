using System;
using System.Globalization;

namespace PowerView.Model
{
    public class SeriesName : ISeriesName, IEquatable<SeriesName>
    {
        public SeriesName(string label, ObisCode obisCode)
        {
            ArgCheck.ThrowIfNullOrEmpty(label);

            Label = label;
            ObisCode = obisCode;
        }

        public string Label { get; private set; }
        public ObisCode ObisCode { get; private set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[SeriesName: Label={0}, ObisCode={1}]", Label, ObisCode);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SeriesName)) return false;
            return Equals((SeriesName)obj);
        }

        public bool Equals(SeriesName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
#pragma warning disable CA1309 // Use ordinal string comparison      
            return string.Equals(Label, other.Label, StringComparison.InvariantCulture) &&
                         ObisCode.Equals(other.ObisCode);
#pragma warning restore CA1309 // Use ordinal string comparison                   
        }

        public bool Equals(ISeriesName other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Label.GetHashCode();
                hashCode = (hashCode * 397) ^ ObisCode.GetHashCode();
                return hashCode;
            }
        }

    }
}

