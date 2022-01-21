using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Reels_Bot.Helpers
{
    internal class IGComponentBuilder
    {
        /// <summary>
        /// The Response from the Instagram processor.
        /// </summary>
        private InstagramProcessorResponse Response;
        private ulong RequesterId;
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
        public IGComponentBuilder(InstagramProcessorResponse response, ulong requesterId)
        {
            this.Response = response;
            this.RequesterId = requesterId;
        }
        /// <summary>
        /// For use when requester is not needed or unknown.
        /// </summary>
        /// <param name="response"></param>
        public IGComponentBuilder(InstagramProcessorResponse response)
        {
            this.Response = response;
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
            if (RequesterIsKnown)
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

            if (Response.externalURL == null) // if there is no external url, then no button
            {
                return component.Build(); // no button
            }
            else
            {
                // create button
                ButtonBuilder buttonLinkBio = new ButtonBuilder();
                buttonLinkBio.Label = "Link in bio";
                buttonLinkBio.Style = ButtonStyle.Link;
                buttonLinkBio.Url = Response.externalURL.ToString();

                // add button to component
                if (RequesterIsKnown)
                {
                    component.WithButton(buttonLinkBio)
                        .WithButton("Delete Message", $"delete-message-{RequesterId}", style: ButtonStyle.Danger);
                }
                else
                {
                    component.WithButton(buttonLinkBio);
                }
            }

            return component.Build();
        }
    }
}
