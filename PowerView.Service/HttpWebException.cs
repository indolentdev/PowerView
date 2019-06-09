using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;

namespace PowerView.Service
{
  [Serializable]
  public class HttpWebException : InvalidOperationException, ISerializable
  {
    public IHttpWebResponse Response { get; private set; }
    public WebExceptionStatus Status { get; private set; }

    public HttpWebException()
    {}

    public HttpWebException(WebException templateException) 
      : base(GenerateMessage(templateException.Message, templateException.Status), templateException)
    {
      var httpWebResponse = templateException.Response as HttpWebResponse;
      if ( httpWebResponse != null )
      {
        Response = new WrapHttpWebRespone(httpWebResponse);
      }
      Status = templateException.Status;
    }

//    internal WebException(string message, Exception innerException, WebExceptionStatus status);

    public HttpWebException(string message, WebExceptionStatus status, IHttpWebResponse response)
      : base(GenerateMessage(message, status))
    {
      Response = response;
      Status = status;
    }

//    public WebException(string message, Exception innerException);

    protected HttpWebException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
    }

    public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
    }

    private static string GenerateMessage(string message, WebExceptionStatus status)
    {
      return message + " - Status:" + status;
    }
  }
}

