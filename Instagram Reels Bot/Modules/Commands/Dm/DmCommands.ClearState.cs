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
        if (Directory.Exists(stateFile)) {
            // Delete all files in the directory
            foreach (string file in Directory.GetFiles(stateFile)) {
                File.Delete(file);
            }

            // Delete all subdirectories and their contents
            foreach (string dir in Directory.GetDirectories(stateFile)) {
                Directory.Delete(dir, true);
            }
        } else {
            await message.ReplyAsync("Folder not found. Skipping folder removal.");
        }
        // Clear loaded accounts:
        InstagramProcessor.AccountFinder.Accounts = new List<IGAccount>();
        InstagramProcessor.AccountFinder.LoadAccounts();

        await message.ReplyAsync("State files removed.");
    }

}
