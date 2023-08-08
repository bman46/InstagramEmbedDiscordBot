using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private async Task CommandGetGuilds(SocketUserMessage message) {

        var serverListBuilder = new StringBuilder();
        serverListBuilder.AppendLine(Format.Bold("Servers:"));
        //TODO: Export to CSV file

        foreach (SocketGuild guild in _client.Guilds) {
            string serverLine = $"\n{guild.Name}    Boost: {guild.PremiumTier}    Users: {guild.MemberCount}    Locale: {guild.PreferredLocale}";
            //Discord max message length:
            if (serverListBuilder.Length + serverLine.Length > 2000) {
                break;
            }
            serverListBuilder.AppendLine(serverLine);
        }


        await message.ReplyAsync(serverListBuilder.ToString());
    }

}
