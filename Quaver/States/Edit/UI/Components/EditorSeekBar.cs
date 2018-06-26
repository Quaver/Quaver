using System;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Components
{
    internal class EditorSeekBar : Button
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
        internal EditorSeekBar(SeekBarAxis axis, Vector2 size)
        {
            Axis = axis;
            
            Size = new UDim2D(size.X, size.Y);
            Tint = Color.Black;
            Alpha = 0.75f;

            Progress = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft       
            };

            TextProgressPercent = new SpriteText()
            {
                TextColor = Color.White,
                Font = QuaverFonts.AssistantRegular16,
                Parent = Progress,
                TextAlignment = Alignment.MidCenter,
                TextScale = 0.85f
            };

            switch (Axis)
            {
                case SeekBarAxis.Horizontal:
                    Progress.Size = new UDim2D(1, SizeY);
                    TextProgressPercent.PosY = -20;
                    break;
                case SeekBarAxis.Vertical:
                    Progress.Size = new UDim2D(SizeX, 1);
                    TextProgressPercent.PosX = 20;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            switch (Axis)
            {
                case SeekBarAxis.Horizontal:
                    Progress.PosX = GameBase.AudioEngine.ProgressPercentage / 100 * SizeX;
                    break;
                case SeekBarAxis.Vertical:
                    Progress.PosY = GameBase.AudioEngine.ProgressPercentage / 100 * SizeY;           
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            TextProgressPercent.Text = $"{(int) GameBase.AudioEngine.ProgressPercentage}%";
            base.Update(dt);
        }
        
        protected override void MouseOut()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOver()
        {         
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        protected override void OnClicked()
        {
            // Get where the user's mouse is when they've clicked, and set the audio position to that.
            switch (Axis)
            {
                case SeekBarAxis.Horizontal:
                    var mouseX = GameBase.MouseState.X;
                    GameBase.AudioEngine.ChangeSongPosition(mouseX / SizeX * GameBase.AudioEngine.Length);
                    break;
                case SeekBarAxis.Vertical:
                    var mouseY = GameBase.MouseState.Y;
                    GameBase.AudioEngine.ChangeSongPosition(mouseY / SizeY * GameBase.AudioEngine.Length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            base.OnClicked();
        }
    }

    /// <summary>
    ///     The direction that the seek bar faces.
    /// </summary>
    internal enum SeekBarAxis
    {
        Horizontal,
        Vertical
    }
}