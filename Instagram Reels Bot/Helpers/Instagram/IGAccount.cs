using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes;

namespace Instagram_Reels_Bot.Helpers.Instagram
{
	public class IGAccount : UserSessionData
	{
		public IGAccount() { }
		/// <summary>
		/// Account information without 2FA code.
		/// </summary>
		/// <param name="username">The account username</param>
		/// <param name="password">The account password</param>
		public IGAccount(string username, string password)
		{
			this.UserName = username;
			this.Password = password;
		}
		/// <summary>
		///  Account information with 2FA code.
		/// </summary>
		/// <param name="username">The account username</param>
		/// <param name="password">The account password</param>
		/// <param name="OTPSecret">The 2FA OTP secret code</param>
		public IGAccount(string username, string password, string OTPSecret)
		{
			this.UserName = username;
			this.Password = password;
			this.OTPSecret = OTPSecret;
		}
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
        public List<OperatingTimes> UsageTimes = new List<OperatingTimes>();

		/// <summary>
        /// A single time for the account to be used at
        /// </summary>
		public class OperatingTimes
		{
			/// <summary>
			/// Time to start using this account
			/// </summary>
			public TimeOnly StartTime;
			/// <summary>
			/// Time to stop using the account
			/// </summary>
			public TimeOnly EndTime;
		}
        #endregion

    }
}

