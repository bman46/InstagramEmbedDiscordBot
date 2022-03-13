using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Instagram_Reels_Bot.Modules
{
    public class ComponentRespond : InteractionModuleBase<ShardedInteractionContext>
    {
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;
        private Subscriptions _subscriptions;
        public ComponentRespond(CommandHandler handler, Subscriptions subs)
        {
            _handler = handler;
            _subscriptions = subs;
        }

        [ComponentInteraction("delete-message-*", runMode: RunMode.Async)]
        public async Task DeleteMessageButton(string userId)
        {
            var originalMessage = Context.Interaction as SocketMessageComponent;
            // Context.User.Id are user id of the user that interact with the button.
            if (Context.User.Id == ulong.Parse(userId))
            {
                // Validate authenticity:
                var orginResponse = originalMessage.Message;
                foreach(ActionRowComponent row in orginResponse.Components)
                {
                    foreach(IMessageComponent component in row.Components)
                    {
                        if (component.CustomId == "delete-message-" + userId)
                        {
                            await originalMessage.Message.DeleteAsync();
                            return;
                        }
                    }
                }
                await RespondAsync("Button user ID appears to be spoofed.", ephemeral: true);
            }
            // Also allow for admins to delete posts.
            else if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                await originalMessage.Message.DeleteAsync();
            }
            else
            {
                // if user is not the person who executed the command
                await RespondAsync("You are not allowed to delete that message.", ephemeral: true);
            }
        }

        [ComponentInteraction("unsubscribe", runMode: RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator, Group = "UserPerm")]
        [RequireRole("InstagramBotSubscribe", Group = "UserPerm")]
        [DisableSource]
        public async Task UnsubscribeMenu(string[] values)
        {
            //Ensure subscriptions are enabled:
            if (!_subscriptions.ModuleEnabled)
            {
                await RespondAsync("Subscriptions module is currently disabled.", ephemeral: true);
                return;
            }

            //Buy more time to process posts:
            await DeferAsync(true);

            foreach (string encodedData in values)
            {
                // Split String:
                long IGID = long.Parse(encodedData.Split("-")[0]);
                ulong chanID = ulong.Parse(encodedData.Split("-")[1]);

                // Remove Subscribe:
                try
                {
                    await _subscriptions.UnsubscribeToAccount(IGID, chanID, Context.Guild.Id);
                }
                catch (ArgumentException e) when (e.Message.Contains("Cannot find user."))
                {
                    await FollowupAsync("Error: You are not subscribed to that user.", ephemeral: true);
                    return;
                }

                string username;
                try
                {
                    // Get IG account:
                    InstagramProcessor instagram = new InstagramProcessor(InstagramProcessor.AccountFinder.GetIGAccount());
                    username = await instagram.GetIGUsername(IGID.ToString());
                }
                catch
                {
                    username = "*unknown user*";
                }

                try
                {
                    // Get Channel:
                    var chan = Context.Guild.GetChannel(chanID) as SocketTextChannel;

                    // Notify:
                    await chan.SendMessageAsync("This channel has been unsubscribed to " + username + " on Instagram by " + Context.User.Mention, allowedMentions: AllowedMentions.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Failed to send message (chan might have been deleted).
                }
            }
            await FollowupAsync("Success! You will no longer receive new posts to the selected channel(s).", ephemeral: true);
        }
    }
}
