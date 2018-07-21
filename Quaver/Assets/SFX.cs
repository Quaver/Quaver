using System.IO;
using Microsoft.Xna.Framework.Audio;
using Quaver.Helpers;

namespace Quaver.Assets
{
    /// <summary>
    ///     All sound effects that aren't skinnable.
    /// </summary>
    internal static class SFX
    {
        internal static SoundEffect Woosh { get; private set; }

        /// <summary>
        ///     Loads all the sound effects.
        /// </summary>
        internal static void Load()
        {
            Woosh = SoundEffect.FromStream((UnmanagedMemoryStream)ResourceHelper.GetProperty("sound-woosh"));
        }
    }
}