using System.Globalization;

namespace PowerView.Model
{
    public class EnergiDataServiceImportConfig
    {
        internal const string SettingPrefix = "EDSI_";
        private const string EdsiImportEnabled = SettingPrefix + "ImportEnabled";
        private const string EdsiTimeSpan = SettingPrefix + "TimeSpan";
        private const string EdsiPriceArea = SettingPrefix + "PriceArea";
        private const string EdsiLabel = SettingPrefix + "Label";
        private const string EdsiCurrency = SettingPrefix + "Currency";

        public EnergiDataServiceImportConfig(bool importEnabled, TimeSpan timeSpan, string priceArea, string label, Unit currency)
        {
            if (timeSpan <= TimeSpan.Zero || timeSpan > TimeSpan.FromDays(1)) throw new DomainConstraintException("timeSpan must be lager than zero and below 1 day");
            if (string.IsNullOrEmpty(priceArea)) throw new DomainConstraintException("priceArea is required");
            if (string.IsNullOrEmpty(label)) throw new DomainConstraintException("label is required");
            if (currency != Unit.Dkk && currency != Unit.Eur) throw new DomainConstraintException("currency must be Dkk or Eur");

            ImportEnabled = importEnabled;
            TimeSpan = timeSpan;
            PriceArea = priceArea;
            Label = label;
            Currency = currency;
        }

        internal EnergiDataServiceImportConfig(ICollection<KeyValuePair<string, string>> entsoeSettings)
        {
            if (entsoeSettings == null) throw new ArgumentNullException(nameof(entsoeSettings));

            var importEnabledString = GetValue(entsoeSettings, EdsiImportEnabled);
            var timeSpanString = GetValue(entsoeSettings, EdsiTimeSpan);
            var priceAreaString = GetValue(entsoeSettings, EdsiPriceArea);
            var labelString = GetValue(entsoeSettings, EdsiLabel);
            var currencyString = GetValue(entsoeSettings, EdsiCurrency);
            if (string.IsNullOrEmpty(importEnabledString)) throw new DomainConstraintException("ImportEnabled is required");
            if (string.IsNullOrEmpty(timeSpanString)) throw new DomainConstraintException("TimeSpan is required");
            if (string.IsNullOrEmpty(priceAreaString)) throw new DomainConstraintException("PriceArea (crypt) is required");
            if (string.IsNullOrEmpty(labelString)) throw new DomainConstraintException("Label is required");
            if (string.IsNullOrEmpty(currencyString)) throw new DomainConstraintException("Currency is required");

            PriceArea = priceAreaString;
            Label = labelString;

            if (!bool.TryParse(importEnabledString, out var importEnabled))
            {
                throw new DomainConstraintException($"ImportEnabled must be a bool (roundtrip). Was:{importEnabledString}");
            }
            ImportEnabled = importEnabled;

            if (!TimeSpan.TryParse(timeSpanString, CultureInfo.InvariantCulture, out var timeSpan))
            {
                throw new DomainConstraintException($"TimeSpan must be a TimeSpan (roundtrip). Was:{timeSpanString}");
            }
            TimeSpan = timeSpan;

            if (!Enum.TryParse<Unit>(currencyString, false, out var currency) || (currency != Unit.Dkk && currency != Unit.Eur)) 
            {
                throw new DomainConstraintException($"Currency must be a Unit with Dkk or Eur (roundtrip). Was:{currencyString}");
            }
            Currency = currency;
        }

        private static string GetValue(ICollection<KeyValuePair<string, string>> entsoeSettings, string key)
        {
            return entsoeSettings.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
        }

        public bool ImportEnabled { get; private set; }
        public TimeSpan TimeSpan { get; private set; }
        public string PriceArea { get; private set; }
        public string Label { get; private set; }
        public Unit Currency { get; private set; }

        internal ICollection<KeyValuePair<string, string>> GetSettings()
        {
            var settings = new List<KeyValuePair<string, string>>
            {
              new KeyValuePair<string, string>(EdsiImportEnabled, ImportEnabled.ToString(CultureInfo.InvariantCulture)),
              new KeyValuePair<string, string>(EdsiTimeSpan, TimeSpan.ToString()),
              new KeyValuePair<string, string>(EdsiPriceArea, PriceArea),
              new KeyValuePair<string, string>(EdsiLabel, Label),
              new KeyValuePair<string, string>(EdsiCurrency, Currency.ToString()),
            };
            return settings;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "EnergiDataServiceImportConfig [ImportEnabled:{0}, TimeSpan:{1}, PriceArea:{2}, Label:{3}, Currency:{4}]", 
                ImportEnabled, TimeSpan, PriceArea, Label, Currency);
        }

    }
}
