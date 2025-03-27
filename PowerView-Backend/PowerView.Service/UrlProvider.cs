using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace PowerView.Service
{
    public class UrlProvider : IUrlProvider
    {
        private readonly IOptions<ServiceOptions> options;

        public UrlProvider(IOptions<ServiceOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Uri GetEventsUrl()
        {
            //      return new Uri(GetBaseUri(), "Content/index.html#!/events");
            return GetBaseUri();
        }

        private Uri GetBaseUri()
        {
            var baseUrlString = options.Value.BaseUrl;
            if (baseUrlString.Contains("://*"))
            {
                baseUrlString = baseUrlString.Replace("://*", "://localhost");
            }

            var baseUri = new Uri(baseUrlString);
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

        private static string GetIPv4Address()
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