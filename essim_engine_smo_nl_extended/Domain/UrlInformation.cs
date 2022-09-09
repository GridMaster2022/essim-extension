using System;
using System.Linq;
using System.Net;

namespace essim_engine_smo_nl_extended.Domain
{
    public class UrlInformation
    {
        public string Url { get; }
        public string Host { get; }
        public string IpAddress { get; }

        public UrlInformation(string url)
        {
            Url = url;
            Host = GetHostFromUrl(Url);
            IpAddress = GetIpFromHostName(Host);
        }

        private string GetHostFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                Uri uriInformation = new Uri(url);
                return uriInformation.Host;
            }
            catch
            {
                return "**ERROR**";
            }
        }

        private string GetIpFromHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) return null;
            if (hostName == "**ERROR**") return null;

            try
            {
                return Dns.GetHostEntry(hostName).AddressList.First(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            }
            catch
            {
                return "**ERROR**";
            }
        }
    }
}
