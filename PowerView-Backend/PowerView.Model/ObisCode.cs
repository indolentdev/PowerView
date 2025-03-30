using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PowerView.Model
{
    /// <summary>
    /// Represents an obis code.
    /// And defines some standard instances and some non-standard instances.
    /// 
    /// Non-standard instances are grouped mainly into: 
    /// Delta value which is the difference between two adjencent series values, calculated from the corresponding cumulative value. 
    /// Applies utility specific B code: 65
    /// 
    /// Period value which is the difference between the first series value of a period and another series value within the period, calculated from the corresponding cumulative value. 
    /// Applies utility specific B code: 66
    /// 
    /// Average value which is the converted "actual" average actual between two adjencent series values, calculated from the corresponding cumulative value.
    /// I.e. the energy between two time points converted to an average power. Or the volume between two time points converted to average flow.
    /// Applies utility specific B code: 67
    /// 
    /// Income or expense amount for one kWh in a specific time period. Income/expense amounts are excl. VAT or other charges.
    /// Applies utility specific B code: 68
    /// </summary>
    public struct ObisCode : IEnumerable<byte>, IEquatable<ObisCode>
    {
        public static readonly ObisCode ElectrActiveEnergyA14 = "1.0.1.8.0.255";
        public static readonly ObisCode ElectrActiveEnergyA14Delta = "1.65.1.8.0.255";
        public static readonly ObisCode ElectrActiveEnergyA14Period = "1.66.1.8.0.255";
        public static readonly ObisCode ElectrActiveEnergyA14NetDelta = "1.65.16.8.0.255";
        public static readonly ObisCode ElectrActualPowerP14 = "1.0.1.7.0.255";
        public static readonly ObisCode ElectrActualPowerP14Average = "1.67.1.7.0.255";
        public static readonly ObisCode ElectrActualPowerP14L1 = "1.0.21.7.0.255";
        public static readonly ObisCode ElectrActualPowerP14L2 = "1.0.41.7.0.255";
        public static readonly ObisCode ElectrActualPowerP14L3 = "1.0.61.7.0.255";
        public static readonly ObisCode ElectrActiveEnergyA23 = "1.0.2.8.0.255";
        public static readonly ObisCode ElectrActiveEnergyA23Delta = "1.65.2.8.0.255";
        public static readonly ObisCode ElectrActiveEnergyA23Period = "1.66.2.8.0.255";
        public static readonly ObisCode ElectrActiveEnergyA23NetDelta = "1.65.26.8.0.255";
        public static readonly ObisCode ElectrActualPowerP23 = "1.0.2.7.0.255";
        public static readonly ObisCode ElectrActualPowerP23Average = "1.67.2.7.0.255";
        public static readonly ObisCode ElectrActualPowerP23L1 = "1.0.22.7.0.255";
        public static readonly ObisCode ElectrActualPowerP23L2 = "1.0.42.7.0.255";
        public static readonly ObisCode ElectrActualPowerP23L3 = "1.0.62.7.0.255";
        public static readonly ObisCode ElectrActiveEnergyKwhIncomeExpenseExclVat = "1.68.25.67.0.255"; // 68=income/expense amount excl vat per kWh, 25=(active) energy import/export, 67=From timestamp and 60 mins.
        public static readonly ObisCode ElectrActiveEnergyKwhIncomeExpenseInclVat = "1.69.25.67.0.255"; // 69=income/expense amount incl vat per kWh, 25=(active) energy import/export, 67=From timestamp and 60 mins.

        private static readonly Regex isElectricityImport = new Regex(@"^1\.[0-9]{1,3}\.[246]?1\.[7-8]{1}\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex isElectricityExport = new Regex(@"^1\.[0-9]{1,3}\.[246]?2\.[7-8]{1}\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex isElectricityCumulative = new Regex(@"^1\.0\.[1-2]{1}\.8\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static readonly ObisCode ColdWaterVolume1 = "8.0.1.0.0.255";
        public static readonly ObisCode ColdWaterVolume1Delta = "8.65.1.0.0.255";
        public static readonly ObisCode ColdWaterVolume1Period = "8.66.1.0.0.255";
        public static readonly ObisCode ColdWaterFlow1 = "8.0.2.0.0.255";
        public static readonly ObisCode ColdWaterFlow1Average = "8.67.2.0.0.255";

        public static readonly ObisCode HotWaterVolume1 = "9.0.1.0.0.255";
        public static readonly ObisCode HotWaterVolume1Delta = "9.65.1.0.0.255";
        public static readonly ObisCode HotWaterVolume1Period = "9.66.1.0.0.255";
        public static readonly ObisCode HotWaterFlow1 = "9.0.2.0.0.255";
        public static readonly ObisCode HotWaterFlow1Average = "9.67.2.0.0.255";

        private static readonly Regex isWaterImport = new Regex(@"^[8-9]{1}\.[0-9]{1,3}\.[1-2]{1}\.0\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex isWaterCumulative = new Regex(@"^[8-9]{1}\.0\.1\.0\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static readonly ObisCode HeatEnergyEnergy1 = "6.0.1.0.0.255";
        public static readonly ObisCode HeatEnergyEnergy1Delta = "6.65.1.0.0.255";
        public static readonly ObisCode HeatEnergyEnergy1Period = "6.66.1.0.0.255";
        public static readonly ObisCode HeatEnergyVolume1 = "6.0.2.0.0.255";
        public static readonly ObisCode HeatEnergyVolume1Delta = "6.65.2.0.0.255";
        public static readonly ObisCode HeatEnergyVolume1Period = "6.66.2.0.0.255";
        public static readonly ObisCode HeatEnergyPower1 = "6.0.8.0.0.255";
        public static readonly ObisCode HeatEnergyPower1Average = "6.67.8.0.0.255";
        public static readonly ObisCode HeatEnergyFlow1 = "6.0.9.0.0.255";
        public static readonly ObisCode HeatEnergyFlow1Average = "6.67.9.0.0.255";
        public static readonly ObisCode HeatEnergyFlowTemperature = "6.0.10.0.0.255";
        public static readonly ObisCode HeatEnergyReturnTemperature = "6.0.11.0.0.255";

        private static readonly Regex isEnergyImport = new Regex(@"^6\.[0-9]{1,3}\.[0-9]{1,2}\.0\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex isEnergyCumulative = new Regex(@"^6\.0\.[12]{1}\.0\.0\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static readonly ObisCode RoomTemperature = "15.0.223.0.0.255"; // "Manufacture" specific obis
        public static readonly ObisCode RoomRelativeHumidity = "15.0.223.0.2.255"; // "Manufacture" specific obis

        private static readonly Regex isDisconnectControl = new Regex(@"^0\.[0-9]\.96\.3\.10\.255$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const byte BCodeDelta = 65;
        private const byte BCodePeriod = 66;
        private const byte BCodeAverage = 67;

        private readonly byte a;
        private readonly byte b;
        private readonly byte c;
        private readonly byte d;
        private readonly byte e;
        private readonly byte f;

        public ObisCode(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            if (bytes.Length != 6) throw new ArgumentOutOfRangeException(nameof(bytes), "Must be 6 bytes");

            a = bytes[0];
            b = bytes[1];
            c = bytes[2];
            d = bytes[3];
            e = bytes[4];
            f = bytes[5];
        }

        public bool IsElectricityImport { get { return isElectricityImport.IsMatch(ToString()); } }
        public bool IsElectricityExport { get { return isElectricityExport.IsMatch(ToString()); } }
        public bool IsElectricityCumulative { get { return isElectricityCumulative.IsMatch(ToString()); } }

        public bool IsWaterImport { get { return isWaterImport.IsMatch(ToString()); } }
        public bool IsWaterCumulative { get { return isWaterCumulative.IsMatch(ToString()); } }

        public bool IsEnergyImport { get { return isEnergyImport.IsMatch(ToString()); } }
        public bool IsEnergyCumulative { get { return isEnergyCumulative.IsMatch(ToString()); } }

        public bool IsCumulative { get { return IsElectricityCumulative || IsWaterCumulative || IsEnergyCumulative; } }

        public bool IsDelta { get { return b == BCodeDelta; } }

        public bool IsPeriod { get { return b == BCodePeriod; } }

        public bool IsAverage { get { return b == BCodeAverage; } }

        public bool IsDisconnectControl { get { return isDisconnectControl.IsMatch(ToString()); } }

        public bool IsUtilitySpecific { get { return b >= 65 && b <= 127; } }

        public ObisCode ToDelta()
        {
            return new ObisCode(new byte[] { a, BCodeDelta, c, d, e, f });
        }

        public ObisCode ToPeriod()
        {
            return new ObisCode(new byte[] { a, BCodePeriod, c, d, e, f });
        }

        public ObisCode ToAverage()
        {
            return new ObisCode(new byte[] { a, BCodeAverage, c, d, e, f });
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObisCode)) return false;
            return Equals((ObisCode)obj);
        }

        public bool Equals(ObisCode other)
        {
            return a == other.a && b == other.b && c == other.c &&
              d == other.d && e == other.e && f == other.f;
        }

        public override int GetHashCode()
        {
            return a + b + c + d + e + f;
        }

        public override string ToString()
        {
            return string.Join(".", (IEnumerable<byte>)this);
        }

        public static bool operator ==(ObisCode item1, ObisCode item2)
        {
            return item1.Equals(item2);
        }

        public static bool operator !=(ObisCode item1, ObisCode item2)
        {
            return !(item1 == item2);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            yield return a;
            yield return b;
            yield return c;
            yield return d;
            yield return e;
            yield return f;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool TryParse(string s, out ObisCode obisCode)
        {
            ArgumentNullException.ThrowIfNull(s);

            obisCode = new ObisCode();

            var groups = s.Split('.');
            if (groups.Length != 6) return false;
            
            var bytes = groups
                .Select(g =>
                    {
                        var ok = byte.TryParse(g, out var b);
                        return ok ? (byte?)b : null;
                    })
                .Where(g => g != null)
                .Select(g => g.Value)
                .ToArray();

            if (bytes.Length != 6) return false;

            obisCode = new ObisCode(bytes);

            return true;
        }

        public static implicit operator ObisCode(string s)
        {
            ArgumentNullException.ThrowIfNull(s);
            var groups = s.Split('.');
            return new ObisCode(groups.Select(str =>
            {
                byte groupByte;
                if (!byte.TryParse(str, out groupByte))
                {
                    throw new ArgumentException("Groups must be convertible to numbers");
                }
                return groupByte;
            }).ToArray());
        }

        public static implicit operator long(ObisCode oc)
        {
            return BitConverter.ToInt64(new byte[] { oc.f, oc.e, oc.d, oc.c, oc.b, oc.a, 0, 0 }, 0);
        }

        public static implicit operator ObisCode(long l)
        {
            var b = BitConverter.GetBytes(l);
            return new ObisCode(new[] { b[5], b[4], b[3], b[2], b[1], b[0] });
        }

        public static IEnumerable<ObisCode> GetDefined()
        {
            var obisCodeType = typeof(ObisCode);

            var publicStaticReadonlyObisCodeFields = obisCodeType.GetFields(BindingFlags.Public | BindingFlags.Static)
                                .Where(fi => fi.IsInitOnly && fi.FieldType == obisCodeType);

            foreach (var obisCodeField in publicStaticReadonlyObisCodeFields)
            {
                var obisCode = (ObisCode)obisCodeField.GetValue(null);
                yield return obisCode;
            }
        }
    }
}

