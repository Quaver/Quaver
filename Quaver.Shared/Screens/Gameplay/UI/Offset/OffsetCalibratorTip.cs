using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay.UI.Offset
{
    public class OffsetCalibratorTip : Sprite
    {
        private Sprite Icon { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public OffsetCalibratorTip()
        {
            Tint = ColorHelper.HexToColor("#242424");
            Size = new ScalableVector2(WindowManager.Width, 100);
            SetChildrenAlpha = true;

            Animations.Add(new Animation(AnimationProperty.Wait, Easing.Linear, 0, 1, 2000));

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 2),
                Tint = Colors.MainAccent
            };

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_information_button),
                Y = 18,
                Size = new ScalableVector2(28, 28)
            };

            // ReSharper disable once ObjectCreationAsStatement
            var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Play through the map. At the end, a new global audio offset will be suggested to you.", 22)
            {
                Parent = this,
                Y = Icon.Y + Icon.Height + 15,
                Alignment = Alignment.TopCenter
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 2),
                Tint = Colors.MainAccent,
                Alignment = Alignment.BotLeft
            };

            ThreadScheduler.RunAfter(() =>
            {
                ClearAnimations();
                text.Visible = false;
                FadeTo(0, Easing.Linear, 200);
            }, 3500);
        }
    }
}
