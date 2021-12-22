using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Instagram_Reels_Bot.Services;
using System.Net;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System.IO;
using System.Collections.Generic;

namespace Instagram_Reels_Bot
{
    class Program
    {
        // setup our fields we assign later
        private readonly IConfiguration _config;
        private DiscordShardedClient _client;
        public static InstagramApiSharp.API.IInstaApi instaApi;

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            _config = _builder.Build();

            //proxy config:
            if (!string.IsNullOrEmpty(_config["ProxyURL"]))
            {
                var proxyObject = new WebProxy(_config["ProxyURL"]);
                WebRequest.DefaultWebProxy = proxyObject;
            }
            //Insta Initialize:
            var userSession = new UserSessionData
            {
                UserName = _config["IGUserName"],
                Password = _config["IGPassword"]
            };

            instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .Build();
            InstagramLogin();
        }

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            using (var services = ConfigureServices())
            {
                // get the client and assign to client 
                // you get the services via GetRequiredService<T>
                var client = services.GetRequiredService<DiscordShardedClient>();
                _client = client;

                // setup logging and the ready event
                client.Log += LogAsync;
                client.ShardReady += ReadyAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // this is where we get the Token value from the configuration file, and start the bot
                await client.LoginAsync(TokenType.Bot, _config["Token"]);
                await client.StartAsync();

                //Set status:
                await client.SetActivityAsync(new Game("for Instagram links", ActivityType.Watching));

                // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
                await services.GetRequiredService<CommandHandler>().InitializeAsync();
               
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
            await CreateSlashCommands(shard);
            return Task.CompletedTask;
        }
        private async Task CreateSlashCommands(DiscordSocketClient shard)
        {
            //List of commands to register:
            List<SlashCommandProperties> slashCommands = new List<SlashCommandProperties>();

            //carousel command:
            {
                SlashCommandBuilder Command = new SlashCommandBuilder();
                // Create carousel command:
                Command.WithName("carousel");
                Command.WithDescription("Select a specific image/video in a multi-content Instagram post.");
                //Add URL option:
                Command.AddOption("url", ApplicationCommandOptionType.String, "The URL of the post.", true);
                //Add media number:
                Command.AddOption("postnumber", ApplicationCommandOptionType.Integer, "The post number for the desired post in the carousel", true, false, false, 1);

                slashCommands.Add(Command.Build());
            }
            //help command:
            {
                SlashCommandBuilder Command = new SlashCommandBuilder();
                // Create carousel command:
                Command.WithName("help");
                Command.WithDescription("For help using the bot");

                slashCommands.Add(Command.Build());
            }
            //invite command:
            {
                SlashCommandBuilder Command = new SlashCommandBuilder();
                // Create carousel command:
                Command.WithName("invite");
                Command.WithDescription("Invite the bot to your server!");

                slashCommands.Add(Command.Build());
            }         
            //top.gg command:
            {
                SlashCommandBuilder Command = new SlashCommandBuilder();
                // Create carousel command:
                Command.WithName("topgg");
                Command.WithDescription("Vote and rate the bot on top.gg");

                slashCommands.Add(Command.Build());
            }
            //github command:
            {
                SlashCommandBuilder Command = new SlashCommandBuilder();
                // Create carousel command:
                Command.WithName("github");
                Command.WithDescription("View the bots source code and report issues.");

                slashCommands.Add(Command.Build());
            }

            foreach (SlashCommandProperties command in slashCommands)
            {
#if DEBUG
                //guild slash command for debug
                foreach(SocketGuild guild in _client.Guilds)
                {
                    Console.WriteLine("Creating command for guild " + guild.Name);
                    await guild.CreateApplicationCommandAsync(command);
                }
#else
                //guild slash command for d
                Console.WriteLine("Creating Slash Commands for shard " + shard.ShardId);
                await shard.CreateGlobalApplicationCommandAsync(command);
#endif
            }

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
                .BuildServiceProvider();
        }
        /// <summary>
        /// Login to instagram if unauthenticated:
        /// </summary>
        public static void InstagramLogin()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config          
            var config = _builder.Build();
            //set user session
            var userSession = new UserSessionData
            {
                UserName = config["IGUserName"],
                Password = config["IGPassword"]
            };
            const string stateFile = "state.bin";
            try
            {
                // load session file if exists
                if (File.Exists(stateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(stateFile))
                    {
                        instaApi.LoadStateDataFromStream(fs);
                        // in .net core or uwp apps don't use LoadStateDataFromStream
                        // use this one:
                        // _instaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                        // you should pass json string as parameter to this function.
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (!instaApi.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                var logInResult = instaApi.LoginAsync().GetAwaiter().GetResult();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return;
                }
            }
        }
    }
}
