using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Downloading.UI.Mapsets
{
    public class DownloadableMapsetBanner : Sprite
    {
        /// <summary>
        /// </summary>
        private DownloadableMapset Mapset { get; set; }

        /// <summary>
        /// </summary>
        private int? LoadedMapsetId { get; set; }

        /// <summary>
        /// </summary>
        private int? RequestedMapsetId { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="mapset"></param>
        public DownloadableMapsetBanner(DownloadableMapset mapset, ScalableVector2 size)
        {
            Mapset = mapset;
            Size = size;
            Alpha = 0f;
            UsePreviousSpriteBatchOptions = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public void UpdateMapset(DownloadableMapset mapset)
        {
            if (Mapset?.Id == mapset.Id)
                return;

            Mapset = mapset;
            LoadedMapsetId = null;
            RequestedMapsetId = null;

            ClearAnimations();
            Alpha = 0;
            Image = null;
        }

        /// <summary>
        /// </summary>
        public void LoadIfNeeded()
        {
            if (Mapset == null || LoadedMapsetId == Mapset.Id || RequestedMapsetId == Mapset.Id)
                return;

            LoadBanner();
        }

        /// <summary>
        /// </summary>
        private void LoadBanner()
        {
            var mapsetId = Mapset.Id;

            RequestedMapsetId = mapsetId;

            ImageDownloader.DownloadMapsetBanner(mapsetId).ContinueWith(x =>
            {
                if (Mapset == null || Mapset.Id != mapsetId)
                    return;

                LoadedMapsetId = mapsetId;
                Image = x.Result;

                var alpha = Mapset.IsOwned ? 0.45f : 0.85f;
                FadeTo(alpha, Easing.Linear, 250);
            });
        }
    }
}
