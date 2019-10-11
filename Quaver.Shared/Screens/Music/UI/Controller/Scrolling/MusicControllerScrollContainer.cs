using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;

namespace Quaver.Shared.Screens.Music.UI.Controller.Scrolling
{
    public class MusicControllerScrollContainer : PoolableScrollContainer<Mapset>
    {
        /// <summary>
        /// </summary>
        public MusicControllerSongContainer SongContainer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="songContainer"></param>
        /// <param name="availableItems"></param>
        /// <param name="size"></param>
        public MusicControllerScrollContainer(MusicControllerSongContainer songContainer, List<Mapset> availableItems, ScalableVector2 size)
            : base(availableItems, 16, 0, size, size)
        {
            SongContainer = songContainer;
            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 10;

            InputEnabled = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            Alpha = 0;
            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Mapset> CreateObject(Mapset item, int index) => new MusicControllerSong(this, item, index);
    }
}