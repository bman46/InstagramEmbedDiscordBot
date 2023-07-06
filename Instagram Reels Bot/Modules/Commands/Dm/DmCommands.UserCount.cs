using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using System;

namespace Instagram_Reels_Bot.Modules.Commands.Dm;
public partial class DmCommands {
    private async Task CommandUserCount(SocketUserMessage message) {
        long users = _client.Guilds.Sum(guild => guild.MemberCount);

        await message.ReplyAsync("Users: " + users);
    }
}
