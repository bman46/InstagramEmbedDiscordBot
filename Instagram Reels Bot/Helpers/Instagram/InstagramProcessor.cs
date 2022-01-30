using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Logger;
using Microsoft.Extensions.Configuration;
using OtpNet;
using System.Linq;
using System.Net;
using Instagram_Reels_Bot.Helpers.Instagram;

namespace Instagram_Reels_Bot.Helpers
{
    public class InstagramProcessor
	{
        /// <summary>
        /// The IG processor for the account:
        /// </summary>
        public InstagramApiSharp.API.IInstaApi instaApi;

        /// <summary>
        /// Class responsible for getting an account to use the IG Processor with
        /// </summary>
        public static class AccountFinder
        {
            /// <summary>
            /// List of accounts
            /// </summary>
            public static List<IGAccount> Accounts = new List<IGAccount>();

            /// <summary>
            /// Loads accounts from the Config file:
            /// </summary>
            public static void LoadAccounts()
            {
                // create the configuration
                var _builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(path: "config.json");

                // build the configuration and assign to _config          
                var config = _builder.Build();

                //Add accounts to the array:
                List<IGAccount> creds = config.GetSection("IGAccounts").Get<List<IGAccount>>();
                Accounts = creds;
            }
            /// <summary>
            /// Gets a valid IG account.
            /// </summary>
            /// <returns>An IG Account</returns>
            /// <exception cref="InvalidDataException">No accounts avaliable</exception>
            public static IGAccount GetIGAccount()
            {
                //Randomize the accounts
                Random rand = new Random();
                var shuffledAccounts = Accounts.OrderBy(x => rand.Next()).ToList();

                //Find a valid account
                foreach (IGAccount cred in shuffledAccounts)
                {
                    TimeOnly timeNow = TimeOnly.FromDateTime(DateTime.Now);
                    if (!cred.FailedLogin)
                    {
                        if (cred.UsageTimes.Count > 0)
                        {
                            // Check valid times:
                            foreach (IGAccount.OperatingTime time in cred.UsageTimes)
                            {
                                if (time.BetweenStartAndEnd(timeNow))
                                {
                                    return cred;
                                }
                            }
                        }
                        else
                        {
                            // Warn about not setting valid times:
                            Console.WriteLine("Warning: No time set on account " + cred.UserName+". Using the account.");
                            return cred;
                        }
                    }
                }
                throw new InvalidDataException("No avaliable accounts.");
            }
        }
    }
}