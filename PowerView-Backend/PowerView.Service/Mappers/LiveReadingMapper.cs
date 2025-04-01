using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Logging;
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

        public Reading MapPvOutputArgs(Uri requestUrl, string contentType, Stream body, string deviceLabel, string deviceId, string deviceIdParam,
                                           string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param)
        {
            ArgumentNullException.ThrowIfNull(requestUrl);
            ArgCheck.ThrowIfNullOrEmpty(contentType);
            ArgumentNullException.ThrowIfNull(body);
            ArgCheck.ThrowIfNullOrEmpty(deviceLabel);
            ArgCheck.ThrowIfNullOrEmpty(deviceIdParam);

            var pvArgString = GetPvArgs(requestUrl, contentType, body);
            var pvArgsArray = pvArgString.Split('&', StringSplitOptions.RemoveEmptyEntries);
            var pvArgs = GetPvArgValues(pvArgsArray);

            if (!pvArgs.TryGetValue("d", out var dArgs) || !pvArgs.TryGetValue("t", out var tArgs) ||
                ((!pvArgs.ContainsKey("v1") || !pvArgs.ContainsKey("c1") || pvArgs["c1"][0] != "1") && !pvArgs.ContainsKey("v2")))
            {
                logger.LogInformation("Unable to extract PV data from PVOutput.org parameters. Needed parameters not present. Expected parameters d and t with v2 and/or v1 and c1=1. Params:{Args}", pvArgString);
                return null;
            }

            var resolvedDeviceId = GetDeviceId(deviceId, pvArgs, deviceIdParam);
            if (resolvedDeviceId == null)
            {
                logger.LogInformation("Failed to extract PV device id from configuration or PV data. DeviceIdParam:{Param}, Params:{Args}", deviceIdParam, pvArgString);
                return null;
            }

            var d = dArgs[0];
            var t = tArgs[0];
            var timestamp = DateTime.ParseExact((d + t).Replace(":", string.Empty), "yyyyMMddHHmm",
              CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.NoCurrentDateDefault).ToUniversalTime();

            var registerValues = new[] {
        GetRegisterValue(pvArgs, "v1", ObisCode.ElectrActiveEnergyA23, Unit.WattHour),
        GetRegisterValue(pvArgs, "v2", ObisCode.ElectrActualPowerP23, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L1Param, ObisCode.ElectrActualPowerP23L1, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L2Param, ObisCode.ElectrActualPowerP23L2, Unit.Watt),
        GetRegisterValue(pvArgs, actualPowerP23L3Param, ObisCode.ElectrActualPowerP23L3, Unit.Watt),
      }.Where(rv => rv != null).Select(rv => rv.Value);

            return new Reading(deviceLabel, resolvedDeviceId, timestamp, registerValues);
        }

        private static string GetPvArgs(Uri requestUrl, string contentType, Stream body)
        {
            if (!string.IsNullOrEmpty(contentType) && contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
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

        private Dictionary<string, IList<string>> GetPvArgValues(string[] pvArgs)
        {
            var result = new Dictionary<string, IList<string>>(pvArgs.Length);
            foreach (var pvArg in pvArgs)
            {
                var pvArgValue = pvArg.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (pvArgValue.Length != 2)
                {
                    logger.LogDebug("Failed to map pv arg:{Arg}", pvArg);
                    continue;
                }

                var key = pvArgValue[0].ToLowerInvariant();
                if (!result.TryGetValue(key, out var args))
                {
                    args = new List<string>(2);
                    result.Add(key, args);
                }

                var value = HttpUtility.UrlDecode(pvArgValue[1]);
                args.Add(value);
            }
            return result;
        }

        private static string GetDeviceId(string deviceId, Dictionary<string, IList<string>> pvArgs, string deviceIdParam)
        {
            if (deviceId != null)
            {
                return deviceId;
            }

            if (!pvArgs.TryGetValue(deviceIdParam, out var deviceIdFromParam))
            {
                return null;
            }

            return deviceIdFromParam[0];
        }

        private RegisterValue? GetRegisterValue(Dictionary<string, IList<string>> pvArgs, string param, ObisCode obisCode, Unit unit)
        {
            if (string.IsNullOrEmpty(param))
            {
                return null;
            }

            if (!pvArgs.TryGetValue(param, out var pvParams))
            {
                return null;
            }

            var valueString = pvParams[0];
            if (valueString == null)
            {
                return null;
            }

            int value;
            if (!int.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                logger.LogWarning("Failed to parse PvOutput numeric value {Value}.", valueString);
                return null;
            }
            return new RegisterValue(obisCode, value, 0, unit);
        }
    }
}