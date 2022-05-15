using System.IO;
using System.Net;

namespace PowerView.Service
{
  public interface IHttpWebRequest
  {
    string Method { get; set; }
    int Timeout { get; set; }
    string ContentType { get; set; }
    WebHeaderCollection Headers { get; }

    Stream GetRequestStream();
    IHttpWebResponse GetResponse();
  }
}

