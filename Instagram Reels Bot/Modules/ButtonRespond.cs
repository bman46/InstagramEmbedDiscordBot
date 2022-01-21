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
            // Context.User.Id are user id of the user that interact with the button.
            // Also allow for admins with ManageMessages permission to delete posts.
            if (Context.User.Id == ulong.Parse(userId) || (Context.User as SocketGuildUser).GuildPermissions.ManageMessages)
            {
                var originalMessage = Context.Interaction as SocketMessageComponent;
                await originalMessage.Message.DeleteAsync();
            }
            else
            {
                // if user is not the person who executed the command
                await RespondAsync("You can't delete this message", ephemeral: true);
            }
        }
    }
}
