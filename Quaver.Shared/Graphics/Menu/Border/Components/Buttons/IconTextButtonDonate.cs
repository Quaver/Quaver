using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonDonate : IconTextButton
    {
        public IconTextButtonDonate() : base(FontAwesome.Get(FontAwesomeIcon.fa_heart_shape_silhouette),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Donate", (sender, args) =>
            {
                NotificationManager.Show(NotificationLevel.Info, 
                    "Donating is currently unavailable from in-game and can only be done on the website.\n\n" +
                    "We are working on adding this back soon.");
            })
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsHovered)
            {
                Text.Tint = HoveredColor;
                Icon.Tint = HoveredColor;
            }
            else
            {
                Text.Tint = SkinManager.Skin?.MenuBorder?.ButtonTextColor ?? Color.White;
                Icon.Tint = Color.Crimson;
            }
        }
    }
}