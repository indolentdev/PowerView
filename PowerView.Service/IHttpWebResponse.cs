using System;
using System.IO;
using System.Net;
using System.Web;

namespace PowerView.Service
{
  public interface IHttpWebResponse : IDisposable
  {
    HttpStatusCode StatusCode { get; }
    string ContentType { get; }
    WebHeaderCollection Headers { get; }

    Stream GetResponseStream();
  }
}

