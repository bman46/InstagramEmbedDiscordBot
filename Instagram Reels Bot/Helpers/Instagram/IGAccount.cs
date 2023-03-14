using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Logger;
using Org.BouncyCastle.Asn1.X500;

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
			if (StaticDevice == null)
			{
                StaticDevice = AndroidDeviceGenerator.GetRandomAndroidDevice();
			}
			else
			{
				Random rand = new Random();
                StaticDevice.IGBandwidthSpeedKbps = $"{rand.Next(1233, 1567)}.{rand.Next(100, 999)}";
                StaticDevice.IGBandwidthTotalTimeMS = rand.Next(781, 999).ToString();
				StaticDevice.DeviceId = ApiRequestMessage.GenerateDeviceIdFromGuid(StaticDevice.DeviceGuid);
            }
		}

		/// <summary>
		/// The 2FA secret code for generating OTPs
		/// </summary>
		public string OTPSecret { get; set; }

		/// <summary>
		/// List of times that this account should be used at
		/// </summary>
		public AndroidDevice StaticDevice { get; set; }

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

