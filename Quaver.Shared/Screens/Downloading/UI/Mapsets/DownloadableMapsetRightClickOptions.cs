using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Download;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Mapsets
{
    public class DownloadableMapsetRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private DownloadableMapset Mapset { get; }

        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        private const string Select = "Select";

        private const string Download = "Download";

        private const string OnlineListing = "Online Listing";

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        /// <param name="selectedMapset"></param>
        public DownloadableMapsetRightClickOptions(DownloadableMapset mapset, Bindable<DownloadableMapset> selectedMapset)
            : base(GetOptions(mapset, selectedMapset), new ScalableVector2(200, 40), 22)
        {
            Mapset = mapset;
            SelectedMapset = selectedMapset;

            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case Select:
                        SelectedMapset.Value = Mapset;
                        break;
                    case Download:
                        if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == Mapset.Id))
                        {
                            NotificationManager.Show(NotificationLevel.Warning, $"This mapset is already downloading!");
                            return;
                        }

                        MapsetDownloadManager.Download(Mapset.Id, Mapset.Artist, Mapset.Title);
                        break;
                    case OnlineListing:
                        BrowserHelper.OpenURL($"https://quavergame.com/mapsets/{Mapset.Id}");
                        break;
                }
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        /// <param name="selectedMapset"></param>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions(DownloadableMapset mapset, Bindable<DownloadableMapset> selectedMapset)
        {
            var options = new Dictionary<string, Color>();

            if (selectedMapset.Value != mapset)
                options.Add(Select, Color.White);

            options.Add(Download, ColorHelper.HexToColor("#27B06E"));
            options.Add(OnlineListing, ColorHelper.HexToColor("#FFE76B"));

            return options;
        }
    }
}