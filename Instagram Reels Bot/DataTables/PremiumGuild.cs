using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Instagram_Reels_Bot.DataTables
{
	public class PremiumGuild
	{
		/// <summary>
		/// Creates a new premium guild that subscribes to paid services.
		/// set RecheckSubscribedAccounts to true if reducing user count
		/// </summary>
		/// <param name="GuildID">The guild</param>
		/// <param name="AdditionalAccounts">Number of additional accounts granted to the server.</param>
		/// <param name="RecheckSubscribedAccounts">Recheck on next runthrough for duplicates.</param>
		public PremiumGuild(ulong GuildID, uint AdditionalAccounts, bool RecheckSubscribedAccounts = false)
		{
			this.GuildID = GuildID.ToString();
			this.AdditionalAccounts = AdditionalAccounts;
			this.RecheckSubscribedAccounts = RecheckSubscribedAccounts;
		}
		[BsonId]
		public ObjectId Id { get; set; }
		public string GuildID { get; set; }
		public uint AdditionalAccounts { get; set; }
		public List<long> PaidDiscordUsers { get; set; }
		public bool RecheckSubscribedAccounts { get; set; }
	}
}

