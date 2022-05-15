/*
using System;
using System.Globalization;
using System.Linq;
using Nancy;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace PowerView.Service.Modules
{
  public class DeviceLiveReadingModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ILiveReadingMapper liveReadingMapper;
    private readonly IReadingAccepter readingAccepter;

    public DeviceLiveReadingModule(ILiveReadingMapper liveReadingMapper, IReadingAccepter readingAccepter)
      :base("/api/devices")
    {
      if (liveReadingMapper == null) throw new ArgumentNullException("liveReadingMapper");
      if (readingAccepter == null) throw new ArgumentNullException("readingAccepter");

      this.liveReadingMapper = liveReadingMapper;
      this.readingAccepter = readingAccepter;

      Post["/livereadings"] = PostLiveReadings;
    }

    private dynamic PostLiveReadings(dynamic param)
    {
      var liveReadings = liveReadingMapper.Map(Request.Headers.ContentType, Request.Body).ToList();

      var response = new Response { StatusCode = HttpStatusCode.NoContent };
      try
      {
        readingAccepter.Accept(liveReadings);
      }
      catch (DataStoreBusyException e)
      {
        response.StatusCode = HttpStatusCode.ServiceUnavailable;
        response.ReasonPhrase = "Data store busy";

        var msg = string.Format(CultureInfo.InvariantCulture,
                                "Unable to add readings for label(s):{0}. Data store busy. Responding {1} ({2})",
                                string.Join(",", liveReadings.Select(r => r.Label)), response.StatusCode, (int)response.StatusCode);
        if (log.IsDebugEnabled)
        {
          log.Info(msg, e);
        }
        else
        {
          log.Info(msg);
        }
      }
      return response;
    }

  }
}
*/