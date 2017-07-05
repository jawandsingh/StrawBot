using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrawBot
{
    public static class BaseAddress
    {
        private static readonly object Lock = new object();
        public static string[] AvailableAddresses =
        {
            "http://google.com",
            "http://twitch.tv",
            "http://github.com",
        };

        public static string GetAddress()
        {
            lock (Lock)
            {
                var random = new Random().Next(0, AvailableAddresses.Length);
                return AvailableAddresses[random];
            }
        }
    }
}
