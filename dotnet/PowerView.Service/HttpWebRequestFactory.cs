using System;

namespace PowerView.Service
{
  internal class HttpWebRequestFactory : IHttpWebRequestFactory
  {
    public IHttpWebRequest Create(Uri requestUri)
    {
      return new WrapHttpWebRequest(requestUri);
    }
  }
}

