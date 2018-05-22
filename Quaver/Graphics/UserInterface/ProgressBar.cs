using System;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Base;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;

namespace Quaver.Graphics.UserInterface
{
    /// <inheritdoc />
    /// <summary>
    ///     General purpose progress bar.
    ///     TODO: Make this with BindedFloat class instead for auto-updates.
    /// </summary>
    internal class ProgressBar : QuaverSprite
    {
        /// <summary>
        ///     The  maximum value of the progress bar.
        /// </summary>
        internal double MaxValue { get; }

        /// <summary>
        ///     The current value of the progress bar.
        /// </summary>
        internal double CurrentValue { get; set; }

        /// <summary>
        ///     The active color of the progress bar
        ///     
        /// </summary>
        internal Color ActiveColor { get; set; } = QuaverColors.MainAccent;

        /// <summary>
        ///     The inactive color of the progress bar.
        /// </summary>
        internal Color InactiveColor { get; set; } = QuaverColors.MainAccentInactive;

        /// <summary>
        ///     The current active progress bar.
        /// </summary>
        internal QuaverSprite ActiveProgressBar { get; }

        /// <summary>
        ///     The current percentage of the progress bar.
        /// </summary>
        private double Percentage => (CurrentValue - 0) * 100 / MaxValue - 0 * 100;

        /// <summary>
        ///     The X size of the progress bar.
        /// </summary>
        private float BarSizeX { get; }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="initialValue"></param>
        /// <param name="size"></param>
        /// <param name="parent"></param>
        /// <param name="alignment"></param>
        internal ProgressBar(float maxValue, float initialValue, Vector2 size, Drawable parent, Alignment alignment)
        {
            MaxValue = maxValue;
            CurrentValue = initialValue;
            
            // Set the initial color of it.
            Tint = InactiveColor;
            Size = new UDim2D(size.X, size.Y);
            Alignment = alignment;
            Parent = parent;

            // Create the active progress bar.
            ActiveProgressBar = new QuaverSprite
            {
                Tint = ActiveColor,
                Alignment = alignment,
                Parent = this,
                Size = new UDim2D(0, size.Y)
            };
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Update the active progress.
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            ActiveProgressBar.SizeX = GraphicsHelper.Tween((float)(SizeX * (Percentage / 100f)), ActiveProgressBar.SizeX, Math.Min(dt / 240, 1));
            base.Update(dt);
        }
    }
}