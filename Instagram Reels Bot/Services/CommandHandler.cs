using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using OpenGraphNet;
using System.Linq;

namespace Instagram_Reels_Bot.Services
{
    public class CommandHandler
    {
        // setup fields to be set later in the constructor
        private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        /// <summary>
        /// Notifies the owner of an error
        /// false by default. Toggled by user DM command.
        /// Reverts to false when bot is restarted.
        /// </summary>
        public static bool notifyOwnerOnError = false;

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
            //DMs:
            if (message.Channel.GetType() == typeof(SocketDMChannel))
            {
                if (message.Content.ToLower().StartsWith("debug"))
                {
                    if (!string.IsNullOrEmpty(_config["OwnerID"]) && message.Author.Id == ulong.Parse(_config["OwnerID"]))
                    {
                        //Server count:
                        await message.ReplyAsync("Server Count: " + _client.Guilds.Count);

                        //IP check:
                        try
                        {
                            OpenGraph graph = OpenGraph.ParseUrl("https://api.ipify.org/", "");
                            await message.ReplyAsync("IP: " + graph.OriginalHtml);
                        }
                        catch (Exception e)
                        {
                            await message.ReplyAsync("Could not connect to server. Error: "+e);
                        }
                    }
                }
                else if (message.Content.ToLower().StartsWith("guilds"))
                {
                    //Guild list
                    if (!string.IsNullOrEmpty(_config["OwnerID"]) && message.Author.Id == ulong.Parse(_config["OwnerID"]))
                    {
                        string serverList = Format.Bold("Servers:");
                        foreach(SocketGuild guild in _client.Guilds)
                        {
                            String serverLine = "\n" + guild.Name + " \tBoost: " + guild.PremiumTier + " \tUsers: " + guild.MemberCount + " \tLocale: " + guild.PreferredLocale;
                            //Discord max message length:
                            if (serverList.Length + serverLine.Length > 2000)
                            {
                                await message.ReplyAsync(serverList);
                                serverList = "";
                            }
                            serverList += serverLine;
                        }
                        await message.ReplyAsync(serverList);
                    }
                }
                else if (message.Content.ToLower().StartsWith("toggle error"))
                {
                    //toggle error DMs
                    if (!string.IsNullOrEmpty(_config["OwnerID"]) && message.Author.Id == ulong.Parse(_config["OwnerID"]))
                    {
                        notifyOwnerOnError = !notifyOwnerOnError;
                        if (notifyOwnerOnError)
                        {
                            await message.ReplyAsync("Error notifications enabled.");
                        }
                        else
                        {
                            await message.ReplyAsync("Error notifications disabled.");
                        }
                    }
                }else if (message.Content.ToLower().StartsWith("users"))
                {
                    if (!string.IsNullOrEmpty(_config["OwnerID"]) && message.Author.Id == ulong.Parse(_config["OwnerID"]))
                    {
                        int users = 0;
                        foreach(SocketGuild guild in _client.Guilds)
                        {
                            users += guild.MemberCount;
                        }
                        await message.ReplyAsync("Users: "+users);
                    }
                }
                return;
            }

            // sets the argument position away from the prefix we set
            int argPos = 0;
            int endUrlLength = 0;
            bool foundPrefix = false;

            // get each prefix from the configuration file
            foreach (string prefix in _config.GetSection("Prefix").GetChildren().ToArray().Select(c => c.Value).ToArray())
            {
                //check for valid prefix:
                if (message.Content.Contains(prefix))
                {
                    argPos = message.Content.IndexOf(prefix) + prefix.Length;
                    endUrlLength = message.Content.Substring(argPos).Replace("\n"," ").IndexOf(" ");
                    foundPrefix = true;
                    break;
                }
            }
            if (!foundPrefix)
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

            

            //Split url down to params:
            String[] userInput = commandText.Split(" ");

            foreach(CommandInfo command in _commands.Commands)
            {
                if (command.Name.Equals(userInput[0]))
                {
                    await _commands.ExecuteAsync(context, commandText, _services);
                }
                else if (command.Name.Equals(userInput[1]))
                {
                    commandText=commandText.Replace(userInput[0]+" ", "");
                    Console.WriteLine(commandText);
                    await _commands.ExecuteAsync(context, commandText, _services);
                }
            }
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
            if(result.ErrorReason.Contains("Missing Permissions"))
            {
                await context.Channel.SendMessageAsync($"I do not have permission to carry out this action. I need permission to Send Messages, Embed Links, Attach Files, Add Reactions, and Manage Messages in this channel.");
                return;
            }


            // failure scenario, let's let the user know
            await context.Channel.SendMessageAsync($"Sorry, Something went wrong... Discord support server: https://top.gg/servers/921830686439124993");

            //notify owner if desired:
            if (notifyOwnerOnError&&!string.IsNullOrEmpty(_config["OwnerID"]))
            {
                string error = Format.Bold("Error:") + "\n" + result.Error + "\n" + Format.Code(result.ErrorReason)+"\n\n"+ Format.Bold("Command:") + "\n" + Format.BlockQuote(context.Message.Content);
                if (error.Length > 2000)
                {
                    error = error.Substring(0, 2000);
                }
                await Discord.UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
            }

        }
    }
}
