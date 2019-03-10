using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
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
            Tint = Color.Black;
            Size = new ScalableVector2(WindowManager.Width, 100);
            Alpha = 0;
            SetChildrenAlpha = true;

            FadeTo(0.85f, Easing.Linear, 400);
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
                Size = new ScalableVector2(24, 24)
            };

            // ReSharper disable once ObjectCreationAsStatement
            var text = new SpriteTextBitmap(FontsBitmap.AllerRegular, "Play through the map, and at the end, a new global audio offset will be suggested to you.")
            {
                Parent = this,
                FontSize = 20,
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
