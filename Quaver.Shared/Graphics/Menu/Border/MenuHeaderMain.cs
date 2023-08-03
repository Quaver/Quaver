using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Wobble.Graphics;

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