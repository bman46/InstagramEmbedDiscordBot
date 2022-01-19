using System;
using Discord;

namespace Instagram_Reels_Bot.Helpers
{
	public class IGEmbedBuilder
	{
		/// <summary>
        /// The Response from the Instagram processor.
        /// </summary>
		private InstagramProcessorResponse Response;
		private string Requester;
		/// <summary>
        /// Create an instance of the embed builder.
        /// </summary>
        /// <param name="Response"></param>
		public IGEmbedBuilder(InstagramProcessorResponse response, string requester)
		{
			this.Response = response;
			this.Requester = requester;
		}
		public Embed PostEmbed()
        {
			var embed = BaseEmbed();

			//Embeds:
			//Account Name:
			var account = new EmbedAuthorBuilder();
			account.IconUrl = Response.iconURL.ToString();
			account.Name = (string.IsNullOrEmpty(Response.accountName)) ? Response.username : Response.accountName;
			account.Url = Response.accountUrl.ToString();

			embed.Author = account;
			embed.Title = "Content from " + Requester + "'s linked post.";
			embed.Timestamp = new DateTimeOffset(Response.postDate);
			embed.Url = Response.postURL.ToString();
			embed.Description = (Response.caption != null) ? (DiscordTools.Truncate(Response.caption)) : ("");


            if (!Response.isVideo)
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
			embed.ThumbnailUrl = Response.iconURL.ToString();
			embed.Title = (string.IsNullOrEmpty(Response.accountName)) ? Response.username : Response.accountName + "'s Instagram Account";
			embed.Url = Response.accountUrl.ToString();
			embed.Description = "**Biography:**\n" + Response.bio + "\n\n";
			if (Response.externalURL != null)
			{
				embed.Description += "[Link in bio](" + Response.externalURL.ToString() + ")\n";
			}
			embed.Description += "Requested by: " + Requester;
			embed.Description += "\nUse the `/subscribe` command to subscribe to accounts.";

			//Post count:
			EmbedFieldBuilder posts = new EmbedFieldBuilder();
			posts.Name = "Posts:";
			posts.Value = String.Format("{0:n0}", Response.posts);
			posts.IsInline = true;
			embed.Fields.Add(posts);

			//Follower count:
			EmbedFieldBuilder followers = new EmbedFieldBuilder();
			followers.Name = "Followers:";
			followers.Value = String.Format("{0:n0}", Response.followers);
			followers.IsInline = true;
			embed.Fields.Add(followers);

			//Following count
			EmbedFieldBuilder following = new EmbedFieldBuilder();
			following.Name = "Following:";
			following.Value = String.Format("{0:n0}", Response.following);
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

