using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Audio;
using Quaver.Skinning;
using Wobble.Screens;

namespace Quaver.Screens.Menu
{
    public class MainMenuScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public MainMenuScreen() => View = new MainMenuScreenView(this);
    }
}
