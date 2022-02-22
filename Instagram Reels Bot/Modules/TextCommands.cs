using Discord;
using Discord.Commands;
using System.Threading.Tasks;
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
        [Command("reel", RunMode = RunMode.Async)]
        public async Task ReelParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/reel/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Parse an instagram post:
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("p", RunMode = RunMode.Async)]
        public async Task PostParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/p/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Parse an instagram TV link:
        /// https://www.instagram.com/tv/
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("tv", RunMode = RunMode.Async)]
        public async Task TVParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/tv/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        /// <summary>
        /// Parse Story Link
        /// Ex: https://instagram.com/stories/wevolverapp/2718330735469161935
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("stories", RunMode = RunMode.Async)]
        public async Task StoryParser([Remainder] string args = null)
        {
            //form url:
            string url = "https://www.instagram.com/stories/" + args.Replace(" ", "/");
            await Responder(url, Context);
        }
        [Command("profile", RunMode = RunMode.Async)]
        public async Task ProfileParser([Remainder] string args = null)
        {
            // Check whitelist:
            if (!Whitelist.IsServerOnList(Context.Guild.Id))
            {
                // Ignore if not on list:
                return;
            }
            using (Context.Channel.EnterTypingState())
            {
                // Get IG account:
                InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

                string url = "https://instagram.com/" + args.Replace(" ", "/");

                // Process profile:
                InstagramProcessorResponse response = await instagram.PostRouter(url, (int)Context.Guild.PremiumTier, 1);

                // Check for failed post:
                if (!response.success)
                {
                    await Context.Message.ReplyAsync(response.error);
                    return;
                }
                // If not a profile for some reason, treat otherwise:
                if (!response.onlyAccountData)
                {
                    await Responder(url, Context);
                    return;
                }

                IGEmbedBuilder embed = new IGEmbedBuilder(response, Context.User.Username);
                IGComponentBuilder component = new IGComponentBuilder(response, Context.User.Id);

                await Context.Message.ReplyAsync(embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());

                //Attempt to remove any automatic embeds:
                DiscordTools.SuppressEmbeds(Context);
            }
        }
        /// <summary>
        /// Centralized method to handle all Instagram links and respond to text based messages (No slash commands).
        /// </summary>
        /// <param name="url">The Instagram URL of the content</param>
        /// <param name="context">The discord context of the message</param>
        /// <returns></returns>
        private static async Task Responder(string url, ICommandContext context)
        {
            // Check whitelist:
            if (!Whitelist.IsServerOnList(context.Guild.Id))
            {
                // Ignore if not on list:
                return;
            }
            using (context.Channel.EnterTypingState())
            {
                // Get IG account:
                InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());

                //Process Post:
                InstagramProcessorResponse response = await instagram.PostRouter(url, (int)context.Guild.PremiumTier, 1);

                //Check for failed post:
                if (!response.success)
                {
                    await context.Message.ReplyAsync(response.error);
                    return;
                }

                // Embed builder:
                IGEmbedBuilder embed = new IGEmbedBuilder(response, context.User.Username);
                IGComponentBuilder component = new IGComponentBuilder(response, context.User.Id);

                if (response.isVideo)
                {
                    if (response.stream != null)
                    {
                        //Response with stream:
                        using (Stream stream = new MemoryStream(response.stream))
                        {
                            FileAttachment attachment = new FileAttachment(stream, "IGMedia.mp4", "An Instagram Video.");
                            await context.Message.Channel.SendFileAsync(attachment, embed: embed.AutoSelector(), components: component.AutoSelector());
                        }
                    }
                    else
                    {
                        //Response without stream:
                        await context.Message.ReplyAsync(response.contentURL.ToString(), embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());
                    }

                }
                else
                {
                    if (response.stream != null)
                    {
                        using (Stream stream = new MemoryStream(response.stream))
                        {
                            FileAttachment attachment = new FileAttachment(stream, "IGMedia.jpg", "An Instagram Image.");
                            await context.Channel.SendFileAsync(attachment, embed: embed.AutoSelector(), components: component.AutoSelector());
                        }
                    }
                    else
                    {
                        await context.Message.ReplyAsync(embed: embed.AutoSelector(), allowedMentions: AllowedMentions.None, components: component.AutoSelector());
                    }
                }

                //Try to remove the embeds on the command post:
                DiscordTools.SuppressEmbeds(context);
            }
        }
    }
}