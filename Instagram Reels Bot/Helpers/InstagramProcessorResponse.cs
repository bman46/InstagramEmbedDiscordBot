using System;
using System.IO;

namespace Instagram_Reels_Bot.Helpers
{
	public class InstagramProcessorResponse
	{
		/// <summary>
        /// Creates a new successful Instagram response.
        /// </summary>
        /// <param name="isVideo">Set to true if the response is a video</param>
        /// <param name="caption">Post caption</param>
        /// <param name="contentURL">URL to the content (image of video)</param>
        /// <param name="postURL">URL to the post.</param>
        /// <param name="date">Time of the post.</param>
        /// <param name="stream">Byte array of the downloaded video or image.</param>
		public InstagramProcessorResponse(bool isVideo, string caption, string contentURL, string postURL, DateTime date, byte[] stream)
		{
			this.isVideo = isVideo;
			this.caption = caption;
			this.contentURL = new Uri(contentURL);
			this.postURL = new Uri(postURL);
			this.postDate = date;
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
		/// Creates a new failed Instagram response.
		/// </summary>
		/// <param name="error">The error reason or message string.</param>
		/// <param name="success">Defaults to false for error</param>
		public InstagramProcessorResponse(string error, bool success = false)
		{
			this.success = success;
			this.error = error;
		}
		public Boolean success = true;
		public string error = "";
		public Boolean isVideo = false;
		public string caption = "";
		public Uri contentURL = null;
		public Uri postURL = null;
		public long sizeByte = 0;
		public byte[] stream = null;
		public DateTime postDate = DateTime.Now;
	}
}