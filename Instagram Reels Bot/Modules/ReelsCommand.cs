using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Instagram_Reels_Bot.Modules
{
    public class ReelsCommand : ModuleBase
    {
        /// <summary>
        /// Sets the message size for embeds.
        /// </summary>
        private static readonly int MAX_MSG_SIZE = 40;
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
                    if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < MaxUploadSize(Context))
                    {
                        using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                        {
                            if (stream.Length < MaxUploadSize(Context))
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
                        if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < MaxUploadSize(Context))
                        {
                            using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                            {
                                if (stream.Length < MaxUploadSize(Context))
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
                embed.Description = (media.Value.Caption!=null)?(Truncate(media.Value.Caption.Text, MAX_MSG_SIZE)):("");
                embed.ImageUrl = media.Value.Images[0].Uri;
                embed.WithColor(new Color(131, 58, 180));
                await ReplyAsync(null, false, embed.Build());
            }
            //Remove discords automatic embed (If one exists)
            try
            {
                await Context.Message.ModifyAsync((MessageProperties obj) => { obj.Embed = null; });
            }
            catch
            {
                //no permission to do this.
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
                    if(Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < MaxUploadSize(Context))
                    {
                        using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                        {
                            if (stream.Length < MaxUploadSize(Context))
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
        /// <summary>
        /// Parse Story Link
        /// Ex: https://instagram.com/stories/wevolverapp/2718330735469161935?utm_source=ig_story_item_share&utm_medium=copy_link
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("stories")]
        public async Task StoryParser([Remainder] string args = null)
        {
            //ensure login:
            Program.InstagramLogin();

            string userName;
            string storyID;
            try
            {
                userName = args.Split(' ')[0];
                storyID = args.Split(' ')[1].Substring(0, (args.Split(' ')[1].IndexOf("?")>0)?(args.Split(' ')[1].IndexOf("?")) :(args.Split(' ')[1].Length));
            }
            catch(Exception e)
            {
                await ReplyAsync("Invalid story link.");
                Console.WriteLine("Error Parsing: " + e);
                return;
            }
            var user = await Program.instaApi.UserProcessor.GetUserAsync(userName);
            long userId = user.Value.Pk;
            var stories = await Program.instaApi.StoryProcessor.GetUserStoryAsync(userId);
            if (stories.Value.Items.Count == 0)
            {
                await ReplyAsync("No stories exist. (Is the account private?)");
                Console.WriteLine("No stories.");
                return;
            }
            foreach(var story in stories.Value.Items)
            {
                //find story:
                if (story.Id.Contains(storyID))
                {
                   
                    if (story.VideoList.Count > 0)
                    {
                        //process video:
                        string videourl = story.VideoList[0].Uri;
                        try
                        {
                            using (System.Net.WebClient wc = new System.Net.WebClient())
                            {
                                wc.OpenRead(videourl);
                                if (Convert.ToInt64(wc.ResponseHeaders["Content-Length"]) < MaxUploadSize(Context))
                                {
                                    using (var stream = new MemoryStream(wc.DownloadData(videourl)))
                                    {
                                        if (stream.Length < MaxUploadSize(Context))
                                        {
                                            await Context.Channel.SendFileAsync(stream, "Story.mp4", "Video from " + Context.Message.Author.Mention + "'s linked story:");
                                        }
                                        else
                                        {
                                            await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked story: " + videourl);
                                        }
                                    }
                                }
                                else
                                {
                                    await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked story: " + videourl);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //failback to link to video:
                            Console.WriteLine(e);
                            await ReplyAsync("Video from " + Context.Message.Author.Mention + "'s linked story: " + videourl);
                        }

                        //Remove discords automatic embed (If one exists)
                        try
                        {
                            await Context.Message.ModifyAsync((MessageProperties obj) => { obj.Embed = null; });
                        }
                        catch
                        {
                            //no permission to do this.
                        }

                        return;
                    }
                    else if (story.ImageList.Count > 0)
                    {
                        var embed = new EmbedBuilder();
                        embed.Title = "Content from " + Context.Message.Author.Username + "'s linked story";
                        embed.ImageUrl = story.ImageList[0].Uri;
                        embed.WithColor(new Color(131, 58, 180));
                        await ReplyAsync(null, false, embed.Build());

                        //Remove discords automatic embed (If one exists)
                        try
                        {
                            await Context.Message.ModifyAsync((MessageProperties obj) => { obj.Embed = null; });
                        }
                        catch
                        {
                            //no permission to do this.
                        }
                        
                        return;
                    }
                    
                }
            }
            await ReplyAsync("Could not find the story.");
        }
        /// <summary>
        /// Calculates the max upload size of a given server.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Upload size in bytes</returns>
        private static int MaxUploadSize(ICommandContext context)
        {
            switch (context.Guild.PremiumTier)
            {
                case PremiumTier.Tier2:
                    //Tier 2 50MB Upload Limit
                    return 50000000;
                case PremiumTier.Tier3:
                    //Tier 3 100MB Upload Limit
                    return 100000000;
                default:
                    //Default 8MB Upload Limit
                    return 8000000;
            }
        }
        /// <summary>
        /// Cuts the size of the string down.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <param name="atWord"></param>
        /// <param name="addEllipsis"></param>
        /// <returns></returns>
        public static string Truncate(string s, int length, bool atWord = true, bool addEllipsis = true)
        {
            // Return if the string is less than or equal to the truncation length
            if (s == null || s.Length <= length) {
                return s;
            }
            //cut description at new line
            else if (s.Contains("\n"))
            {
                //cut string at newline:
                s = s.Substring(0, s.IndexOf("\n"))+"...";
                //recheck size after cut:
                if (s == null || s.Length <= length)
                {
                    return s;
                }
            }
            // Do a simple tuncation at the desired length
            string s2 = s.Substring(0, length);

            // Truncate the string at the word
            if (atWord)
            {
                // List of characters that denote the start or a new word (add to or remove more as necessary)
                List<char> alternativeCutOffs = new List<char>() { ' ', ',', '.', '?', '/', ':', ';', '\'', '\"', '\'', '-', '\n' };

                // Get the index of the last space in the truncated string
                int lastSpace = s2.LastIndexOf(' ');

                // If the last space index isn't -1 and also the next character in the original
                // string isn't contained in the alternativeCutOffs List (which means the previous
                // truncation actually truncated at the end of a word),then shorten string to the last space
                if (lastSpace != -1 && (s.Length >= length + 1 && !alternativeCutOffs.Contains(s.ToCharArray()[length])))
                    s2 = s2.Remove(lastSpace);
            }

            // Add Ellipsis if desired
            if (addEllipsis)
                s2 += "...";

            return s2;
        }
    }
}
