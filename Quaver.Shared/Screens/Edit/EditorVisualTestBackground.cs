using System;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Shared.Screens.Edit
{
    public class EditorVisualTestBackground : IDisposable
    {
        /// <summary>
        /// </summary>
        public Texture2D Texture { get; }

        /// <summary>
        /// </summary>
        public bool ShouldDispose { get; }

        /// <summary>
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="shouldDispose"></param>
        public EditorVisualTestBackground(Texture2D tex, bool shouldDispose = true)
        {
            Texture = tex;
            ShouldDispose = shouldDispose;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            if (!ShouldDispose)
                return;
            Texture?.Dispose();
        }
    }
}