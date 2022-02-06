using System;
using System.Collections.Generic;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using Microsoft.Extensions.Configuration;

namespace Instagram_Reels_Bot.Helpers.Instagram
{
	public class IGAccount : UserSessionData
	{
		/// <summary>
        /// Default class constructor
        /// </summary>
		public IGAccount() { }

		/// <summary>
		/// The IG processor for the account:
		/// </summary>
		public InstagramApiSharp.API.IInstaApi instaApi;

		/// <summary>
        /// Load the InstaAPI
        /// </summary>
		public void InitializeAPI()
        {
			//Create the instaApi object:
			instaApi = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.Exceptions))
				.Build();

			// Set the Android Device:
			instaApi.SetDevice(InstagramProcessor.device);

			// Set API version to v180:
			instaApi.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version180);
		}

		/// <summary>
		/// The 2FA secret code for generating OTPs
		/// </summary>
		public string OTPSecret { get; set; }

		/// <summary>
		/// Set to true if the account has be blocked or failed to log in.
		/// Defaults to false.
		/// </summary>
		public bool Blacklist = false;

        #region times
        /// <summary>
        /// List of times that this account should be used at
        /// </summary>
        public List<OperatingTime> UsageTimes = new List<OperatingTime>();

		/// <summary>
		/// A single time for the account to be used at
		/// </summary>
        [Serializable]
		public class OperatingTime
		{
			/// <summary>
			/// Time to start using this account
			/// </summary>
			public TimeOnly StartTime;
			public int StartHour
			{
				get
				{
					return StartTime.Hour;
				}
				set
				{
					StartTime = new TimeOnly(value, 0);
				}
			}
			/// <summary>
			/// Time to stop using the account
			/// </summary>
			public TimeOnly EndTime;
			public int EndHour
			{
				get
				{
					return EndTime.Hour;
				}
				set
				{
					EndTime = new TimeOnly(value, 0);
				}
			}
			/// <summary>
			/// Check to see if the time is valid
			/// </summary>
			/// <returns></returns>
			public bool BetweenStartAndEnd(TimeOnly checkTime)
            {
				return checkTime.IsBetween(StartTime, EndTime);
            }
		}
        #endregion

    }
}

