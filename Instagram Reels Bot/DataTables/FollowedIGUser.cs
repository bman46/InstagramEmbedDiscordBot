using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Instagram_Reels_Bot.DataTables
{
	public class FollowedIGUser
	{
		public FollowedIGUser()
		{
            LastPostDate = DateTime.Now;
            LastCheckTime = DateTime.Now;
        }

        [JsonProperty(PropertyName = "id")]
        public string InstagramID { get; set; }
        public DateTime LastPostDate { get; set; }
        public DateTime LastCheckTime { get; set; }
        public List<RespondChannel> SubscribedChannels { get; set; }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    /// <summary>
    /// The channel and guild to respond to new posts with.
    /// TODO: ensure that this always exists (deleted channels must unsubscribe)
    /// </summary>
    public class RespondChannel
    {
        public RespondChannel(ulong guildID, ulong ChannelID)
        {
            this.GuildID = guildID.ToString();
            this.ChannelID = ChannelID.ToString();
        }

        public string GuildID { get; set; }
        public string ChannelID { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

