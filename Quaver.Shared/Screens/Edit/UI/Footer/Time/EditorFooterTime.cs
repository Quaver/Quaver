using System;
using Microsoft.Xna.Framework;
using Wobble.Audio.Tracks;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.Edit.UI.Footer.Time
{
    public class EditorFooterTime : SpriteTextPlus
    {
        /// <summary>
        /// </summary>
        private EditorFooterTimeType Type { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="font"></param>
        /// <param name="track"></param>
        public EditorFooterTime(EditorFooterTimeType type, WobbleFontStore font, IAudioTrack track) : base(font, "00:00.000", 24, false)
        {
            Type = type;
            Track = track;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Optimize this, so it's not creating a new rendertarget each frame
            switch (Type)
            {
                case EditorFooterTimeType.Current:
                    // @"mm\:ss\.fff")
                    Text = TimeSpan.FromMilliseconds(Math.Round(Track.Time)).ToString(@"mm\:ss\.fff");
                    break;
                case EditorFooterTimeType.Left:
                    Text = "-" + TimeSpan.FromMilliseconds(Track.Length - Math.Round(Track.Time)).ToString(@"mm\:ss\.fff");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            base.Update(gameTime);
        }
    }

    public enum EditorFooterTimeType
    {
        Current,
        Left
    }
}
