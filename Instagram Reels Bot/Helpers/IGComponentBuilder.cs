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

            // add button to component
            component.WithButton(button);

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
                ButtonBuilder button = new ButtonBuilder();
                button.Label = "Link in bio";
                button.Style = ButtonStyle.Link;
                button.Url = Response.externalURL.ToString();

                // add button to component
                component.WithButton(button);
            }

            return component.Build();
        }
    }
}
