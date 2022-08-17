using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;
using Microsoft.AspNetCore.Http.Extensions;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("/service/r2")]
public class PvOutputFacadeController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IReadingAccepter readingAccepter;
    private readonly ILiveReadingMapper liveReadingMapper;
    private readonly PvOutputOptions options;
    private readonly IHttpWebRequestFactory httpWebRequestFactory;

    public PvOutputFacadeController(ILogger<PvOutputFacadeController> logger, IReadingAccepter readingAccepter, ILiveReadingMapper liveReadingMapper, IOptions<PvOutputOptions> options, IHttpWebRequestFactory httpWebRequestFactory)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
        this.liveReadingMapper = liveReadingMapper ?? throw new ArgumentNullException(nameof(liveReadingMapper));
        this.options = options.Value;
        this.httpWebRequestFactory = httpWebRequestFactory ?? throw new ArgumentNullException(nameof(httpWebRequestFactory));
    }

    [HttpGet("addstatus.jsp")]
    [HttpPost("addstatus.jsp")]
    public ActionResult AddStatus()
    {
        ControllerContext.HttpContext.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO = true;
        Request.EnableBuffering();

        IHttpWebResponse forwardResponse;
        try
        {
            var forwardRequest = CreateForwardRequest(Request, options.PvOutputAddStatusUrl);
            forwardResponse = forwardRequest.GetResponse();
        }
        catch (HttpWebException httpWebException)
        {
            var forwardErrorResponse = httpWebException.Response;
            if (forwardErrorResponse != null)
            {
                const string msg = "Experienced web exception forwarding PVOutput request";
                if (httpWebException.Status == System.Net.WebExceptionStatus.ConnectFailure)
                {
                    logger.LogDebug(msg + $". Message:{httpWebException.Message} - Status:{httpWebException.Status}");
                }
                else
                {
                    logger.LogDebug(httpWebException, msg);
                }
                SetResponse(forwardErrorResponse);
                return new EmptyResult();
            }
            logger.LogError(httpWebException, "Experienced web exception forwarding PVOutput request. Failed generating response. Responding with general error");
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal error interacting with PVOutput. Error message:{httpWebException.Message}");
        }

        StoreValues(Request);

        SetResponse(forwardResponse);
        return new EmptyResult();
    }

    private IHttpWebRequest CreateForwardRequest(HttpRequest request, Uri pvOutputAddStatus)
    {
        var pvOutputAddStatusUrl = new Uri(pvOutputAddStatus, request.QueryString.ToString());
        var forwardRequest = httpWebRequestFactory.Create(pvOutputAddStatusUrl);
        forwardRequest.Method = request.Method;
        forwardRequest.ContentType = request.Headers.ContentType;

        CopyHeader("X-Pvoutput-Apikey", request, forwardRequest);
        CopyHeader("X-Pvoutput-SystemId", request, forwardRequest);

        CopyContent(request, forwardRequest);

        return forwardRequest;
    }

    private void SetResponse(IHttpWebResponse forwardResponse)
    {
        Response.ContentType = forwardResponse.ContentType;
        Response.StatusCode = (int)forwardResponse.StatusCode;

        CopyHeader("X-Rate-Limit-Remaining", forwardResponse.Headers, Response.Headers);
        CopyHeader("X-Rate-Limit-Limit", forwardResponse.Headers, Response.Headers);
        CopyHeader("X-Rate-Limit-Reset", forwardResponse.Headers, Response.Headers);

        CopyContent(forwardResponse, Response);

        forwardResponse.Dispose();
    }

    private static void CopyHeader(string header, HttpRequest request, IHttpWebRequest forwardRequest)
    {
        if (!request.Headers.Keys.Contains(header, new CaseInsensitiveStringEqualityComparer()))
        {
            return;
        }
        forwardRequest.Headers.Add(header, request.Headers[header].First());
    }

    private static void CopyHeader(string header, System.Net.WebHeaderCollection forwardResponse, IHeaderDictionary response)
    {
        if (!forwardResponse.AllKeys.Contains(header, new CaseInsensitiveStringEqualityComparer()))
        {
            return;
        }
        response.Add(header, forwardResponse[header]);
    }

    private static void CopyContent(HttpRequest request, IHttpWebRequest forwardRequest)
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

    private static void CopyContent(IHttpWebResponse forwardResponse, HttpResponse response)
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
        response.Body.Write(contentBytes, 0, contentBytes.Length);
    }

    private void StoreValues(HttpRequest request)
    {
        if (string.IsNullOrEmpty(options.PvDeviceLabel))
        {
            logger.LogInformation("Configuration PvDeviceLabel is empty or absent. PvDeviceLabel must be configured for PowerView to capture PV readings.");
            return;
        }

        var url = new Uri(request.GetEncodedUrl());
        var liveReading = liveReadingMapper.MapPvOutputArgs(url, request.Headers.ContentType, request.Body,
          options.PvDeviceLabel, options.PvDeviceId, options.PvDeviceIdParam,
          options.ActualPowerP23L1Param, options.ActualPowerP23L2Param, options.ActualPowerP23L3Param);
          
        if (liveReading == null)
        {
            return;
        }
        readingAccepter.Accept(new LiveReading[] { liveReading });
    }

}
