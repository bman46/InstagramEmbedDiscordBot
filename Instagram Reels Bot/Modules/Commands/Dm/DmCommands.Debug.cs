using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using OpenGraphNet;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private async Task CommandDebug(SocketUserMessage message) {
        //Server count:
        await message.ReplyAsync($"Server Count: {_client.Guilds.Count}");
        //Shard count:
        await message.ReplyAsync($"Shards: {_client.Shards.Count}");

        //IP check:
        try {
            OpenGraph graph = await OpenGraph.ParseUrlAsync("https://api.ipify.org/", "");
            await message.ReplyAsync($"IP: {graph.OriginalHtml}");
        } catch (Exception e) {
            await message.ReplyAsync("Could not connect to server. Error: " + e);
        }
    }
}
