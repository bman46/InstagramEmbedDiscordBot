using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private async Task CommandSync(SocketUserMessage message) {
        if (_commandHandler.Subscriptions.CurrentlyCheckingAccounts()) {
            await message.ReplyAsync("Already doing that.");
        } else {
            // Run this async to avoid blocking the current thread:
            // Use discard since im not interested in the output, only the process.
            _ = _commandHandler.Subscriptions.GetLatestsPosts();
            //Let the user know its being worked on:
            await message.ReplyAsync("Working on it.");
        }
    }
}
