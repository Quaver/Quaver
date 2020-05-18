using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Wobble.Bindables;
using Wobble.Graphics.UI;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class JukeboxProgressBar : ProgressBar
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="defaultValue"></param>
        /// <param name="inactiveColor"></param>
        /// <param name="activeColor"></param>
        public JukeboxProgressBar(Vector2 size, double minValue, double maxValue, double defaultValue, 
            Color inactiveColor, Color activeColor) : base(size, minValue, maxValue, defaultValue, inactiveColor, activeColor)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
            {
                Bindable.MaxValue = AudioEngine.Track.Length;
                Bindable.Value = AudioEngine.Track.Position;
            }

            base.Update(gameTime);
        }
    }
}