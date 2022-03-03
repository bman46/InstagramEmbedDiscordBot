using System;

namespace Instagram_Reels_Bot.Helpers
{
	public class InstagramProcessorResponse
	{
        #region constructors
        /// <summary>
        /// Creates a new successful Instagram response.
        /// </summary>
        /// <param name="isVideo">Set to true if the response is a video</param>
        /// <param name="caption">Post caption</param>
        /// <param name="accountName">The name of the IG account.</param>
        /// <param name="username">The instagram username of the account</param>
        /// <param name="accountImage">Link to the account's image</param>
        /// <param name="contentURL">URL to the content (image of video)</param>
        /// <param name="postURL">URL to the post.</param>
        /// <param name="date">Time of the post.</param>
        /// <param name="stream">Byte array of the downloaded video or image.</param>
        /// <param name="postCount">The amount of images/videos (for carousel posts)</param>
        public InstagramProcessorResponse(bool isVideo, string caption, string accountName, string username, Uri accountImage, string contentURL, string postURL, DateTime date, byte[] stream, int postCount)
		{
			this.isVideo = isVideo;
			this.caption = caption;
			this.contentURL = new Uri(contentURL);
			this.postURL = new Uri(postURL);
			this.postDate = date;
			this.accountName = accountName;
			this.iconURL = accountImage;
			this.accountUrl = new Uri("https://www.instagram.com/"+username);
			this.username = username;
			this.postCount = postCount;

			if (stream != null)
            {
				this.stream = new Byte[stream.Length];
				stream.CopyTo(this.stream, 0);
            }
            else
            {
				this.stream = null;
            }
		}
		/// <summary>
		/// Creates a new successful Instagram response with only profile information.
		/// </summary>
		/// <param name="accountName"></param>
		/// <param name="username"></param>
		/// <param name="accountImage"></param>
		/// <param name="followers"></param>
		/// <param name="following"></param>
		/// <param name="posts"></param>
		/// <param name="bio"></param>
        /// <param name="externalURL"></param>
		public InstagramProcessorResponse(string accountName, string username, Uri accountImage, long followers, long following, long posts, string bio, string externalURL)
        {
			this.accountName = accountName;
			this.iconURL = accountImage;
			this.accountUrl = new Uri("https://www.instagram.com/" + username);
			this.followers = followers;
			this.following = following;
			this.posts = posts;
			this.bio = bio;
			this.onlyAccountData = true;
			this.username = username;

			if (!string.IsNullOrEmpty(externalURL))
            {
				this.externalURL = new Uri(externalURL);
            }
		}
		/// <summary>
		/// Creates a new failed Instagram response.
		/// </summary>
		/// <param name="error">The error reason or message string.</param>
		/// <param name="success">Defaults to false for error</param>
		public InstagramProcessorResponse(string error, bool success = false)
		{
			this.success = success;
			this.error = error;
		}
        #endregion

        #region error handling
        public Boolean success = true;
		public string error = "";
        #endregion

        #region data
        public Boolean isVideo = false;
		public int postCount = 1;
		public string caption = "";
		public Uri contentURL = null;
		public Uri postURL = null;
		public byte[] stream = null;
		public long sizeByte
		{
			get
			{
				return stream.Length;
			}
		}
		public DateTime postDate = DateTime.Now;
		#endregion

		#region IG Account data
		public bool onlyAccountData = false;
        public Uri accountUrl = null;
		public Uri iconURL = null;
		public long followers = 0;
		public long following = 0;
		public long posts = 0;
		public string bio = "";
		public string accountName = "";
		public string username = "";
		public Uri externalURL = null;
        #endregion
    }
}