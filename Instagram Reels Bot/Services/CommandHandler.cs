using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using OpenGraphNet;

namespace Instagram_Reels_Bot.Services
{
    public class CommandHandler
    {
        // setup fields to be set later in the constructor
        private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            // juice up the fields with these services
            // since we passed the services in, we can use GetRequiredService to pass them into the fields set earlier
            _config = services.GetRequiredService<IConfiguration>();
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            // take action when we execute a command
            _commands.CommandExecuted += CommandExecutedAsync;

            // take action when we receive a message (so we can process it, and see if it is a valid command)
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // this class is where the magic starts, and takes actions upon receiving messages
        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // ensures we don't process system/other bot messages
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }
            if (message.Channel.GetType() == typeof(SocketDMChannel))
            {
                if (message.Content.ToLower().StartsWith("debug"))
                {
                    await message.ReplyAsync("Server Count: " + _client.Guilds.Count);

                    //IP Check:
                    if (!string.IsNullOrEmpty(_config["OwnerID"]) && message.Author.Id == ulong.Parse(_config["OwnerID"]))
                    {
                        try
                        {
                            OpenGraph graph = OpenGraph.ParseUrl("https://api.ipify.org/", "");
                            await message.ReplyAsync("IP: " + graph.OriginalHtml);
                        }
                        catch (Exception e)
                        {
                            await message.ReplyAsync("Proxy may have failed. Error: "+e);
                        }
                    }
                }
                else if (message.Content.ToLower().StartsWith("guilds"))
                {
                    //Guild list
                    if (!string.IsNullOrEmpty(_config["OwnerID"]) && message.Author.Id == ulong.Parse(_config["OwnerID"]))
                    {
                        string serverList = "";
                        foreach(SocketGuild guild in _client.Guilds)
                        {
                            serverList += "\n" + guild.Name;
                        }
                        await message.ReplyAsync("Servers:" + serverList);
                    }
                }
                return;
            }

            // sets the argument position away from the prefix we set
            int argPos = 0;
            int endUrlLength = 0;

            // get prefix from the configuration file
            string prefix = _config["Prefix"];

            // determine if the message has a valid prefix, and adjust argPos based on prefix
            if (message.Content.Contains(prefix))
            {
                argPos = message.Content.IndexOf(prefix) + prefix.Length;
                endUrlLength = message.Content.Substring(argPos).IndexOf(" ");
            }
            else
            {
                return;
            }
            
            var context = new ShardedCommandContext(_client, message);

            //create new string from command
            string commandText;
            if (endUrlLength <= 0)
            {
                commandText = message.Content.Substring(argPos).Replace("/", " ");
            }
            else
            {
                commandText = message.Content.Substring(argPos, endUrlLength).Replace("/", " ");
            }

            //Console.WriteLine("New command " + commandText);

            // execute command if one is found that matches
            await _commands.ExecuteAsync(context, commandText, _services);
        }
       
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // if a command isn't found, log that info to console and exit this method
            if (!command.IsSpecified)
            {
                System.Console.WriteLine($"Command not found");
                return;
            }


            // log success to the console and exit this method
            if (result.IsSuccess)
            {
                System.Console.WriteLine($"Command Executed.");
                return;
            }


            // failure scenario, let's let the user know
            await context.Channel.SendMessageAsync($"Sorry, Something went wrong...");
        }
    }
}
