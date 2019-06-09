using System;
using System.IO;
using System.Net;

namespace PowerView.Service.Test
{
  public class HttpWebRequestMock : IHttpWebRequest
  {
    private readonly WebHeaderCollection headers;
    private byte[] content;

    private IHttpWebResponse response;
    private Exception responseException;

    private Exception requestStreamException;

    public HttpWebRequestMock()
    {
      headers = new WebHeaderCollection();
      content = new byte[0];
    }

    public void SetResponse(IHttpWebResponse response)
    {
      this.response = response;
    }

    public void SetResponse(Exception responseException)
    {
      this.responseException = responseException;
    }

    public void SetRequestStream(Exception requestStreamException)
    {
      this.requestStreamException = requestStreamException;
    }

    public byte[] GetContentBytes()
    {
      return content;
    }

    #region IHttpWebRequest implementation

    public Stream GetRequestStream()
    {
      if (requestStreamException != null)
      {
        throw requestStreamException;
      }
      var ms = new SpecialMemoryStream(b => content = b);
      return ms;
    }

    public IHttpWebResponse GetResponse()
    {
      if ( responseException != null )
      {
        throw responseException;
      }
      return response;
    }

    public string Method { get; set; }
    public int Timeout { get; set; }
    public string ContentType { get; set; }
    public WebHeaderCollection Headers { get { return headers; } }

    #endregion

    private class SpecialMemoryStream : MemoryStream
    {
      private readonly Action<byte[]> action;

      public SpecialMemoryStream(Action<byte[]> action)
      {
        this.action = action;
      }

      protected override void Dispose(bool disposing)
      {
        action(ToArray());
        base.Dispose(disposing);
      }
    }

  }
}

