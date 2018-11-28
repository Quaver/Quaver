using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.UI
{
    internal class EditorSongTimeDisplay : NumberDisplay
    {
        public EditorSongTimeDisplay(NumberDisplayType type, string startingValue, Vector2 imageScale)
                                                    : base(type, startingValue, imageScale) => X = -TotalWidth;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds((int) AudioEngine.Track.Time);
            Value = currTime.ToString("mm:ss.fff");

            base.Update(gameTime);
        }
    }
}
