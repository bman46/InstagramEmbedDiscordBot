using System;
using System.Text.RegularExpressions;
using System.Threading;
using AE.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Instagram_Reels_Bot.Helpers
{
	public class EmailTools
	{
        private IConfiguration configComs;

        public EmailTools()
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "configCommunications.json");
            // build the configuration and assign to _config          
            configComs = _builder.Build();
        }
		public string GetVerificationCode(DateTime requestTime)
        {
            using (var imap = new AE.Net.Mail.ImapClient(configComs["emailConfig:host"], configComs["emailCredentials:username"], configComs["emailCredentials:password"], port: int.Parse(configComs["emailConfig:port"])))
            {
                for (int i = 0; i < 10; i++)
                {
                    var msgs = imap.SearchMessages(
                      SearchCondition.Undeleted().And(
                        SearchCondition.SentSince(requestTime),
                        SearchCondition.Subject("Verify your account"),
                        SearchCondition.Unseen(),
                        SearchCondition.From("security@mail.instagram.com")
                      )
                    );
                    //Ensure message is found
                    if (msgs.Length > 0)
                    {
                        var msg = msgs[0].Value;
                        msg.Flags = Flags.Seen;

                        Regex codeRegex = new Regex("\b[0-9]{6}\b");
                        Match match = codeRegex.Match(msg.Body);
                        if (match.Success)
                        {
                            return match.Value;
                        }
                    }
                    else
                    {
                        //Wait for message if not found
                        Thread.Sleep(2000);
                    }
                }
            }
            //Not found at all:
            throw new Exception("Failed to get code.");
        }
	}
}

