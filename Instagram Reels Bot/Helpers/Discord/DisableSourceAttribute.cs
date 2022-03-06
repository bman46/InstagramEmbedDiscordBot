using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace Instagram_Reels_Bot.Helpers
{
    /// <summary>
    /// Automagically adds attribute to disable the source of an interaction.
    /// [DisableSource]
    /// </summary>
    public sealed class DisableSourceAttribute : PreconditionAttribute
    {
        /// <summary>
        /// Precondition to disable the source of an interaction.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="commandInfo"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Interaction is not SocketMessageComponent messageComponent)
                return PreconditionResult.FromError("This attribute does not work for application commands!");

            var builder = new ComponentBuilder();

            var rows = ComponentBuilder.FromMessage(messageComponent.Message).ActionRows;

            for (int i = 0; i < rows.Count; i++)
            {
                foreach (var component in rows[i].Components)
                {
                    switch (component)
                    {
                        case ButtonComponent button:
                            builder.WithButton(button.ToBuilder()
                                .WithDisabled(true), i);
                            break;
                        case SelectMenuComponent menu:
                            builder.WithSelectMenu(menu.ToBuilder()
                                .WithDisabled(true), i);
                            break;
                    }
                }
            }

            try
            {
                await messageComponent.Message.ModifyAsync(x => x.Components = builder.Build());
                return PreconditionResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return PreconditionResult.FromError(ex);
            }
        }
    }
}
