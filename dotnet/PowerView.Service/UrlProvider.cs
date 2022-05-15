using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PowerView.Service
{
  public class UrlProvider : IUrlProvider
  {
    private readonly Uri baseUri;
    
    public UrlProvider(Uri baseUri)
    {
      this.baseUri = baseUri;
    }

    public Uri GetEventsUrl()
    {
//      return new Uri(GetBaseUri(), "Content/index.html#!/events");
      return GetBaseUri();
    }

    private Uri GetBaseUri()
    {
      if (baseUri.IsLoopback)
      {
        var ipAddress = GetIPv4Address();
        if (!string.IsNullOrEmpty(ipAddress))
        {
          var builder = new UriBuilder(baseUri) { Host = ipAddress };
          return builder.Uri;
        }
      }

      return baseUri;
    }

    private string GetIPv4Address()
    {
      string output = null;
      foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
      {
        if (item.OperationalStatus == OperationalStatus.Up)
        {
          foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
          {
            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
            {
              output = ip.Address.ToString();
            }
          }
        }
      }
      return output;
    }
  }
}