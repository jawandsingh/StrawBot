using System;
using System.Net;
using System.Threading.Tasks;

namespace StrawBot.Helpers
{
    public static class ProxyHelpers
    {
        public static async Task<bool> CheckProxyAsync(string address)
        {
            try
            {
                var webProxy = new WebProxy(address);
                webProxy.Credentials = new NetworkCredential("scottn1991", "temp00129");
                var httpClient = HttpClientHelper.CreateHttpClient(webProxy);
                var responseMessage =  await httpClient.GetAsync("http://www.strawpoll.me/embed_1");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}