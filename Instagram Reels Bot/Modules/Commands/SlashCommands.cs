using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Instagram_Reels_Bot.DataTables;
using Instagram_Reels_Bot.Helpers;
using Instagram_Reels_Bot.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace Instagram_Reels_Bot.Modules
{
	public partial class SlashCommands : InteractionModuleBase<ShardedInteractionContext>
	{
		// Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
		public InteractionService Commands { get; set; }

		private CommandHandler _handler;
		private Subscriptions _subscriptions;
        private readonly IConfiguration _config;

        // Constructor injection is also a valid way to access the dependecies
        public SlashCommands(CommandHandler handler, Subscriptions subs, IConfiguration config)
		{
			_handler = handler;
			_subscriptions = subs;
			_config = config;
		}

		private async Task<bool> EnsureWhitelist() {
            if (Whitelist.IsServerOnList(Context.Guild?.Id ?? 0)) {
                return true;
            }

            // Self-hosted whitelist notification for official bot:
            if (Context.Client.CurrentUser.Id == 815695225678463017) {
                await RespondAsync("This bot is now self-host only. Learn more about this change in the updates channel on the support server: https://discord.gg/8dkjmGSbYE", ephemeral: true);
            } else {
                await RespondAsync("This guild is not on the whitelist. The command was blocked.", ephemeral: true);
            }

            return false;
        }

        private async Task<bool> EnsureSubscription() {
			if (_subscriptions.ModuleEnabled) {
				return true;
			}

            await RespondAsync("Subscriptions module is currently disabled.", ephemeral: true);
            return false;
        }
	}
}