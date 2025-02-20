using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PowerView.Service.Test;

public class HttpMessageHandlerStub : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendAsync;

    public HttpMessageHandlerStub(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendAsync)
    {
        this.sendAsync = sendAsync;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return sendAsync(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Send(request, cancellationToken));
    }
}