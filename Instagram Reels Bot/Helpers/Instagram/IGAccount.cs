using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes;

namespace Instagram_Reels_Bot.Helpers.Instagram
{
	public class IGAccount : UserSessionData
	{
		/// <summary>
        /// Default class constructor
        /// </summary>
		public IGAccount() { }

		/// <summary>
		/// The 2FA secret code for generating OTPs
		/// </summary>
		public string OTPSecret { get; set; }

		/// <summary>
		/// Set to true if login failed.
		/// Defaults to false.
		/// </summary>
		public bool FailedLogin = false;

        #region times
        /// <summary>
        /// List of times that this account should be used at
        /// </summary>
        public List<OperatingTime> UsageTimes = new List<OperatingTime>();

		/// <summary>
        /// A single time for the account to be used at
        /// </summary>
		public class OperatingTime
		{
			/// <summary>
			/// Time to start using this account
			/// </summary>
			public TimeOnly StartTime;
			/// <summary>
			/// Time to stop using the account
			/// </summary>
			public TimeOnly EndTime;

			/// <summary>
            /// Check to see if the time is valid
            /// </summary>
            /// <returns></returns>
			public bool BetweenStartAndEnd(TimeOnly checkTime)
            {
				return checkTime >= StartTime && checkTime <= EndTime;
            }
		}
        #endregion

    }
}

