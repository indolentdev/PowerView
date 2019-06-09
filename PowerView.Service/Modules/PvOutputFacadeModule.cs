using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Nancy;
using log4net;

using PowerView.Model;
using PowerView.Service.Mappers;

namespace PowerView.Service.Modules
{
  public class PvOutputFacadeModule : CommonNancyModule
  {
    protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IReadingAccepter readingAccepter;
    private readonly ILiveReadingMapper liveReadingMapper;
    private readonly IPvOutputFacadeModuleConfigProvider moduleConfigProvider;
    private readonly IHttpWebRequestFactory httpWebRequestFactory;

    public PvOutputFacadeModule(IReadingAccepter readingAccepter, ILiveReadingMapper liveReadingMapper, IPvOutputFacadeModuleConfigProvider moduleConfigProvider, IHttpWebRequestFactory httpWebRequestFactory)
      :base("/service/r2")
    {
      if (readingAccepter == null) throw new ArgumentNullException("readingAccepter");
      if (liveReadingMapper == null) throw new ArgumentNullException("liveReadingMapper");
      if (moduleConfigProvider == null) throw new ArgumentNullException("moduleConfigProvider");
      if (httpWebRequestFactory == null) throw new ArgumentNullException("httpWebRequestFactory");

      this.readingAccepter = readingAccepter;
      this.liveReadingMapper = liveReadingMapper;
      this.moduleConfigProvider = moduleConfigProvider;
      this.httpWebRequestFactory = httpWebRequestFactory;

      Get["/addstatus.jsp"] = AddStatus;
      Post["/addstatus.jsp"] = AddStatus;
    }

    private dynamic AddStatus(dynamic param)
    {
      IHttpWebResponse forwardResponse;
      try
      {
        var forwardRequest = CreateForwardRequest(Request, moduleConfigProvider.PvOutputAddStatus);
        forwardResponse = forwardRequest.GetResponse();
      }
      catch (HttpWebException httpWebException)
      {
        var httpWebResponse = httpWebException.Response;
        if (httpWebResponse != null)
        {
          const string msg = "Experienced web exception forwarding PVOutput request";
          if (httpWebException.Status == System.Net.WebExceptionStatus.ConnectFailure)
          {
            log.DebugFormat(msg + ". Message:{0} - Status:{1}", httpWebException.Message, httpWebException.Status);
          }
          else
          {
            log.Debug(msg, httpWebException);
          }
          var errorResponse = CreateForwardResponse(httpWebResponse);
          return errorResponse;
        }
        log.Error("Experienced web exception forwarding PVOutput request. Failed generating response. Responding with general error", httpWebException);
        var generalErrorResponse = new Response
        {
          StatusCode = HttpStatusCode.InternalServerError,
          ContentType = "text/plain; charset=utf-8",
          Contents = stream => (new StreamWriter(stream) { AutoFlush = true })
            .Write("Internal error interacting with PVOutput. Error message:" + httpWebException.Message)
        };
        return generalErrorResponse;
      }

      StoreValues(Request);

      var response = CreateForwardResponse(forwardResponse);
      return response;
    }

    private IHttpWebRequest CreateForwardRequest(Request request, Uri pvOutputAddStatus)
    {
      var requestUrl = request.Url;
      var pvOutputAddStatusUrl = new Uri(pvOutputAddStatus, requestUrl.Query);
      var forwardRequest = httpWebRequestFactory.Create(pvOutputAddStatusUrl);
      forwardRequest.Method = request.Method;
      forwardRequest.ContentType = request.Headers.ContentType;

      CopyHeader("X-Pvoutput-Apikey", request, forwardRequest);
      CopyHeader("X-Pvoutput-SystemId", request, forwardRequest);

      CopyContent(request, forwardRequest);

      return forwardRequest;
    }

    private static Response CreateForwardResponse(IHttpWebResponse forwardResponse)
    {
      var response = new Response();
      response.StatusCode = (HttpStatusCode)((int)forwardResponse.StatusCode);
      response.ContentType = forwardResponse.ContentType;

      CopyHeader("X-Rate-Limit-Remaining", forwardResponse.Headers, response.Headers);
      CopyHeader("X-Rate-Limit-Limit", forwardResponse.Headers, response.Headers);
      CopyHeader("X-Rate-Limit-Reset", forwardResponse.Headers, response.Headers);

      CopyContent(forwardResponse, response);

      forwardResponse.Dispose();

      return response;
    }

    private static void CopyHeader(string header, Request request, IHttpWebRequest forwardRequest)
    {
      if ( !request.Headers.Keys.Contains(header, new CaseInsensitiveStringEqualityComparer()) )
      {
        return;
      }
      forwardRequest.Headers.Add(header, request.Headers[header].First());
    }

    private static void CopyHeader(string header, NameValueCollection request, IDictionary<string, string> forwardResponse)
    {
      if (!request.AllKeys.Contains(header, new CaseInsensitiveStringEqualityComparer()))
      {
        return;
      }
      forwardResponse.Add(header, request[header]);
    }

    private static void CopyContent(Request request, IHttpWebRequest forwardRequest)
    {
      byte[] contentBytes;
      using (var memoryStream = new MemoryStream())
      {
        request.Body.CopyTo(memoryStream);
        request.Body.Position = 0; // rewind for reading to db storage
        memoryStream.Position = 0; // rewind for reading to array
        contentBytes = memoryStream.ToArray();
      }
      using (var forwardRequestContentStream = forwardRequest.GetRequestStream())
      {
        forwardRequestContentStream.Write(contentBytes, 0, contentBytes.Length);
      }
    }

    private static void CopyContent(IHttpWebResponse forwardResponse, Response response)
    {
      byte[] contentBytes;
      using (var ms = new MemoryStream())
      {
        using (var responseStream = forwardResponse.GetResponseStream())
        {
          responseStream.CopyTo(ms);
          contentBytes = ms.ToArray();
        }
      }
      response.Contents = c => c.Write(contentBytes, 0, contentBytes.Length);
    }

    private void StoreValues(Request request)
    {
      if (string.IsNullOrEmpty(moduleConfigProvider.PvDeviceLabel))
      {
        log.InfoFormat("Configuration PvDeviceLabel is empty or absent. Configuration must be set to capture PV readings.");
        return;
      }
      
      var liveReading = liveReadingMapper.MapPvOutputArgs(request.Url, request.Headers.ContentType, request.Body,
        moduleConfigProvider.PvDeviceLabel, moduleConfigProvider.PvDeviceSerialNumber, moduleConfigProvider.PvDeviceSerialNumberParam,
        moduleConfigProvider.ActualPowerP23L1Param, moduleConfigProvider.ActualPowerP23L2Param, moduleConfigProvider.ActualPowerP23L3Param);
      if (liveReading == null)
      {
        return;
      }
      readingAccepter.Accept(new LiveReading[] { liveReading } );
    }

  }
}

