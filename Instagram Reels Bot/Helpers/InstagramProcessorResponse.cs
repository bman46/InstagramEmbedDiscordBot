using System;
using System.IO;

namespace Instagram_Reels_Bot.Helpers
{
	public class InstagramProcessorResponse
	{
		public InstagramProcessorResponse(bool isVideo, string caption, string contentURL, string postURL, MemoryStream stream)
		{
			this.isVideo = isVideo;
			this.caption = caption;
			this.contentURL = new Uri(contentURL);
			this.postURL = new Uri(postURL);
			if (stream != null)
			{
				this.stream = stream.ToArray();
			}
		}
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
		public Byte[] stream = null;

	}
}