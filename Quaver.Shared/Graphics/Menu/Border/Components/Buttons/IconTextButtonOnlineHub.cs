using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonOnlineHub : IconButton
    {
        /// <summary>
        /// </summary>
        public IconTextButtonOnlineHub() : base(FontAwesome.Get(FontAwesomeIcon.fa_reorder_option), OnClicked)
        {
            Size = new ScalableVector2(44, 44);
            Tint = SkinManager.Skin?.MenuBorder?.ButtonTextColor ?? Color.White;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (GameBase.Game is QuaverGame game)
            {
                var img = game.OnlineHub.Sections.Any(x => x.Value.IsUnread) ? UserInterface.HubNotificationIconUnread : UserInterface.HubNotificationIcon;

                if (Image != img)
                    Image = img;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnClicked(object sender, EventArgs e)
        {
            if (GameBase.Game is QuaverGame game)
                game.ToggleOnlineHub();
        }
    }
}