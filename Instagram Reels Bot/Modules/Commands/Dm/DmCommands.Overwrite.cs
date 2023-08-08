using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private async Task CommandOverwrite(SocketUserMessage message) {

        // Load all registered commands:
        var commands = await _client.Rest.GetGlobalApplicationCommands();
        // Delete all commands:
        foreach (var command in commands) {
            await command.DeleteAsync();
        }
        // Re-register commands:
        await _commandHandler.Interact.RegisterCommandsGloballyAsync(true);
        // Alert user:
        await message.ReplyAsync("Slash commands resynced.");
    }
}
