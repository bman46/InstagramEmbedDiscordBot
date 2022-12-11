using Discord;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Instagram_Reels_Bot.Helpers
{
    internal class IGComponentBuilder
    {
        /// <summary>
        /// The Response from the Instagram processor.
        /// </summary>
        private InstagramProcessorResponse Response;
        private readonly ulong RequesterId;
        private readonly bool EnableDeleteButton;
        private readonly IConfiguration _config;
        private bool RequesterIsKnown
        {
            get
            {
                return RequesterId != default(ulong);
            }
        }
        /// <summary>
        /// Create an instance of the component builder.
        /// </summary>
        /// <param name="response"></param>
        public IGComponentBuilder(InstagramProcessorResponse response, ulong requesterId, IConfiguration config)
        {
            this.Response = response;
            this.RequesterId = requesterId;
            _config = config;
            EnableDeleteButton = true;
            if (config["EnableDeleteButton"]?.ToLower() == "false")
            {
                EnableDeleteButton = false;
            }
        }
        /// <summary>
        /// For use when requester is not needed or unknown.
        /// </summary>
        /// <param name="response"></param>
        public IGComponentBuilder(InstagramProcessorResponse response, IConfiguration config)
        {
            this.Response = response;
            _config = config;
            EnableDeleteButton = true;
            if (config["EnableDeleteButton"]?.ToLower() == "false")
            {
                EnableDeleteButton = false;
            }
        }
        /// <summary>
        /// Automatically determines what component type to use
        /// </summary>
        /// <returns>An message component</returns>
		public MessageComponent AutoSelector()
        {
            if (Response.onlyAccountData)
            {
                return AccountComponent(); // Link in bio
            }
            return PostComponent(); // View on IG
        }
        /// <summary>
        /// The basic structure for Component
        /// </summary>
        /// <returns>An component builder with basic settings</returns>
		public ComponentBuilder BaseComponent()
        {
            ComponentBuilder component = new ComponentBuilder();

            return component;
        }
        /// <summary>
        /// Builds a Component for IG posts
        /// </summary>
        /// <returns></returns>
		public MessageComponent PostComponent()
        {
            var component = BaseComponent();

            // create button
            ButtonBuilder button = new ButtonBuilder();
            button.Label = "View on IG";
            button.Style = ButtonStyle.Link;
            button.Url = Response.postURL.ToString();

            component.WithButton(button);

            // add button to component
            if (RequesterIsKnown && EnableDeleteButton)
            {
                component.WithButton("Delete Message", $"delete-message-{RequesterId}", style: ButtonStyle.Danger);
            }

            return component.Build();
        }
        /// <summary>
        /// Builds a Component for accounts (profile)
        /// </summary>
        /// <returns></returns>
		public MessageComponent AccountComponent()
        {
            var component = BaseComponent();

            // Check for external URL
            if (Response.externalURL != null)
            {
                // create button
                ButtonBuilder buttonLinkBio = new ButtonBuilder();
                buttonLinkBio.Label = "Link in bio";
                buttonLinkBio.Style = ButtonStyle.Link;
                buttonLinkBio.Url = Response.externalURL.ToString();

                component.WithButton(buttonLinkBio);
            }
            // Add the delete button if user is known
            if (RequesterIsKnown && EnableDeleteButton)
            {
                component.WithButton("Delete Message", $"delete-message-{RequesterId}", style: ButtonStyle.Danger);
            }

            return component.Build();
        }
    }
}
