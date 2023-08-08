using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {

    private static async Task CommandGenerateDevice(SocketUserMessage message) {
        AndroidDevice device = AndroidDeviceGenerator.GetRandomAndroidDevice();
        string jsonString = $"```json\n{JsonSerializer.Serialize(device)}\n```";
        await message.ReplyAsync($"Device:\n{jsonString}");
    }
}
