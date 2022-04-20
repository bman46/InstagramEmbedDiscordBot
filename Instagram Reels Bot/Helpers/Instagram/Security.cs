using System.Threading;
using OtpNet;

namespace Instagram_Reels_Bot.Helpers.Instagram
{
	public class Security
	{
		/// <summary>
        /// Gets a 2FA code
        /// (RFC3548)
        /// </summary>
        /// <param name="secret">The secret for generating the OTP</param>
        /// <returns>6 digit OTP</returns>
		public static string GetTwoFactorAuthCode(string secret)
		{
			//Convert secret and remove excess spaces:
			var bytes = Base32Encoding.ToBytes(secret.Replace(" ",""));
			var totp = new Totp(bytes);

			if (totp.RemainingSeconds() > 1)
			{
				return totp.ComputeTotp();
			}
			else
			{
				//Wait for next code if the current code is about to expire:
				Thread.Sleep(totp.RemainingSeconds() * 1000 + 100);
				return totp.ComputeTotp();
			}
		}
	}
}