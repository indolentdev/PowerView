using System.IO;
using System.Net;

namespace PowerView.Service.IntegrationTest
{
  public class HttpWebResponseMock : IHttpWebResponse
  {
    private readonly WebHeaderCollection headers;
    private byte[] content;

    public HttpWebResponseMock()
    {
      headers = new WebHeaderCollection();
      content = new byte[0];
    }

    public void SetContentBytes(byte[] bytes)
    {
      content = bytes;
    }
      
    #region IHttpWebResponse implementation

    public Stream GetResponseStream()
    {
      return new MemoryStream(content);
    }

    public HttpStatusCode StatusCode{ get; set; }
    public string ContentType { get; set; }
    public WebHeaderCollection Headers { get { return headers; } }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
    }

    #endregion
  }
}

