using System;
using System.IO;
using System.Net;
using System.Web;

namespace PowerView.Service
{
  internal class WrapHttpWebRespone : IHttpWebResponse
  {
    private readonly HttpWebResponse response;

    public WrapHttpWebRespone(HttpWebResponse response)
    {
      if (response == null) throw new ArgumentNullException("response");

      this.response = response;
    }

    #region IHttpWebResponse implementation
    public HttpStatusCode StatusCode
    {
      get { return response.StatusCode; }
    }

    public string ContentType
    {
      get { return response.ContentType; }
    }

    public WebHeaderCollection Headers
    {
      get { return response.Headers; }
    }

    public Stream GetResponseStream()
    {
      return response.GetResponseStream();
    }
    #endregion

    #region IDisposable implementation
    public void Dispose() 
    {
      Dispose(true);
      GC.SuppressFinalize(this);      
    }

    ~WrapHttpWebRespone()
    {
      // Finalizer calls Dispose(false)
      Dispose(false);
    }

    private bool disposed;
    protected virtual void Dispose(bool disposing)
    {
      if (disposed)
      {
        return;
      }

      if (disposing) 
      {
        response.Dispose();
      }
      disposed = true;   
    }
    #endregion
    }
}

