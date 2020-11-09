using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
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
                BrowserHelper.OpenURL($"https://quavergame.com/donate");
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