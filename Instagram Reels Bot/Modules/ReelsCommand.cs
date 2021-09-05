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
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;
using InstagramApiSharp.API.Builder;

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

            //ensure login:
            Program.InstagramLogin();
            //parse URL:
            var mediaId = await Program.instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));


            //Parse for url:
            var media = await Program.instaApi.MediaProcessor.GetMediaByIdAsync(mediaId.Value);

            //check for private account:
            if (media.Value == null)
            {
                //Add reactions to spell private:
                List<Task> taskList = new List<Task>();
                await Context.Message.AddReactionAsync(new Emoji("🇵"));
                await Context.Message.AddReactionAsync(new Emoji("🇷"));
                await Context.Message.AddReactionAsync(new Emoji("🇮"));
                await Context.Message.AddReactionAsync(new Emoji("🇻"));
                await Context.Message.AddReactionAsync(new Emoji("🇦"));
                await Context.Message.AddReactionAsync(new Emoji("🇹"));
                await Context.Message.AddReactionAsync(new Emoji("🇪"));
                return;
            }

            string videourl = media.Value.Videos[0].Uri;

            if(videourl == "")
            {
                throw new Exception("Couldnt find a video file file.");
            }

            try
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.OpenRead(videourl);
                    if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < 8283750)
                    {
                        using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                        {
                            if (stream.Length < 8283750)
                            {
                                await Context.Channel.SendFileAsync(stream, "reel.mp4", "Video from " + Context.Message.Author.Mention + "'s linked reel:");
                            }
                            else
                            {
                                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked reel: " + videourl);
                            }
                        }
                    }
                    else
                    {
                        await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked reel: " + videourl);
                    }
                }
            }
            catch (Exception e)
            {
                //failback to link to video:
                Console.WriteLine(e);
                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked reel: " + videourl);
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

            //ensure login:
            Program.InstagramLogin();
            //parse URL:
            var mediaId = await Program.instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));

            //Parse for url:
            var media = await Program.instaApi.MediaProcessor.GetMediaByIdAsync(mediaId.Value);

            //check for private account:
            if (media.Value == null)
            {
                //Add reactions to spell private:
                List<Task> taskList = new List<Task>();
                await Context.Message.AddReactionAsync(new Emoji("🇵"));
                await Context.Message.AddReactionAsync(new Emoji("🇷"));
                await Context.Message.AddReactionAsync(new Emoji("🇮"));
                await Context.Message.AddReactionAsync(new Emoji("🇻"));
                await Context.Message.AddReactionAsync(new Emoji("🇦"));
                await Context.Message.AddReactionAsync(new Emoji("🇹"));
                await Context.Message.AddReactionAsync(new Emoji("🇪"));
                return;
            }

            //inject image from carousel:
            if(media.Value.Carousel!=null && media.Value.Carousel.Count > 0)
            {
                if (media.Value.Carousel[0].Videos.Count > 0)
                {
                    var video = media.Value.Carousel[0].Videos[0];
                    media.Value.Videos.Add(video);
                }
                else
                {
                    var image = media.Value.Carousel[0].Images[0];
                    media.Value.Images.Add(image);
                }
            }
            //check video:
            if (media.Value.Videos.Count > 0)
            {
                string videourl = media.Value.Videos[0].Uri;
                try
                {
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        wc.OpenRead(videourl);
                        if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < 8283750)
                        {
                            using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                            {
                                if (stream.Length < 8283750)
                                {
                                    await Context.Channel.SendFileAsync(stream, "IGPost.mp4", "Video from " + Context.Message.Author.Mention + "'s linked Post:");
                                }
                                else
                                {
                                    await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked post: " + videourl);
                                }
                            }
                        }
                        else
                        {
                            await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked post: " + videourl);
                        }
                    }
                }
                catch (Exception e)
                {
                    //failback to link to video:
                    Console.WriteLine(e);
                    await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked post: " + videourl);
                }
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.Title = "Content from " + Context.Message.Author.Username + "'s linked post";
                embed.Url = url;
                embed.Description = (media.Value.Caption!=null)?(media.Value.Caption.Text):("");
                embed.ImageUrl = media.Value.Images[0].Uri;
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

            //ensure login:
            Program.InstagramLogin();
            //parse URL:
            var mediaId = await Program.instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));


            //Parse for url:
            var media = await Program.instaApi.MediaProcessor.GetMediaByIdAsync(mediaId.Value);

            //check for private account:
            if (media.Value == null)
            {
                //Add reactions to spell private:
                List<Task> taskList = new List<Task>();
                await Context.Message.AddReactionAsync(new Emoji("🇵"));
                await Context.Message.AddReactionAsync(new Emoji("🇷"));
                await Context.Message.AddReactionAsync(new Emoji("🇮"));
                await Context.Message.AddReactionAsync(new Emoji("🇻"));
                await Context.Message.AddReactionAsync(new Emoji("🇦"));
                await Context.Message.AddReactionAsync(new Emoji("🇹"));
                await Context.Message.AddReactionAsync(new Emoji("🇪"));
                return;
            }

            string videourl = media.Value.Videos[0].Uri;

            try
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.OpenRead(videourl);
                    if(Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < 8283750)
                    {
                        using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                        {
                            if (stream.Length < 8283750)
                            {
                                await Context.Channel.SendFileAsync(stream, "IGTV.mp4", "Video from " + Context.Message.Author.Mention + "'s linked IGTV Video:");
                            }
                            else
                            {
                                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: " + videourl);
                            }
                        }
                    }
                    else
                    {
                        await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: " + videourl);
                    }
                }
            }
            catch(Exception e)
            {
                //failback to link to video:
                Console.WriteLine(e);
                await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked IGTV Video: " + videourl);
            }
        }
    }
}
