using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
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
    private readonly IHttpClientFactory httpClientFactory;

    public PvOutputFacadeController(ILogger<PvOutputFacadeController> logger, IReadingAccepter readingAccepter, ILiveReadingMapper liveReadingMapper, IOptions<PvOutputOptions> options, IHttpClientFactory httpClientFactory)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readingAccepter = readingAccepter ?? throw new ArgumentNullException(nameof(readingAccepter));
        this.liveReadingMapper = liveReadingMapper ?? throw new ArgumentNullException(nameof(liveReadingMapper));
        this.options = options.Value;
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    [HttpGet("addstatus.jsp")]
    [HttpPost("addstatus.jsp")]
    public ActionResult AddStatus()
    {
        ControllerContext.HttpContext.Features.Get<IHttpBodyControlFeature>().AllowSynchronousIO = true;
        Request.EnableBuffering();

        var forwardRequest = CreateForwardRequest(Request, options.PvOutputAddStatusUrl);
        HttpResponseMessage forwardResponse;
        try
        {
            using var httpClient = httpClientFactory.CreateClient(nameof(PvOutputFacadeController));
            forwardResponse = httpClient.Send(forwardRequest, HttpCompletionOption.ResponseContentRead);
        }
        catch (HttpRequestException httpRequestException)
        {
            logger.LogInformation(httpRequestException, $"Experienced error forwarding request to PVOutput. Request:{forwardRequest}");

            Response.StatusCode = 500;
            return new EmptyResult();
        }

        try
        {
            forwardResponse.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpRequestException)
        {
            logger.LogInformation(httpRequestException, $"Experienced error processing response from PVOutput. Request:{forwardRequest}, Response:{forwardResponse}");

            SetResponse(forwardResponse);
            forwardResponse.Dispose();
            return new EmptyResult();
        }

        StoreValues(Request);

        SetResponse(forwardResponse);
        forwardResponse.Dispose();
        return new EmptyResult();
    }

    private HttpRequestMessage CreateForwardRequest(HttpRequest request, Uri pvOutputAddStatus)
    {
        var method = new HttpMethod(request.Method);
        var pvOutputAddStatusUrl = new Uri(pvOutputAddStatus, request.QueryString.ToString());
        var forwardRequest = new HttpRequestMessage(method, pvOutputAddStatusUrl);

        CopyHeader("X-Pvoutput-Apikey", request, forwardRequest);
        CopyHeader("X-Pvoutput-SystemId", request, forwardRequest);

        CopyContent(request, forwardRequest);

        return forwardRequest;
    }

    private void SetResponse(HttpResponseMessage forwardResponse)
    {
        Response.ContentType = forwardResponse.Content.Headers.ContentType?.ToString();
        Response.StatusCode = (int)forwardResponse.StatusCode;

        CopyHeader("X-Rate-Limit-Remaining", forwardResponse.Headers, Response.Headers);
        CopyHeader("X-Rate-Limit-Limit", forwardResponse.Headers, Response.Headers);
        CopyHeader("X-Rate-Limit-Reset", forwardResponse.Headers, Response.Headers);

        CopyContent(forwardResponse, Response);

        forwardResponse.Dispose();
    }

    private static void CopyHeader(string header, HttpRequest request, HttpRequestMessage forwardRequest)
    {
        if (!request.Headers.ContainsKey(header))
        {
            return;
        }
        IEnumerable<string> headerValues = request.Headers[header];
        forwardRequest.Headers.Add(header, headerValues);
    }

    private static void CopyHeader(string header, HttpResponseHeaders forwardResponse, IHeaderDictionary response)
    {
        if (!forwardResponse.Contains(header))
        {
            return;
        }

        response.Add(header, forwardResponse.GetValues(header).ToArray());
    }

    private static void CopyContent(HttpRequest request, HttpRequestMessage forwardRequest)
    {
        if (request.ContentType == null)
        {
            return;
        }

        byte[] contentBytes;
        using (var memoryStream = new MemoryStream())
        {
            request.Body.CopyTo(memoryStream);
            request.Body.Position = 0; // rewind for reading to db storage
            memoryStream.Position = 0; // rewind for reading to array
            contentBytes = memoryStream.ToArray();
        }
        forwardRequest.Content = new ByteArrayContent(contentBytes);
        forwardRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
    }

    private static void CopyContent(HttpResponseMessage forwardResponse, HttpResponse response)
    {
        byte[] contentBytes;
        using (var ms = new MemoryStream())
        {
            using (var responseStream = forwardResponse.Content.ReadAsStream())
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
