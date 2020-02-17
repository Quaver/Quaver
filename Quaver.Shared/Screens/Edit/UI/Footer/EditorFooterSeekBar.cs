using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class EditorFooterSeekBar : Slider
    {
        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="binded"></param>
        /// <param name="size"></param>
        /// <param name="track"></param>
        public EditorFooterSeekBar(BindableInt binded, Vector2 size, IAudioTrack track) : base(binded, size, UserInterface.VolumeSliderProgressBall)
        {
            Track = track;
            ActiveColor.Image = UserInterface.VolumeSliderActive;
            Image = UserInterface.VolumeSliderInactive;

            ProgressBall.Size = new ScalableVector2(24, 24);

            BindedValue.ValueChanged += (sender, args) =>
            {
                if (!IsHeld)
                    return;

                if (Math.Abs(Track.Time - args.Value) < 200)
                    return;

                Track.Seek(args.Value);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            BindedValue.Value = (int) Track.Time;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BindedValue.Dispose();
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the click area of the slider.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            // The RectY increase of the click area.
            const int offset = 36;

            DrawRectangle clickArea;

            clickArea = new DrawRectangle(ScreenRectangle.X, ScreenRectangle.Y - offset / 2f, ScreenRectangle.Width,
                ScreenRectangle.Height + offset);

            return GraphicsHelper.RectangleContains(clickArea, MouseManager.CurrentState.Position);
        }
    }
}