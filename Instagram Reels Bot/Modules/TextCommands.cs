using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Instagram_Reels_Bot.Helpers;

namespace Instagram_Reels_Bot.Modules
{
    public class TextCommands : ModuleBase
    {
        /// <summary>
        /// Parse reel URL:
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("reel")]
        public async Task ReelParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/reel/" + args.Replace(" ", "/");
            await Responder(url);
        }
        /// <summary>
        /// Parse an instagram post:
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("p")]
        public async Task PostParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/p/" + args.Replace(" ", "/");
            await Responder(url);
        }
        /// <summary>
        /// Parse an instagram TV link:
        /// https://www.instagram.com/tv/
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("tv")]
        public async Task TVParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/tv/" + args.Replace(" ", "/");
            await Responder(url);
        }
        /// <summary>
        /// Parse Story Link
        /// Ex: https://instagram.com/stories/wevolverapp/2718330735469161935?utm_source=ig_story_item_share&utm_medium=copy_link
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("stories")]
        public async Task StoryParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/stories/" + args.Replace(" ", "/");
            await Responder(url);
        }

        private async Task Responder(string url)
        {
            //Process Post:
            InstagramProcessorResponse response = await InstagramProcessor.PostRouter(url, (int)Context.Guild.PremiumTier, 1);

            if (!response.success)
            {
                await ReplyAsync(response.error);
                return;
            }
            if (response.isVideo)
            {
                if (response.stream != null)
                {
                    //Response with stream:
                    await Context.Channel.SendFileAsync(new MemoryStream(response.stream), "IGMedia.mp4", "Video from " + Context.Message.Author.Mention + "'s Instagram link:");
                    return;
                }
                else
                {
                    //Response without stream:
                    await ReplyAsync("Video from " + Context.User.Mention + "'s linked reel: " + response.contentURL);
                    return;
                }

            }
            else
            {
                var embed = new EmbedBuilder();
                embed.Title = "Content from " + Context.User.Username + "'s linked post";
                embed.Url = url;
                embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption, 40)) : ("");
                embed.ImageUrl = "attachment://IGMedia.jpg";
                embed.WithColor(new Color(131, 58, 180));
                if (response.stream != null)
                {
                    await Context.Channel.SendFileAsync(new MemoryStream(response.stream), "IGMedia.jpg", "", false, embed.Build());
                }
                else
                {
                    embed.ImageUrl = response.contentURL.ToString();
                    await ReplyAsync(null, false, embed.Build());
                }
            }
        }
    }
}
