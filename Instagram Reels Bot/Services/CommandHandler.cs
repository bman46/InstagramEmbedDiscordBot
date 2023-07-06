using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Helpers.Extensions;
using Instagram_Reels_Bot.Helpers.Instagram;
using Instagram_Reels_Bot.Modules.Commands.Dm;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenGraphNet;
using System;

namespace Instagram_Reels_Bot.Services
{
    public class CommandHandler
    {
        // setup fields to be set later in the constructor
        private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly InteractionService _interact;
        public readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private readonly Subscriptions _subscriptions;
        /// <summary>
        /// Notifies the owner of an error
        /// false by default. Toggled by user DM command or config setting.
        /// </summary>
        public static bool NotifyOwnerOnError { get; set; }

        public Subscriptions Subscriptions => _subscriptions;
        public InteractionService Interact => _interact;

        public CommandHandler(IServiceProvider services, Services.Subscriptions subs)
        {
            // juice up the fields with these services
            // since we passed the services in, we can use GetRequiredService to pass them into the fields set earlier
            _config = services.GetRequiredService<IConfiguration>();
            _commands = services.GetRequiredService<CommandService>();
            _interact = services.GetRequiredService<InteractionService>();
            _client = services.GetRequiredService<DiscordShardedClient>();
            _services = services;
            _subscriptions = subs;

            // take action when we execute a command
            _commands.CommandExecuted += CommandExecutedAsync;

            // take action when we receive a message (so we can process it, and see if it is a valid command)
            _client.MessageReceived += MessageReceivedAsync;

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            //Slash Commands:
            _interact.SlashCommandExecuted += SlashCommandExecuted;
            _interact.ContextCommandExecuted += ContextCommandExecuted;
            _interact.ComponentCommandExecuted += ComponentCommandExecuted;

            //set DM errors flag if possible:
            try
            {
                NotifyOwnerOnError = _config.Parse<bool>("DMErrors");
            }
            catch
            {
                //Default to false
                NotifyOwnerOnError = false;
            }
        }

        public async Task InitializeAsync()
        {
            // register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interact.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // this class is where the magic starts, and takes actions upon receiving messages
        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message)
            {
                return;
            }

            // ensures we don't process system/other bot messages AND add exception to this rule if desired:
            if (message.Source != MessageSource.User && _config.Is("AllowBotMessages", true, true))
            {
                return;
            }

            //DMs:
            if (message.Channel is SocketDMChannel)
            {
                var dmCommands = new DmCommands(this, _client, _config);
                await dmCommands.ManageMessageAsync(message);
                return;
            }

            // sets the argument position away from the prefix we set
            int argPos = 0;
            int endUrlLength = 0;
            bool foundPrefix = false;

            // get each prefix from the configuration file
            foreach (string prefix in _config.GetSection("Prefix").GetChildren().Select(c => c.Value))
            {
                //check for valid prefix:
                if (message.Content.StartsWith(prefix))
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
            //Check for profile link:
            if (InstagramProcessor.isProfileLink(new Uri("https://instagram.com/"+commandText.Replace(" ", "/"))))
            {
                //Little hack to add command to url (profile is also reserved by ig so no conflicts):
                commandText = "profile " + commandText;
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
        /// <summary>
        /// Handles text command success and failures.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
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
            else if(result.ErrorReason.Contains("Missing Permissions"))
            {
                await context.Channel.SendMessageAsync($"I do not have permission to carry out this action. I need permission to Send Messages, Embed Links, Attach Files, Add Reactions, Read Message History, and Manage Messages in this channel.");
                return;
            }else if(result.ErrorReason.Contains("Cannot reply without permission to read message history"))
            {
                await context.Channel.SendMessageAsync("I need permission to read message history in order to reply to a message and carry out the command.");
                return;
            }
            else if (result.ErrorReason.Contains("timed out"))
            {
                await context.Channel.SendMessageAsync($"Your command timed out. This may be because the bot is overloaded. Please try again later.");
                //report to owner anyway.
            }
            else
            {
                // failure scenario, let's let the user know
                await context.Channel.SendMessageAsync($"Sorry, Something went wrong...");
            }

            //notify owner if desired:
            if (NotifyOwnerOnError&&!string.IsNullOrEmpty(_config["OwnerID"]))
            {
                string error = Format.Bold("Error:") + "\n" + result.Error + "\n" + Format.Code(result.ErrorReason)+"\n\n"+ Format.Bold("Command:") + "\n" + Format.BlockQuote(context.Message.ToString());

                if (error.Length > 2000)
                {
                    error = error.Substring(0, 2000);
                }
                await Discord.UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
            }

        }
        /// <summary>
        /// Handles success and failures from slash commands.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                // Defer if not already done:
                try
                {
                    arg2.Interaction.DeferAsync(true).GetAwaiter().GetResult();
                }
                catch
                {
                    // ignore
                }
                // Write error to console:
                Console.WriteLine("Error: " + arg3.Error);
                
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // Check for userperm error:
                        if (arg3.ErrorReason.Contains("UserPerm"))
                        {
                            arg2.Interaction.FollowupAsync("You do not have permission to execute this command.", ephemeral: true);
                            break;
                        }
                        arg2.Interaction.FollowupAsync("Command Failed\n"+arg3.ErrorReason, ephemeral: true);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        arg2.Interaction.FollowupAsync("Unknown command. It may have been recently removed or changed.", ephemeral: true);
                        break;
                    case InteractionCommandError.BadArgs:
                        arg2.Interaction.FollowupAsync("The provided values are invalid. (BadArgs)", ephemeral: true);
                        break;
                    case InteractionCommandError.Exception:
                        //notify owner if desired:
                        if(arg3.ErrorReason.Contains("Invalid Form Body"))
                        {
                            arg2.Interaction.FollowupAsync("Invalid form body. Please check to ensure that all of your parameters are correct.", ephemeral: true);
                            break;
                        }
                        if (NotifyOwnerOnError && !string.IsNullOrEmpty(_config["OwnerID"]))
                        {
                            string error = Format.Bold("Error:") + "\n" + Format.Code(arg3.ErrorReason) + "\n\n" + Format.Bold("Command:") + "\n" + Format.BlockQuote(arg1.Name + " " + DiscordTools.SlashParamToString(arg2));
                            if (error.Length > 2000)
                            {
                                error = error.Substring(0, 2000);
                            }
                            UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
                        }
                        arg2.Interaction.FollowupAsync("Sorry, Something went wrong...", ephemeral: true);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        //notify owner if desired:
                        if (NotifyOwnerOnError && !string.IsNullOrEmpty(_config["OwnerID"]))
                        {
                            string error = Format.Bold("Error:") + "\n" + Format.Code(arg3.ErrorReason) + "\n\n" + Format.Bold("Command:") + "\n" + Format.BlockQuote(arg1.Name + " " + DiscordTools.SlashParamToString(arg2));
                            if (error.Length > 2000)
                            {
                                error = error.Substring(0, 2000);
                            }
                            UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
                        }
                        arg2.Interaction.FollowupAsync("Sorry, Something went wrong...", ephemeral: true);
                        break;
                    default:
                        //notify owner if desired:
                        if (NotifyOwnerOnError && !string.IsNullOrEmpty(_config["OwnerID"]))
                        {
                            string error = Format.Bold("Error:") + "\n" + Format.Code(arg3.ErrorReason) + "\n\n" + Format.Bold("Command:") + "\n" + Format.BlockQuote(arg1.Name + " " + DiscordTools.SlashParamToString(arg2));
                            if (error.Length > 2000)
                            {
                                error = error.Substring(0, 2000);
                            }
                            UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
                        }
                        arg2.Interaction.FollowupAsync("Sorry, Something went wrong...");
                        break;
                }
            }

            return Task.CompletedTask;
        }
        /// <summary>
        /// Handle an executed component
        /// Used for buttons, drop downs, etc.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                // Defer if not already done:
                try
                {
                    arg2.Interaction.DeferAsync(true).GetAwaiter().GetResult();
                }
                catch
                {
                    // ignore
                }

                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // Check for userperm error:
                        if (arg3.ErrorReason.Contains("UserPerm"))
                        {
                            arg2.Interaction.FollowupAsync("You do not have permission to execute this command.", ephemeral: true);
                            break;
                        }

                        arg2.Interaction.FollowupAsync("Action Failed\n" + arg3.ErrorReason, ephemeral: true);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        arg2.Interaction.FollowupAsync("Unknown action. It may have been recently removed or changed.", ephemeral: true);
                        break;
                    case InteractionCommandError.BadArgs:
                        arg2.Interaction.FollowupAsync("The provided values are invalid. (BadArgs)", ephemeral: true);
                        break;
                    case InteractionCommandError.Exception:
                        //notify owner if desired:
                        if (arg3.ErrorReason.Contains("Invalid Form Body"))
                        {
                            arg2.Interaction.FollowupAsync("Invalid form body. Please check to ensure that all of your parameters are correct.", ephemeral: true);
                            break;
                        }
                        if (NotifyOwnerOnError && !string.IsNullOrEmpty(_config["OwnerID"]))
                        {
                            string error = Format.Bold("Error:") + "\n" + Format.Code(arg3.ErrorReason) + "\n\n" + Format.Bold("Command:") + "\n" + Format.BlockQuote(arg1.Name + " " + DiscordTools.SlashParamToString(arg2));
                            if (error.Length > 2000)
                            {
                                error = error.Substring(0, 2000);
                            }
                            UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
                        }
                        arg2.Interaction.FollowupAsync("Sorry, Something went wrong...", ephemeral: true);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        //notify owner if desired:
                        if (NotifyOwnerOnError && !string.IsNullOrEmpty(_config["OwnerID"]))
                        {
                            string error = Format.Bold("Error:") + "\n" + Format.Code(arg3.ErrorReason) + "\n\n" + Format.Bold("Command:") + "\n" + Format.BlockQuote(arg1.Name + " " + DiscordTools.SlashParamToString(arg2));
                            if (error.Length > 2000)
                            {
                                error = error.Substring(0, 2000);
                            }
                            UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
                        }
                        arg2.Interaction.FollowupAsync("Sorry, Something went wrong...", ephemeral: true);
                        break;
                    default:
                        //notify owner if desired:
                        if (NotifyOwnerOnError && !string.IsNullOrEmpty(_config["OwnerID"]))
                        {
                            string error = Format.Bold("Error:") + "\n" + Format.Code(arg3.ErrorReason) + "\n\n" + Format.Bold("Command:") + "\n" + Format.BlockQuote(arg1.Name + " " + DiscordTools.SlashParamToString(arg2));
                            if (error.Length > 2000)
                            {
                                error = error.Substring(0, 2000);
                            }
                            UserExtensions.SendMessageAsync(_client.GetUser(ulong.Parse(_config["OwnerID"])), error);
                        }
                        arg2.Interaction.FollowupAsync("Sorry, Something went wrong...");
                        break;
                }
            }

            return Task.CompletedTask;
        }
        /// <summary>
        /// Not currently used
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }
        private async Task HandleInteraction(SocketInteraction arg)
        {
            Console.WriteLine("Slash Command Executed");
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new ShardedInteractionContext(_client, arg);
                await _interact.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}