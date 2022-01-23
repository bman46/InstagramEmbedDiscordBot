using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Instagram_Reels_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Reels_Bot.Modules
{
    public class ButtonRespond : InteractionModuleBase<ShardedInteractionContext>
    {
        public InteractionService Commands { get; set; }

        private CommandHandler _handler;
        public ButtonRespond(CommandHandler handler)
        {
            _handler = handler;
        }

        [ComponentInteraction("delete-message-*")]
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
    }
}
