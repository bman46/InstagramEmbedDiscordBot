using System;
using Discord;

namespace Instagram_Reels_Bot.Helpers
{
	public class IGEmbedBuilder
	{
		/// <summary>
        /// The response from the Instagram processor.
        /// </summary>
		private InstagramProcessorResponse response;
		private string requester;
		/// <summary>
        /// Create an instance of the embed builder.
        /// </summary>
        /// <param name="response"></param>
		public IGEmbedBuilder(InstagramProcessorResponse response, string requester)
		{
			this.response = response;
			this.requester = requester;
		}
		public Embed PostEmbed()
        {
			var embed = BaseEmbed();

			//Embeds:
			//Account Name:
			var account = new EmbedAuthorBuilder();
			account.IconUrl = response.iconURL.ToString();
			account.Name = (string.IsNullOrEmpty(response.accountName)) ? response.username : response.accountName;
			account.Url = response.accountUrl.ToString();

			embed.Author = account;
			embed.Title = "Content from " + requester + "'s linked post.";
			embed.Timestamp = new DateTimeOffset(response.postDate);
			embed.Url = response.postURL.ToString();
			embed.Description = (response.caption != null) ? (DiscordTools.Truncate(response.caption)) : ("");


            if (!response.isVideo)
            {
				embed.ImageUrl = "attachment://IGMedia.jpg";
			}

			return embed.Build();
		}
		/// <summary>
        /// Embed for accounts
        /// </summary>
        /// <returns></returns>
		public Embed AccountEmbed()
        {
			var embed = BaseEmbed();

			//custom embed for profiles:
			embed.ThumbnailUrl = response.iconURL.ToString();
			embed.Title = (string.IsNullOrEmpty(response.accountName)) ? response.username : response.accountName + "'s Instagram Account";
			embed.Url = response.accountUrl.ToString();
			embed.Description = "**Biography:**\n" + response.bio + "\n\n";
			if (response.externalURL != null)
			{
				embed.Description += "[Link in bio](" + response.externalURL.ToString() + ")\n";
			}
			embed.Description += "Requested by: " + requester;
			embed.Description += "\nUse the `/subscribe` command to subscribe to accounts.";

			//Post count:
			EmbedFieldBuilder posts = new EmbedFieldBuilder();
			posts.Name = "Posts:";
			posts.Value = String.Format("{0:n0}", response.posts);
			posts.IsInline = true;
			embed.Fields.Add(posts);

			//Follower count:
			EmbedFieldBuilder followers = new EmbedFieldBuilder();
			followers.Name = "Followers:";
			followers.Value = String.Format("{0:n0}", response.followers);
			followers.IsInline = true;
			embed.Fields.Add(followers);

			//Following count
			EmbedFieldBuilder following = new EmbedFieldBuilder();
			following.Name = "Following:";
			following.Value = String.Format("{0:n0}", response.following);
			following.IsInline = true;
			embed.Fields.Add(following);

			return embed.Build();
		}
		/// <summary>
        /// The basic structure for embeds
        /// </summary>
        /// <returns>An embed builder with basic settings</returns>
		public EmbedBuilder BaseEmbed()
        {
			EmbedBuilder embed = new EmbedBuilder();

			//Instagram Footer:
			EmbedFooterBuilder footer = new EmbedFooterBuilder();
			footer.IconUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png";
			footer.Text = "Instagram";

			//Basic embed params:
			embed.WithColor(new Color(131, 58, 180));
			embed.Footer = footer;

			return embed;
		}
	}
}

