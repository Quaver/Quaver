using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Audio;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Screens.Edit.UI
{
    public class EditorSeekBar : Button
    {
        /// <summary>
        ///     The axis of the seek bar.
        /// </summary>
        private SeekBarAxis Axis { get; }

        /// <summary>
        ///     The line that displays where the progress is.
        /// </summary>
        private Sprite Progress { get; }

        /// <summary>
        ///     SpriteText that displays the the percentage progress in the song.
        /// </summary>
        private SpriteText TextProgressPercent { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="size"></param>
        public EditorSeekBar(SeekBarAxis axis, Vector2 size)
        {
            Axis = axis;

            Size = new ScalableVector2(size.X, size.Y);
            Tint = Color.Black;
            Alpha = 0.75f;

            Progress = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };

            TextProgressPercent = new SpriteText(BitmapFonts.Exo2Regular, "", 18)
            {
                Tint = Color.White,
                Parent = Progress,
                TextAlignment = Alignment.MidCenter,
            };

            switch (Axis)
            {
                case SeekBarAxis.Horizontal:
                    Progress.Size = new ScalableVector2(1, Height);
                    TextProgressPercent.Y = -20;
                    break;
                case SeekBarAxis.Vertical:
                    Progress.Size = new ScalableVector2(Width, 1);
                    TextProgressPercent.X = 20;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Clicked += (sender, args) =>
            {
                // Get where the user's mouse is when they've clicked, and set the audio position to that.
                switch (Axis)
                {
                    case SeekBarAxis.Horizontal:
                        var mouseX = MouseManager.CurrentState.X;
                        AudioEngine.Track.Seek(mouseX / Width * AudioEngine.Track.Length);
                        break;
                    case SeekBarAxis.Vertical:
                        var mouseY = MouseManager.CurrentState.Y;
                        AudioEngine.Track.Seek(mouseY / Height * AudioEngine.Track.Length);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            switch (Axis)
            {
                case SeekBarAxis.Horizontal:
                    Progress.X = (float) AudioEngine.Track.ProgressPercentage / 100 * Width;
                    break;
                case SeekBarAxis.Vertical:
                    Progress.Y = (float) AudioEngine.Track.ProgressPercentage / 100 * Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TextProgressPercent.Text = $"{(int) AudioEngine.Track.ProgressPercentage}%";

            base.Update(gameTime);
        }
    }

    /// <summary>
    ///     The direction that the seek bar faces.
    /// </summary>
    public enum SeekBarAxis
    {
        Horizontal,
        Vertical
    }
}
