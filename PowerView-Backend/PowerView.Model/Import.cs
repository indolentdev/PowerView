using System;
using System.Globalization;

namespace PowerView.Model
{
    public class Import
    {
        public Import(string label, string channel, Unit currency, DateTime fromTimestamp, DateTime? currentTimestamp, bool enabled)
        {
            if (string.IsNullOrEmpty(label)) throw new ArgumentOutOfRangeException(nameof(label), "Must not be null or empty");
            if (string.IsNullOrEmpty(channel)) throw new ArgumentOutOfRangeException(nameof(channel), "Must not be null or empty");
            if (currency != Unit.Eur && currency != Unit.Dkk) throw new ArgumentOutOfRangeException(nameof(currency), $"Must be Eur or Dkk. Was:{currency}");
            ArgCheck.ThrowIfNotUtc(fromTimestamp);
            if (currentTimestamp != null) ArgCheck.ThrowIfNotUtc(currentTimestamp.Value);

            Label = label;
            Channel = channel;
            Currency = currency;
            FromTimestamp = fromTimestamp;
            CurrentTimestamp = currentTimestamp;
            Enabled = enabled;
        }

        public string Label { get; private set; }
        public string Channel { get; private set; }
        public Unit Currency { get; private set; }
        public DateTime FromTimestamp { get; private set; }
        public DateTime? CurrentTimestamp { get; private set; }
        public bool Enabled { get; private set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[Import: Label={0}, Channel={1}, Currency={2}, FromTimestamp={3}, CurrentTimestamp={4}, Enabled={5}]",
              Label, Channel, Currency, FromTimestamp.ToString("O"), CurrentTimestamp?.ToString("O"), Enabled);
        }

    }
}

