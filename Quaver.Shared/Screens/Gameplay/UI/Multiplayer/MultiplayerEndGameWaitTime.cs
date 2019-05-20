using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Gameplay.UI.Multiplayer
{
    public class MultiplayerEndGameWaitTime : Sprite
    {
        private Sprite Icon { get; }

        private Sprite LoadingWheel { get;  }

        public MultiplayerEndGameWaitTime()
        {
            Image = UserInterface.WaitingPanel;
            Size = new ScalableVector2(450, 134);
            Alpha = 0;
            SetChildrenAlpha = true;

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_information_button),
                Y = 18,
                Size = new ScalableVector2(24, 24)
            };

            // ReSharper disable once ObjectCreationAsStatement
            var text = new SpriteTextBitmap(FontsBitmap.AllerRegular, "Waiting for other players to finish!")
            {
                Parent = this,
                FontSize = 20,
                Y = Icon.Y + Icon.Height + 10,
                Alignment = Alignment.TopCenter
            };

            LoadingWheel = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(40, 40),
                Image = UserInterface.LoadingWheel,
                Alignment = Alignment.TopCenter,
                Y = text.Y + text.Height + 10
            };
        }

        public override void Update(GameTime gameTime)
        {
            PerformLoadingWheelRotation();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Rotates the loading wheel endlessly
        /// </summary>
        private void PerformLoadingWheelRotation()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }
    }
}