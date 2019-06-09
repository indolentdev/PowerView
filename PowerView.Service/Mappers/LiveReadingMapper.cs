using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using log4net;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Service.Dtos;

namespace PowerView.Service.Mappers
{
  internal class LiveReadingMapper : ILiveReadingMapper
  {
    protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public IEnumerable<LiveReading> Map(string contentType, Stream body)
    {
      if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException("contentType");
      if (!contentType.ToLowerInvariant().Contains("application/json"))
      {
        throw new ArgumentOutOfRangeException("contentType", "Only supports application/json ContentType");
      }
      if (body == null) throw new ArgumentNullException("body");

      var json = ReadStream(body);    
      var liveReadingSetDto = Deserialize(json);
      var liveReading = Map(liveReadingSetDto);
      return liveReading;
    }

    private static string ReadStream(Stream body)
    {
      string json;
      try
      {
        using (var sr = new StreamReader(body))
        {
          json = sr.ReadToEnd();
        }
      }
      catch (IOException e)
      {
        throw new ArgumentOutOfRangeException("Unable to read body stream", e);
      }
      return json;
    }

    private static LiveReadingSetDto Deserialize(string json)
    {
      try
      {
        return JsonConvert.DeserializeObject<LiveReadingSetDto>(json);
      }
      catch (JsonException e)
      {
        throw new ArgumentOutOfRangeException("Unable to deserialize to LiveReading JSON representation", e);
      }
    }

    private static IEnumerable<LiveReading> Map(LiveReadingSetDto liveReadingSetDto)
    {
      foreach (var liveReadingDto in liveReadingSetDto.Items)
      {
        yield return new LiveReading(liveReadingDto.Label, liveReadingDto.SerialNumber, liveReadingDto.Timestamp, MapLiveReadings(liveReadingDto.RegisterValues));
      }
    }

    private static IEnumerable<RegisterValue> MapLiveReadings(IEnumerable<RegisterValueDto> registerValueDtos)
    {
      foreach (var registerValueDto in registerValueDtos)
      {
        ObisCode obisCode = registerValueDto.ObisCode;
        Unit unit;
        if (!Enum.TryParse<Unit>(registerValueDto.Unit, true, out unit))
        {
          throw new ArgumentOutOfRangeException("registerValueDtos", registerValueDto.Unit + " is not a valid Unit value");
        }
        yield return new RegisterValue(obisCode, registerValueDto.Value, registerValueDto.Scale, unit);
      }
    }

    public LiveReading MapPvOutputArgs(Uri requestUrl, string contentType, Stream body, string deviceLabel, string deviceSerialNumber, string deviceSerialNumberParam,
                                       string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param)
    {
      if (requestUrl == null) throw new ArgumentNullException("requestUrl");
      if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException("contentType");
      if (body == null) throw new ArgumentNullException("body");
      if (string.IsNullOrEmpty(deviceLabel)) throw new ArgumentNullException("deviceLabel");
      if (string.IsNullOrEmpty(deviceSerialNumberParam)) throw new ArgumentNullException("deviceSerialNumberParam");

      var pvArgString = GetPvArgs(requestUrl, contentType, body);
      var pvArgsArray = pvArgString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
      var pvArgs = GetPvArgValues(pvArgsArray);

      if (!pvArgs.ContainsKey("d") || !pvArgs.ContainsKey("t") ||
          ((!pvArgs.ContainsKey("v1") || !pvArgs.ContainsKey("c1") || pvArgs["c1"][0] != "1") && !pvArgs.ContainsKey("v2")))
      {
        log.InfoFormat("Unable to extract PV data from PVOutput.org parameters. Needed parameters not present. " +
          "Expected parameters d and t with v2 and/or v1 and c1=1. Params:{0}", pvArgString);
        return null;
      }

      var serialNumber = GetSerialNumber(deviceSerialNumber, pvArgs, deviceSerialNumberParam);
      if (serialNumber == null)
      {
        log.InfoFormat("Failed to extract PV device serial number from configuration or PV data. SerialNumberParam:{0}, Params:{1}",
          deviceSerialNumberParam, pvArgString);
        return null;
      }

      var d = pvArgs["d"][0];
      var t = pvArgs["t"][0];
      var timestamp = DateTime.ParseExact((d+t).Replace(":", string.Empty), "yyyyMMddHHmm", 
        CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.NoCurrentDateDefault).ToUniversalTime();

      var registerValues = new [] { 
        GetRegisterValue(pvArgs, "v1", ObisCode.ActiveEnergyA23, Unit.WattHour),
        GetRegisterValue(pvArgs, "v2", ObisCode.ActualPowerP23, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L1Param, ObisCode.ActualPowerP23L1, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L2Param, ObisCode.ActualPowerP23L2, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L3Param, ObisCode.ActualPowerP23L3, Unit.Watt),
      }.Where(rv => rv != null);

      return new LiveReading(deviceLabel, serialNumber, timestamp, registerValues);
    }

    private static string GetPvArgs(Uri requestUrl, string contentType, Stream body)
    {
      if (!string.IsNullOrEmpty(contentType) && contentType.ToLowerInvariant().Contains("application/x-www-form-urlencoded"))
      {
        using (var sr = new StreamReader(body, System.Text.Encoding.UTF8))
        {
          return sr.ReadToEnd();
        }
      }
      else if (requestUrl.Query.Length > 0)
      {
        return requestUrl.Query.Substring(1); // skip leading ?
      }
      return string.Empty;
    }

    private IDictionary<string, IList<string>> GetPvArgValues(string[] pvArgs)
    {
      var result = new Dictionary<string, IList<string>>(pvArgs.Length);
      foreach (var pvArg in pvArgs)
      {
        var pvArgValue = pvArg.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
        if (pvArgValue.Length != 2)
        {
          log.DebugFormat("Failed to map pv arg:{0}", pvArg);
          continue;
        }

        var key = pvArgValue[0].ToLowerInvariant();
        if (!result.ContainsKey(key))
        {
          result.Add(key, new List<string>(2));
        }

        var value = HttpUtility.UrlDecode(pvArgValue[1]);
        result[key].Add(value);
      }
      return result;
    }

    private static string GetSerialNumber(string deviceSerialNumber, IDictionary<string, IList<string>> pvArgs, string deviceSerialNumberParam)
    {
      if (deviceSerialNumber != null)
      {
        return deviceSerialNumber;
      }

      if (!pvArgs.ContainsKey(deviceSerialNumberParam))
      {
        return null;
      }

      return pvArgs[deviceSerialNumberParam][0];
    }

    private static RegisterValue GetRegisterValue(IDictionary<string, IList<string>> pvArgs, string param, ObisCode obisCode, Unit unit)
    {
      if (string.IsNullOrEmpty(param))
      {
        return null;
      }

      if (!pvArgs.ContainsKey(param))
      {
        return null;
      }

      var valueString = pvArgs[param][0];
      if (valueString == null)
      {
        return null;
      }

      int value;
      if (!int.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
      {
        log.WarnFormat("Failed to parse PvOutput numeric value {0}.", valueString);
        return null;
      }
      return new RegisterValue(obisCode, value, 0, unit);
    }
  }
}