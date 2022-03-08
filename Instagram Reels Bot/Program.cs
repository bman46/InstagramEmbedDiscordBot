using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Instagram_Reels_Bot.Services;
using System.IO;
using Discord.Interactions;
using Instagram_Reels_Bot.Helpers;

namespace Instagram_Reels_Bot
{
    class Program
    {
        // setup our fields we assign later
        private readonly IConfiguration _config;
        private DiscordShardedClient _client;
        private InteractionService _interact;

        public static async Task Main(string[] args)
        {
            await new Program().MainAsync();
        }

        public Program()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();

            // Load the accounts
            InstagramProcessor.AccountFinder.LoadAccounts();

            // Create state file directory (if not existant)
            string stateFileDir;
            if (_config["StateFile"] != null && _config["StateFile"] != "")
            {
                stateFileDir = Path.Combine(_config["StateFile"]);
            }
            else
            {
                stateFileDir = Path.Combine(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "StateFiles");
            }
            if (!Directory.Exists(stateFileDir))
            {
                Directory.CreateDirectory(stateFileDir);
            }
        }

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            using (var services = ConfigureServices())
            {
                // get the client and assign to client 
                // you get the services via GetRequiredService<T>
                var client = services.GetRequiredService<DiscordShardedClient>();
                var interact = services.GetRequiredService<InteractionService>();
                _client = client;
                _interact = interact;

                // setup logging and the ready event
                client.Log += LogAsync;
                interact.Log += LogAsync;
                client.ShardReady += ReadyAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // this is where we get the Token value from the configuration file, and start the bot
                await client.LoginAsync(TokenType.Bot, _config["Token"]);
                await client.StartAsync();

                //Set status:
                string status = "for Instagram links";
                ActivityType activity = ActivityType.Watching;

                if (!string.IsNullOrEmpty(_config["statusDesc"]))
                {
                    status = _config["statusDesc"];
                }
                if (!string.IsNullOrEmpty(_config["statusActivity"]))
                {
                    if (!Enum.TryParse(_config["statusActivity"], out activity))
                    {
                        Console.WriteLine("Could not find 'statusActivity' value in enum.");

                        //Default to Watching:
                        activity = ActivityType.Watching;
                    }
                }

                await client.SetActivityAsync(new Game(status, activity));

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                //Start the subscription service:
                services.GetRequiredService<Subscriptions>().Initialize();

                await Task.Delay(-1);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task<Task> ReadyAsync(DiscordSocketClient shard)
        {
            Console.WriteLine(shard.ShardId+" Shard Ready");

            //Register Slash Commands:
            Console.WriteLine("Register Commands...");
            if (IsDebug())
            {
                Console.WriteLine("Per guild.");
                foreach (SocketGuild guild in _client.Guilds)
                {
                    await _interact.RegisterCommandsToGuildAsync(guild.Id);
                }
            }
            else
            {
                Console.WriteLine("Global");
                await _interact.RegisterCommandsGloballyAsync(true);
            }

            return Task.CompletedTask;
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        private ServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using .Services;
            // the config we build is also added, which comes in handy for setting the command prefix!
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordShardedClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<InteractionService>()
                .AddSingleton<Subscriptions>()
                .BuildServiceProvider();
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
