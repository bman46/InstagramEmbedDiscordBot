using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using OpenGraphNet;

namespace Instagram_Reels_Bot.Modules
{
    public class ReelsCommand : ModuleBase
    {
        [Command("reel")]
        public async Task ReelParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/reel/" + args.Replace(" ", "/");

            //Parse for Opengraph url:
            OpenGraph graph = OpenGraph.ParseUrl(url, "");
            string videourl = graph.Metadata["og:video"].First().Value;

            //return result:
            await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s reel: " + videourl);
        }
    }
}
