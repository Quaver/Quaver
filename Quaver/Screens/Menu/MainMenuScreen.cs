using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Audio;
using Quaver.Skinning;
using Wobble.Discord;
using Wobble.Screens;

namespace Quaver.Screens.Menu
{
    public class MainMenuScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public MainMenuScreen()
        {
            DiscordManager.Client.CurrentPresence.Details = "Idle";
            DiscordManager.Client.CurrentPresence.State = "In the menus";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);

            View = new MainMenuScreenView(this);
        }
    }
}
