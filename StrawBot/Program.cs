using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StrawBot.Helpers;
using Newtonsoft.Json.Linq;

namespace StrawBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            Task.Run(ActionAsync).Wait();
            Console.WriteLine("Elapsed Time: " + stopwatch.Elapsed);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task ActionAsync()
        {
            // Title
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  _____ _                      ____        _   ");
            Console.WriteLine(" / ____| |                    |  _ \\      | |  ");
            Console.WriteLine("| (___ | |_ _ __ __ ___      _| |_) | ___ | |_ ");
            Console.WriteLine(" \\___ \\| __| '__/ _` \\ \\ /\\ / /  _ < / _ \\| __|");
            Console.WriteLine(" ____) | |_| | | (_| |\\ V  V /| |_) | (_) | |_ ");
            Console.WriteLine("|_____/ \\__|_|  \\__,_| \\_/\\_/ |____/ \\___/ \\__|   by Ethan Chrisp");

            // Get Poll ID
            Console.ResetColor();
            Console.WriteLine("\nEnter Strawpoll ID:");
            Console.ForegroundColor = ConsoleColor.White;
            string id = Console.ReadLine();
            Console.WriteLine("");

            var httpClient = HttpClientHelper.CreateHttpClient(WebProxy.GetDefaultProxy(), 10);
            var poll = await StrawpollRequests.GetPollAsync(httpClient, id);

            if (!poll.Votes.Any())
                Console.WriteLine("Error: Invalid Strawpoll ID specified.");

            // Get Poll Choice
            Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 0; i < poll.Votes.Count(); i++)
                Console.WriteLine("Choice #" + i + ": " + poll.Votes.ElementAt(i).Name);

            Console.ResetColor();
            Console.WriteLine("\nEnter Poll Choice:");
            Console.ForegroundColor = ConsoleColor.White;

            int voteId = 0;
            if (!int.TryParse(Console.ReadLine(), out voteId) && voteId >= poll.Votes.Count())
                Console.WriteLine("\nError: Invalid Choice specified.");

            // Get Proxy List
            Console.ResetColor();
            Console.WriteLine("\nEnter Proxy List Path:"); // ToDo: Get this automatically so this user doesn't have to specify a list each time
            Console.ForegroundColor = ConsoleColor.White;
            var proxyPath = Console.ReadLine();

            if (proxyPath == null)
                throw new NullReferenceException("Error: Invalid Proxy List Path specified.");

            if (!File.Exists(proxyPath))
                throw new Exception("Error: Invalid Proxy List Path specified.");

            // Get Desired Votes
            Console.ResetColor();
            Console.WriteLine("\nEnter Desired Votes: ");
            Console.ForegroundColor = ConsoleColor.White;

            int max = 0;
            if (!int.TryParse(Console.ReadLine(), out max))
                throw new Exception("Error: Invalid Desired Votes specified, insufficient Proxies available.");

            var lines = File.ReadAllLines(proxyPath);
            int total = lines.Length;
            int success = 0;
            int fail = 0;
            int threads = 0;

            // Get Desired Threads
            Console.ResetColor();
            Console.WriteLine("\nThreads:");
            Console.ForegroundColor = ConsoleColor.White;
            if (!int.TryParse(Console.ReadLine(), out threads))
                throw new Exception("Error: Invalid number of Threads specified.");


            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = threads,
            };

            Parallel.ForEach(lines, options, address => {
                Task.Run(async () => {
                    if (success <= max)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("\rSuccess: " + success + " ");
                        Console.Write("Fail: " + fail + " ");
                        Console.Write("Remaining: " + Math.Abs((success + fail) - total) + " ");
                        if (await ProxyHelpers.CheckProxyAsync(address))
                        {
                            try
                            {
                                var client = HttpClientHelper.CreateHttpClient(new WebProxy(address), 10);
                                var info = await StrawpollRequests.GetPollAsync(client, id);
                                if (await StrawpollRequests.Vote(client, id, poll.Votes.ElementAt(voteId).ID, info.Token))
                                    success++;
                            }
                            catch (Exception)
                            {
                                fail++;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error: No response from Proxy " + address + ".");
                            fail++;
                        }
                    }
                }).Wait();
            });
        }
    }
}