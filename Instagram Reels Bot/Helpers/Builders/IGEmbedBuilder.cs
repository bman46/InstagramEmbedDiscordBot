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
		private bool RequesterIsKnown
		{
			get
            {
				return !string.IsNullOrWhiteSpace(Requester);
            }
		}
		/// <summary>
        /// Create an instance of the embed builder.
        /// </summary>
        /// <param name="Response"></param>
		public IGEmbedBuilder(InstagramProcessorResponse response, string requester)
		{
			this.Response = response;
			this.Requester = requester;
		}
		/// <summary>
        /// For use when requester is not needed or unknown.
        /// </summary>
        /// <param name="response"></param>
		public IGEmbedBuilder(InstagramProcessorResponse response)
        {
			this.Response = response;
        }
		/// <summary>
        /// Automatically determines what embed type to use
        /// </summary>
        /// <returns>An embed</returns>
		public Embed AutoSelector()
        {
			if (Response.onlyAccountData)
            {
				return AccountEmbed();
            }
			return PostEmbed();

		}
		/// <summary>
        /// Builds an embed for IG posts
        /// </summary>
        /// <returns></returns>
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
			embed.Timestamp = new DateTimeOffset(Response.postDate);
			embed.Description = (Response.caption != null) ? (DiscordTools.Truncate(Response.caption)) : ("");
			// Check to see if requester is known:
			if (RequesterIsKnown)
			{
				embed.Title = "Content from " + Requester + "'s linked post.";
				embed.Url = Response.postURL.ToString();
            }
            if (Response.postCount > 1)
            {
				embed.Description += "\n\nThere ";

				// Plural or singular?
				if ((Response.postCount - 1) != 1)
				{
					embed.Description += "are ";
				}
				else
				{
					embed.Description += "is ";
				}

				embed.Description += (Response.postCount - 1) + " other ";

				// Plural or singular?
				if((Response.postCount - 1) != 1)
                {
					embed.Description += "images/videos";
                }
                else
                {
					embed.Description += "image/video";
                }
				embed.Description += " in this post.";
			}
			if (!Response.isVideo)
            {
				if (Response.stream != null)
				{
					embed.ImageUrl = "attachment://IGMedia.jpg";
                }
                else
                {
					embed.ImageUrl = Response.contentURL.ToString();
				}
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

			if (RequesterIsKnown)
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

