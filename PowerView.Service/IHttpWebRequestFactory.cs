using System;

namespace PowerView.Service
{
  public interface IHttpWebRequestFactory
  {
    IHttpWebRequest Create(Uri requestUri);
  }
}

