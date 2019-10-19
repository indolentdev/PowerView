using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using log4net;
using Mixpanel.NET;
using Mixpanel.NET.Events;
using PowerView.Model.Repository;

namespace PowerView.Service
{
  public class UsageMonitor : IUsageMonitor
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private const string token = "";
    private volatile static string id;

    private readonly IpMixpanelHttp http;
    private readonly MixpanelTracker client;

    public UsageMonitor(ISettingRepository settingRepository)
    {
      if (settingRepository == null) throw new ArgumentNullException("settingRepository");

#if !DEBUG
      if (string.IsNullOrEmpty(id))
      {
        id = settingRepository.ProvideInstallationId();
      }
#else
      id = string.Empty;
#endif

      if (!string.IsNullOrEmpty(token))
      {
        http = new IpMixpanelHttp();
        client = new MixpanelTracker(token, http, new TrackerOptions { SetEventTime = false });
      }
    }

    public void TrackDing(string sqliteVersion, string monoRuntimeVersion)
    {
      var props = new Dictionary<string, object>
      {
        { "Env Processor Count", ToStringInvariant(Environment.ProcessorCount) },
        { "Env IsLittleEndian", ToString(BitConverter.IsLittleEndian) },
        { "Env OS Version", ToString(Environment.OSVersion) },
        { "Env Is 64bit OS", ToString(Environment.Is64BitOperatingSystem) },

        { "Env CLR Version", ToString(Environment.Version) },

        { "Env Is 64bit process", ToString(Environment.Is64BitProcess) },

        { "Env Sqlite Version", sqliteVersion != null ? sqliteVersion : "null" },
        { "Env Mono Runtime Version", monoRuntimeVersion != null ? monoRuntimeVersion : "null" },
      };
      Track("Ding", props);
    }

    private static string ToStringInvariant(IFormattable o)
    {
      return o.ToString("{0}", CultureInfo.InvariantCulture);
    }

    private static string ToString(object o)
    {
      return o.ToString();
    }

    private void Track(string eventName, IDictionary<string, object> props = null)
    {
      if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id))
      {
        return;
      }

      if (props == null)
      {
        props = new Dictionary<string, object>(1);
      }
      props.Add("distinct_id", id);

      var errorMsg = string.Format(CultureInfo.InvariantCulture, "Tracked event: {0}. Failed.", eventName);
      try
      {
        var success = client.Track(eventName, props);
        log.DebugFormat("Tracked event: {0}. Success:{1}", eventName, success);
      } // Catch all kinds of stuff neglected in the lib
      catch (InvalidOperationException e) // Covers WebException
      {
        log.Debug(errorMsg, e);
      }
      catch (System.Security.SecurityException e)
      {
        log.Debug(errorMsg, e);
      }
      catch (System.IO.IOException e)
      {
        log.Debug(errorMsg, e);
      }
      catch (FormatException e)
      {
        log.Debug(errorMsg, e);
      }
    }

    private class IpMixpanelHttp : IMixpanelHttp
    {
      private IMixpanelHttp backing = new MixpanelHttp();

      public string Get(string uri, string query)
      {
        var query2 = Annotate(query);
        return backing.Get(uri, query2);
      }

      public string Post(string uri, string body)
      {
        var body2 = Annotate(body);
        return backing.Post(uri, body2);
      }

      private static string Annotate(string part)
      {
        return part + "&ip=1";
      }
    }

  }
}
