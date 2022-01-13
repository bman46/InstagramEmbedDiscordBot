using System;
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
		public string GetVerificationCode()
        {
            using (var imap = new AE.Net.Mail.ImapClient(configComs["emailConfig"]["Host"], username, password, AE.Net.Mail.ImapClient.AuthMethods.Login, port, isSSL))
            {
                var msgs = imap.SearchMessages(
                  SearchCondition.Undeleted().And(
                    SearchCondition.From("david"),
                    SearchCondition.SentSince(new DateTime(2000, 1, 1))
                  ).Or(SearchCondition.To("andy"))
                );

            }
        }
	}
}

