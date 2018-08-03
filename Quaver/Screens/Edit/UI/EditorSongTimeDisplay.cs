using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Graphics;
using Wobble;

namespace Quaver.Screens.Edit.UI
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
