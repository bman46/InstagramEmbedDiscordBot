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
using System.IO;

namespace Instagram_Reels_Bot.Modules
{
    public class ReelsCommand : ModuleBase
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

            //Parse for Opengraph url:
            OpenGraph graph = OpenGraph.ParseUrl(url, "");
            string videourl = graph.Metadata["og:video"].First().Value;

            try
            {
                //Send uploaded response:
                using (var client = new System.Net.Http.HttpClient())
                {

                    var response = await client.GetByteArrayAsync(videourl);
                    using (var stream = new MemoryStream(response))
                    {
                        //upload if less than 8MB
                        if (stream.Length < 8283750)
                        {
                            //await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: ");
                            await Context.Channel.SendFileAsync(stream, "reel.mp4", "Video from " + Context.Message.Author.Mention + "'s linked reel:");
                        }
                        else
                        {
                            stream.Dispose();
                            await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked Reel: " + videourl);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //failback to link to video (video possibly larger than discord allows)
                Console.WriteLine(e);
                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: " + videourl);
            }
        }
        /// <summary>
        /// Parse an instagram post:
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("p")]
        public async Task PostParser([Remainder] string args = null)
        {
            string url = "https://www.instagram.com/p/" + args.Replace(" ", "/");
            //Parse for Opengraph Video url:
            OpenGraph graph = OpenGraph.ParseUrl(url, "");
            if (graph.Metadata["og:video"].Count > 0)
            {
                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked post: " + graph.Metadata["og:video"].First().Value);
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.Title = "Content from " + Context.Message.Author.Username + "'s linked post";
                embed.Url = graph.Metadata["og:url"].First().Value;
                embed.Description = graph.Metadata["og:title"].First().Value;
                embed.ImageUrl = graph.Metadata["og:image"].First().Value;
                embed.WithColor(new Color(131, 58, 180));
                await ReplyAsync(null, false, embed.Build());
            }
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

            //Parse for Opengraph url:
            OpenGraph graph = OpenGraph.ParseUrl(url, "");
            string videourl = graph.Metadata["og:video"].First().Value;

            try
            {
                //Send uploaded response:
                using (var client = new System.Net.Http.HttpClient())
                {
                  
                    var response = await client.GetByteArrayAsync(videourl);
                    using (var stream = new MemoryStream(response))
                    {
                        //upload if less than 8MB
                        if (stream.Length < 8283750)
                        {
                            //await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: ");
                            await Context.Channel.SendFileAsync(stream, "IGTV.mp4", "Video from " + Context.Message.Author.Mention + "'s linked IGTV Video:");
                        }
                        else
                        {
                            stream.Dispose();
                            await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: " + videourl);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                //failback to link to video (video possibly larger than discord allows)
                Console.WriteLine(e);
                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: " + videourl);
            }
        }
    }
}
