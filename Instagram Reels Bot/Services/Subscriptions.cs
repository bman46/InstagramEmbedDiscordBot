using System;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Instagram_Reels_Bot.Services
{
    /// <summary>
    /// Allows users to get the lastest posts from an account
    /// </summary>
	public class Subscriptions
    {
        private readonly IConfiguration config;
        private readonly DiscordShardedClient client;
        private Timer UpdateTimer;

        public Subscriptions(IServiceProvider services)
        {
            // Dependancy injection:
            config = services.GetRequiredService<IConfiguration>();
            client = services.GetRequiredService<DiscordShardedClient>();
        }
        /// <summary>
        /// Starts the subscription tasks.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            if (config["AllowSubscriptions"].ToLower() == "true")
            {
                // TODO: Initialization.
                UpdateTimer = new Timer(3600000.0 * double.Parse(config["HoursToCheckForNewContent"])); //one hour in milliseconds
                UpdateTimer.Elapsed += new ElapsedEventHandler(GetLatestsPosts);
                UpdateTimer.Start();
            }
            else
            {
                Console.WriteLine("Subscriptions disabled.");
            }
        }
        async void GetLatestsPosts(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
}