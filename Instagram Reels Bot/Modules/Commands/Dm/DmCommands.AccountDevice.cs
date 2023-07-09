using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Helpers.Instagram;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private static async Task CommandAccountDevice(SocketUserMessage message) {
        foreach (IGAccount user in InstagramProcessor.AccountFinder.Accounts) {
            AndroidDevice device = user.StaticDevice;
            string jsonString = "```json\n" + JsonSerializer.Serialize(device) + "\n```";
            await message.ReplyAsync(user.UserName + " Device:\n" + jsonString);
        }
    }
}
