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
        /// <param name="size"></param>
        /// <param name="mapset"></param>
        public DownloadableMapsetBanner(DownloadableMapset mapset, ScalableVector2 size)
        {
            Mapset = mapset;
            Size = size;
            Alpha = 0f;
            UsePreviousSpriteBatchOptions = true;

            LoadBanner();
        }

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public void UpdateMapset(DownloadableMapset mapset)
        {
            Mapset = mapset;
            LoadBanner();
        }

        /// <summary>
        /// </summary>
        private void LoadBanner()
        {
            var mapset = Mapset;

             ClearAnimations();
             Alpha = 0;

            ImageDownloader.DownloadMapsetBanner(Mapset.Id).ContinueWith(x =>
            {
                if (Mapset != mapset)
                    return;

                Image = x.Result;

                var alpha = Mapset.IsOwned ? 0.45f : 0.85f;
                FadeTo(alpha, Easing.Linear, 250);
            });
        }
    }
}