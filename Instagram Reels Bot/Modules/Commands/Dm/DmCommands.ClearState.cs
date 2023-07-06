using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Helpers.Instagram;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private static async Task CommandClearState(SocketUserMessage message) {

        // Clear statefiles:
        string stateFile = Path.Combine(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "StateFiles");
        if (!Directory.Exists(stateFile)) {
            await message.ReplyAsync("Folder not found. Skipping folder removal.");
        } else {
            Directory.Delete(stateFile, true);
            Directory.CreateDirectory(stateFile);
        }         // Clear loaded accounts:
        InstagramProcessor.AccountFinder.Accounts = new List<IGAccount>();
        InstagramProcessor.AccountFinder.LoadAccounts();

        await message.ReplyAsync("State files removed.");
    }

}
