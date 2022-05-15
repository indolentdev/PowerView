using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Service.Dtos;

namespace PowerView.Service.Mappers
{
  internal class LiveReadingMapper : ILiveReadingMapper
  {
    private readonly ILogger logger;

    public LiveReadingMapper(ILogger<LiveReadingMapper> logger)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));      
    }

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
        var deviceId = liveReadingDto.DeviceId;
        if (string.IsNullOrEmpty(deviceId))
        {
          deviceId = liveReadingDto.SerialNumber;
        }
        if (string.IsNullOrEmpty(deviceId))
        {
          throw new ArgumentOutOfRangeException("DeviceId absent in LiveReading JSON representation"); // temporary until SerialNumber property is removed.
        }

        yield return new LiveReading(liveReadingDto.Label, deviceId, liveReadingDto.Timestamp, MapLiveReadings(liveReadingDto.RegisterValues));
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

    public LiveReading MapPvOutputArgs(Uri requestUrl, string contentType, Stream body, string deviceLabel, string deviceId, string deviceIdParam,
                                       string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param)
    {
      if (requestUrl == null) throw new ArgumentNullException("requestUrl");
      if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException("contentType");
      if (body == null) throw new ArgumentNullException("body");
      if (string.IsNullOrEmpty(deviceLabel)) throw new ArgumentNullException("deviceLabel");
      if (string.IsNullOrEmpty(deviceIdParam)) throw new ArgumentNullException("deviceIdParam");

      var pvArgString = GetPvArgs(requestUrl, contentType, body);
      var pvArgsArray = pvArgString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
      var pvArgs = GetPvArgValues(pvArgsArray);

      if (!pvArgs.ContainsKey("d") || !pvArgs.ContainsKey("t") ||
          ((!pvArgs.ContainsKey("v1") || !pvArgs.ContainsKey("c1") || pvArgs["c1"][0] != "1") && !pvArgs.ContainsKey("v2")))
      {
        logger.LogInformation($"Unable to extract PV data from PVOutput.org parameters. Needed parameters not present. Expected parameters d and t with v2 and/or v1 and c1=1. Params:{pvArgString}");
        return null;
      }

      var resolvedDeviceId = GetDeviceId(deviceId, pvArgs, deviceIdParam);
      if (resolvedDeviceId == null)
      {
        logger.LogInformation($"Failed to extract PV device id from configuration or PV data. DeviceIdParam:{deviceIdParam}, Params:{pvArgString}");
        return null;
      }

      var d = pvArgs["d"][0];
      var t = pvArgs["t"][0];
      var timestamp = DateTime.ParseExact((d+t).Replace(":", string.Empty), "yyyyMMddHHmm", 
        CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.NoCurrentDateDefault).ToUniversalTime();

      var registerValues = new [] { 
        GetRegisterValue(pvArgs, "v1", ObisCode.ElectrActiveEnergyA23, Unit.WattHour),
        GetRegisterValue(pvArgs, "v2", ObisCode.ElectrActualPowerP23, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L1Param, ObisCode.ElectrActualPowerP23L1, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L2Param, ObisCode.ElectrActualPowerP23L2, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L3Param, ObisCode.ElectrActualPowerP23L3, Unit.Watt),
      }.Where(rv => rv != null);

      return new LiveReading(deviceLabel, resolvedDeviceId, timestamp, registerValues);
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
          logger.LogDebug($"Failed to map pv arg:{pvArg}");
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

    private static string GetDeviceId(string deviceId, IDictionary<string, IList<string>> pvArgs, string deviceIdParam)
    {
      if (deviceId != null)
      {
        return deviceId;
      }

      if (!pvArgs.ContainsKey(deviceIdParam))
      {
        return null;
      }

      return pvArgs[deviceIdParam][0];
    }

    private RegisterValue GetRegisterValue(IDictionary<string, IList<string>> pvArgs, string param, ObisCode obisCode, Unit unit)
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
        logger.LogWarning($"Failed to parse PvOutput numeric value {valueString}.");
        return null;
      }
      return new RegisterValue(obisCode, value, 0, unit);
    }
  }
}