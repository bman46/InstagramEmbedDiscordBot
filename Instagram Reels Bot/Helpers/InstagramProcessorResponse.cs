using System;
using System.IO;

namespace Instagram_Reels_Bot.Helpers
{
	public class InstagramProcessorResponse
	{
		public InstagramProcessorResponse(bool isVideo, string caption, string contentURL, string postURL, byte[] stream)
		{
			this.isVideo = isVideo;
			this.caption = caption;
			this.contentURL = new Uri(contentURL);
			this.postURL = new Uri(postURL);
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
		public byte[] stream;

	}
}