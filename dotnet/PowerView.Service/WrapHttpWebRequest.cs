using System;
using System.IO;
using System.Net;

namespace PowerView.Service
{
  internal class WrapHttpWebRequest : IHttpWebRequest
  {
    private readonly HttpWebRequest request;

    public WrapHttpWebRequest(Uri requestUri)
    {
      if (requestUri == null) throw new ArgumentNullException("requestUri");

      request = (HttpWebRequest)WebRequest.Create(requestUri);
    }

    #region IHttpWebRequest implementation

    public string Method
    {
      get { return request.Method; }
      set { request.Method = value; }
    }

    public int Timeout
    {
      get { return request.Timeout; }
      set { request.Timeout = value; }
    }

    public string ContentType
    {
      get { return request.ContentType; }
      set { request.ContentType = value; }
    }

    public WebHeaderCollection Headers
    {
      get { return request.Headers; }
    }

    public Stream GetRequestStream()
    {
      try
      {
        return request.GetRequestStream();
      }
      catch (WebException webException)
      {
        throw new HttpWebException(webException);
      }
    }

    public IHttpWebResponse GetResponse()
    {
      try
      {
        var response = (HttpWebResponse)request.GetResponse();
        return new WrapHttpWebRespone(response);
      }
      catch (WebException webException)
      {
        throw new HttpWebException(webException);
      }
    }

    #endregion
  }
}

