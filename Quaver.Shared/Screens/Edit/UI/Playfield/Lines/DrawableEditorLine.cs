using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public abstract class DrawableEditorLine : FilledRectangleSprite, IStartTime
    {
        protected EditorPlayfield Playfield { get; }

        protected static ScalableVector2 DefaultSize { get; } = new ScalableVector2(40, 2);
        
        public DrawableEditorLine(EditorPlayfield playfield)
        {
            Playfield = playfield;
            Size = DefaultSize;

            // ReSharper disable once VirtualMemberCallInConstructor
            Tint = GetColor();
        }

        /// <summary>
        ///     Sets the position of the line
        /// </summary>
        public virtual void SetPosition()
        {
            var x = Playfield.AbsolutePosition.X + Playfield.Width + 2 - Width / 2;
            var y = Playfield.HitPositionY - StartTime * Playfield.TrackSpeed - Height;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != x || Y != y)
                Position = new ScalableVector2(x, y);
        }
        
        /// <summary>
        ///     Checks if the timing line is on-screen.
        /// </summary>
        /// <returns></returns>
        public bool IsOnScreen() => StartTime * Playfield.TrackSpeed >= Playfield.TrackPositionY - Playfield.Height &&
                                    StartTime * Playfield.TrackSpeed <= Playfield.TrackPositionY + Playfield.Height;

        public bool IsInPool { get; set; }
        
        /// <summary>
        ///     Returns the color of the tick
        /// </summary>
        /// <returns></returns>
        public abstract Color GetColor();

        /// <summary>
        ///     Returns the value/text of the tick (SV multiplier/BPM)
        /// </summary>
        /// <returns></returns>
        public abstract string GetValue();

        /// <summary>
        ///     Returns the time of the line
        /// </summary>
        /// <returns></returns>
        public virtual float StartTime
        {
            get => throw new NotImplementedException();
            set => throw new InvalidOperationException();
        }

        /// <summary>
        ///     Sets the size of the line
        /// </summary>
        public virtual void SetSize()
        {
        }
    }
}