using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Instagram_Reels_Bot.Helpers
{
	public class Whitelist
	{
		public static List<ulong> WhitelistedServers = new List<ulong>();
		public static bool ListSet = false;
		public static bool WhitelistEnabled = false;

		public static bool IsServerOnList(ulong serverID)
        {
			// Load the list in:
            if (!ListSet)
            {
				SetWhitelistEnabledBool();
                if (WhitelistEnabled)
                {
					LoadList();
                }
                else
                {
					ListSet = true;
					return true; // whitelist disabled, then IsServerOnList returning true
				}
            }
			else if (WhitelistedServers.Count==0)
            {
				// No whitelist enabled:
				return true;
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

			if (config["Whitelist"] != null) // check if whitelist is found on config
			{
				WhitelistEnabled = config["Whitelist"].ToLower().Equals("true");
			}
			else
			{
				WhitelistEnabled = false; // whitelist disabled by default (not found on config.json)
			}
		}

		/// <summary>
        /// Load the list of whitelisted servers.
        /// </summary>
		private static void LoadList()
        {
			// Set the bool to true:
			ListSet = true;

			// Load in the servers:
			try
			{
				string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "whitelist.txt");
				foreach(string line in lines)
                {
                    if (!line.StartsWith('#'))
                    {
                        try
                        {
							WhitelistedServers.Add(ulong.Parse(line));
                        }
						catch(Exception e)
                        {
							Console.WriteLine("Error loading whitelist line. Error: " + e);
                        }
                    }
                }

			}
			catch(Exception e)
            {
				Console.WriteLine("Error loading whitelist. Error: " + e);
            }
		}
	}
}

