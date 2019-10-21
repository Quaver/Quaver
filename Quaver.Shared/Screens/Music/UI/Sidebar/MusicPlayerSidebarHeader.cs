using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.Sidebar
{
    public class MusicPlayerSidebarHeader : Sprite
    {
        /// <summary>
        /// </summary>
        private Sprite DividerLine { get; }

        /// <summary>
        /// </summary>
        private IconTextButton HeaderText { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        public MusicPlayerSidebarHeader(float width, Texture2D icon, string text, bool hasTopLine = true)
        {
            Size = new ScalableVector2(width, 52);
            Tint = ColorHelper.HexToColor("#181818");

            if (hasTopLine)
            {
                var dividerLineTop = new Sprite
                {
                    Parent = this,
                    Size = new ScalableVector2(Width, 2),
                    Alignment = Alignment.TopLeft,
                    Alpha = 0.65f
                };
            }

            HeaderText = new IconTextButton(icon,
                FontManager.GetWobbleFont(Fonts.LatoBlack), text, null, Color.White, Color.White)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 14
            };

            DividerLine = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 2),
                Alignment = Alignment.BotLeft,
                Alpha = 0.65f
            };
        }
    }
}