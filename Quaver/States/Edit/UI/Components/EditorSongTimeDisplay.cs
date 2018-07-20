using System;
using Microsoft.Xna.Framework;
using Quaver.Graphics.UI;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Components
{
    internal class EditorSongTimeDisplay : NumberDisplay
    {
        internal EditorSongTimeDisplay(NumberDisplayType type, string startingValue, Vector2 imageScale) : base(type, startingValue, imageScale) => PosX = -TotalWidth;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds((int) GameBase.AudioEngine.Time);
            Value = currTime.ToString("mm:ss.fff");

            base.Update(dt);
        }
    }
}