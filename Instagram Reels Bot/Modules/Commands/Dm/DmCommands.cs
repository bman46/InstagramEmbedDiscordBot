using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Helpers.Extensions;
using Instagram_Reels_Bot.Helpers.Instagram;
using Instagram_Reels_Bot.Services;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using Microsoft.Extensions.Configuration;
using OpenGraphNet;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private readonly DiscordShardedClient _client;
    private readonly IConfiguration _config;
    private readonly CommandHandler _commandHandler;

    public DmCommands(CommandHandler commandHandler, DiscordShardedClient client, IConfiguration config) {
        _client = client;
        _config = config;
        _commandHandler = commandHandler;
    }

    public async Task ManageMessageAsync(SocketUserMessage message) {
        string[] commandparts = message.Content.Split(' ');
        string commandName = commandparts[0].ToLower();


        // Owner only commands
        if (!message.Author.IsBotOwner(_config)) {
            return;
        }

        Task task = commandName switch {
            "debug" => CommandDebug(message),
            "guilds" => CommandGetGuilds(message),
            "toggle" => CommandToggle(message, commandparts),
            "users" => CommandUserCount(message),
            "accounts" => CommandAccounts(message),
            "sync" => CommandSync(message),
            "overwrite" => CommandOverwrite(message),
            "clearstate" => CommandClearState(message),
            "generatedevice" => CommandGenerateDevice(message),
            "accountdevice" => CommandAccountDevice(message),
            _ => Task.CompletedTask
        };

        await task;
    }
}
