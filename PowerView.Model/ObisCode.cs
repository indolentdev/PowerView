using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PowerView.Model
{
  public struct ObisCode : IEnumerable<byte>, IEquatable<ObisCode>
  {
    public static readonly ObisCode ActiveEnergyA14 = "1.0.1.8.0.255";
    public static readonly ObisCode ActiveEnergyA14Interim = "1.0.1.8.0.200"; // Fictive obis
    public static readonly ObisCode ActiveEnergyA14Delta = "1.0.1.8.0.100"; // Fictive obis
    public static readonly ObisCode ActualPowerP14 = "1.0.1.7.0.255";
    public static readonly ObisCode ActualPowerP14L1 = "1.0.21.7.0.255";
    public static readonly ObisCode ActualPowerP14L2 = "1.0.41.7.0.255";
    public static readonly ObisCode ActualPowerP14L3 = "1.0.61.7.0.255";
    public static readonly ObisCode ActiveEnergyA23 = "1.0.2.8.0.255";
    public static readonly ObisCode ActiveEnergyA23Interim = "1.0.2.8.0.200"; // Fictive obis
    public static readonly ObisCode ActiveEnergyA23Delta = "1.0.2.8.0.100"; // Fictive obis
    public static readonly ObisCode ActualPowerP23 = "1.0.2.7.0.255";
    public static readonly ObisCode ActualPowerP23L1 = "1.0.22.7.0.255";
    public static readonly ObisCode ActualPowerP23L2 = "1.0.42.7.0.255";
    public static readonly ObisCode ActualPowerP23L3 = "1.0.62.7.0.255";

    private static readonly Regex isElectricityImport  = new Regex(@"^1\.0\.[246]?1\.[7-8]{1}\.0\.[0-9]{1,3}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex isElectricityExport  = new Regex(@"^1\.0\.[246]?2\.[7-8]{1}\.0\.[0-9]{1,3}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex isElectricityCumulative  = new Regex(@"^1\.[0-9]{1,3}\.[1-2]{1}\.8\.0\.255", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly ObisCode ColdWaterVolume1 = "8.0.1.0.0.255";
    public static readonly ObisCode ColdWaterVolume1Interim = "8.0.1.0.0.200"; // Fictive obis
    public static readonly ObisCode ColdWaterVolume1Delta = "8.0.1.0.0.100"; // Fictive obis
    public static readonly ObisCode ColdWaterFlow1 = "8.0.2.0.0.255";

    private static readonly Regex isWaterImport  = new Regex(@"^[8-9]{1}\.0\.[1-2]{1}\.0\.0\.[0-9]{1,3}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex isWaterCumulative  = new Regex(@"^[8-9]{1}\.0\.1\.0\.0\.255", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly ObisCode HeatEnergyEnergy1 = "6.0.1.0.0.255";
    public static readonly ObisCode HeatEnergyEnergy1Interim = "6.0.1.0.0.200"; // Fictive obis
    public static readonly ObisCode HeatEnergyEnergy1Delta = "6.0.1.0.0.100"; // Fictive obis
    public static readonly ObisCode HeatEnergyVolume1 = "6.0.2.0.0.255";
    public static readonly ObisCode HeatEnergyVolume1Interim = "6.0.2.0.0.200"; // Fictive obis
    public static readonly ObisCode HeatEnergyVolume1Delta = "6.0.2.0.0.100"; // Fictive obis
    public static readonly ObisCode HeatEnergyPower1 = "6.0.8.0.0.255";
    public static readonly ObisCode HeatEnergyFlow1 = "6.0.9.0.0.255";
    public static readonly ObisCode HeatEnergyFlowTemperature = "6.0.10.0.0.255";
    public static readonly ObisCode HeatEnergyReturnTemperature = "6.0.11.0.0.255";

    private static readonly Regex isEnergyImport  = new Regex(@"^[5-6]{1}\.0\.[0-9]{1,3}\.0\.0\.[0-9]{1,3}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex isEnergyCumulative  = new Regex(@"^[5-6]{1}\.0\.[12]{1}\.0\.0\.255", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly ObisCode RoomTemperature = "15.0.223.0.0.255"; // Fictive obis
    public static readonly ObisCode RoomRelativeHumidity = "15.0.223.0.2.255"; // Fictive obis

    public static readonly ObisCode ConsumedElectricity = "1.210.1.8.0.255"; // Fictive obis
    public static readonly ObisCode ConsumedElectricityInterim = "1.210.1.8.0.200"; // Fictive obis
    public static readonly ObisCode ConsumedElectricityDelta = "1.210.1.8.0.100"; // Fictive obis
    public static readonly ObisCode ConsumedElectricityWithHeat = "1.220.1.8.0.255"; // Fictive obis
    public static readonly ObisCode ConsumedElectricityWithHeatInterim = "1.220.1.8.0.200"; // Fictive obis
    public static readonly ObisCode ConsumedElectricityWithHeatDelta = "1.220.1.8.0.100"; // Fictive obis

    private static readonly Regex isDisconnectControl = new Regex(@"^0\.[0-9]{1,3}\.96\.3\.10\.255", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly ICollection<ObisCode> templates = new [] { 
      ConsumedElectricity, ConsumedElectricityInterim, ConsumedElectricityDelta, 
      ConsumedElectricityWithHeat, ConsumedElectricityWithHeatInterim, ConsumedElectricityWithHeatDelta };

    private readonly byte a;
    private readonly byte b;
    private readonly byte c;
    private readonly byte d;
    private readonly byte e;
    private readonly byte f;

    public ObisCode(byte[] bytes)
    {
      if ( bytes == null ) throw new ArgumentNullException("bytes");
      if ( bytes.Length != 6 ) throw new ArgumentOutOfRangeException("bytes", "Must be 6 bytes");

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

    public bool IsInterim { get { return f == 200; } }

    public bool IsDelta { get { return f == 100; } }

    public bool IsDisconnectControl { get { return isDisconnectControl.IsMatch(ToString()); } }

    public static ICollection<ObisCode> Templates { get { return templates; } }

    public ObisCode ToInterim()
    {
      return new ObisCode(new byte[] { a, b, c, d, e, 200 });
    }

    public ObisCode ToDelta()
    {
      return new ObisCode(new byte[] { a, b, c, d, e, 100 });
    }

    public override bool Equals(object obj)
    {
      if ( ! (obj is ObisCode) ) return false;
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

    public static implicit operator ObisCode(string s)
    {
      if ( s == null ) throw new ArgumentNullException();
      var groups = s.Split('.');
      return new ObisCode(groups.Select(str => {
        byte groupByte;
        if ( !byte.TryParse(str, out groupByte) )
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
      return new ObisCode(new [] { b[5], b[4], b[3], b[2], b[1], b[0] });
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

