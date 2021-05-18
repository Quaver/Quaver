using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border
{
    public class MenuHeaderMain : MenuBorder
    {
        /// <summary>
        /// </summary>
        private DrawableLoggedInUser User { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuHeaderMain() : base(MenuBorderType.Header, new List<Drawable>
            {
                new MenuBorderLogo(),
                new IconTextButtonHome(),
                new IconTextButtonDownloadMaps(),
                new IconTextButtonMusicPlayer(),
                new IconTextButtonTheater(),
                new IconTextButtonSkins(),
                new IconTextButtonDonate()
            },
            new List<Drawable>
            {
                new IconTextButtonOnlineHub(),
            })
        {
            User = new DrawableLoggedInUser();
            User.Resized += OnUserResized;

            RightAlignedItems.Add(User);
            RightAlignedItems.Add(new DrawableSessionTime());

            AlignRightItems();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            User.Resized -= OnUserResized;
            base.Destroy();
        }

        private void OnUserResized(object sender, LoggedInUserResizedEventArgs e) => AlignRightItems();
    }
}