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

        private static string SelectText => DownloadLocalization.Get(Select);

        private static string DownloadText => DownloadLocalization.Get(Download);

        private static string OnlineListingText => DownloadLocalization.Get(OnlineListing);

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
                    case var text when text == SelectText:
                        SelectedMapset.Value = Mapset;
                        break;
                    case var text when text == DownloadText:
                        if (MapsetDownloadManager.IsMapsetInQueue(Mapset.Id))
                        {
                            NotificationManager.Show(NotificationLevel.Warning, DownloadLocalization.Get("This mapset is already downloading!"));
                            return;
                        }

                        MapsetDownloadManager.Download(Mapset.Id, Mapset.Artist, Mapset.Title);
                        break;
                    case var text when text == OnlineListingText:
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
                options.Add(SelectText, Color.White);

            options.Add(DownloadText, ColorHelper.HexToColor("#27B06E"));
            options.Add(OnlineListingText, ColorHelper.HexToColor("#FFE76B"));

            return options;
        }
    }
}
