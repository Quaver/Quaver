using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics
{
    public class LoadingWheel : Sprite
    {
        public LoadingWheel() => Image = UserInterface.LoadingWheel;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
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
            if (Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(Rotation);
            ClearAnimations();
            Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }
    }
}