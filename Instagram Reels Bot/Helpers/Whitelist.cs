using System.Collections.Generic;
using System.IO;
using System.Linq;

using Instagram_Reels_Bot.Helpers.Extensions;
using Microsoft.Extensions.Configuration;
using System;

namespace Instagram_Reels_Bot.Helpers
{
	public class Whitelist
	{
		public const ulong MainBotId = 815695225678463017;

        public static List<ulong> WhitelistedServers { get; } = new List<ulong>();
		public static bool ListSet { get; private set; } = false;
		public static bool WhitelistEnabled { get; private set; } = false;

		/// <summary>
        /// Checks to see if a server is on the list.
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
		public static bool IsServerOnList(ulong serverID)
        {
            // If Whitelist is empty, everything is whitelisted
            if (ListSet && WhitelistedServers.Count == 0) {
				return true;
			}

            if (!ListSet)
            {
				// Check config file for whitelist enabled bool:
				SetWhitelistEnabledBool();

                if (!WhitelistEnabled) {
                    ListSet = true;
                    return true; // whitelist disabled, then IsServerOnList returning true
                }

                LoadList();
            }

			// Check to see if server is listed:
			return WhitelistedServers.Contains(serverID);
        }

		/// <summary>
        /// Check to see if whitelist is enabled in the config.
        /// </summary>
		private static void SetWhitelistEnabledBool()
        {
			var _builder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile(path: "config.json");

			// build the configuration and assign to _config          
			var config = _builder.Build();

			WhitelistEnabled = config.Is("Whitelist", true);
		}

		/// <summary>
        /// Load the list of whitelisted servers.
        /// </summary>
		private static void LoadList()
        {
			// Set the bool to true:
			ListSet = true;

            // Load in the servers:
            string[] lines;

            string whiteListFile = Path.Combine(Directory.GetCurrentDirectory(), "whitelist.txt");

            if (!File.Exists(whiteListFile)) {
                File.CreateText(whiteListFile);
                return;
            }

            try {
                lines = File.ReadAllLines(whiteListFile);
            }  catch (IOException e) {
                Console.WriteLine("Error reading whitelist file. Error: " + e);
                return;
            }

            foreach ((string line, int lineNumber) in lines.Select((x, i) => (x, i))) {
                if (line.StartsWith('#')) {
                    continue;
                }

                if (!ulong.TryParse(line, out ulong id)) {
                    Console.WriteLine($"Error reading id on line {lineNumber}");
                    continue;
                }

			    WhitelistedServers.Add(id);
			}
		}
	}
}

