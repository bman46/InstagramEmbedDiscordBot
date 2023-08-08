using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers.Extensions;
using Instagram_Reels_Bot.Services;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private Task CommandToggle(SocketUserMessage message, string[] commandParts) {
        if (commandParts.Length < 2) return Task.CompletedTask;

        Task task = commandParts[1] switch {
            "error" => CommandToggleError(message),
            _ => Task.CompletedTask
        };

        return task;
    }

    private async Task CommandToggleError(SocketUserMessage message) {
        CommandHandler.NotifyOwnerOnError = !CommandHandler.NotifyOwnerOnError;

        string statusName = CommandHandler.NotifyOwnerOnError.ToString("enabled", "disabled");
        await message.ReplyAsync($"Error notifications {statusName}.");
    }

}
