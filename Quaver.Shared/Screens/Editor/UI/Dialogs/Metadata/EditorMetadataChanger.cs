using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataChanger : ScrollContainer
    {
        public EditorMetadataChanger() : base(new ScalableVector2(400, 600), new ScalableVector2(400, 600))
        {
            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 5;
            Scrollbar.X += 10;
            ScrollSpeed = 320;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;

            Tint = ColorHelper.HexToColor("#161616");
            Alpha = 1f;

            CreateBorderLines();
        }

        /// <summary>
        /// </summary>
        private void CreateBorderLines()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 2),
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(2, Height),
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 2),
            };
        }
    }
}