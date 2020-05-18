using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Music.UI.Controller.Scrolling
{
    public class MusicPlayerSongRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private Mapset Mapset { get; }

        private const string Play = "Play";

        private const string Export = "Export";

        private const string OnlineListing = "Online Listing";

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public MusicPlayerSongRightClickOptions(Mapset mapset)
            : base(new Dictionary<string, Color>()
            {
                {Play, Color.White},
                {Export, ColorHelper.HexToColor("#0787E3")},
                {OnlineListing, ColorHelper.HexToColor("#FFE76B")},
            }, new ScalableVector2(200, 40), 22)
        {
            Mapset = mapset;

            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case Play:
                        if (!OnlineManager.IsListeningPartyHost)
                        {
                            NotificationManager.Show(NotificationLevel.Error, "You are not the host of listening party!");
                            return;
                        }
                        
                        MapManager.Selected.Value = Mapset.Maps.First();
                        break;
                    case Export:
                        ThreadScheduler.Run(() =>
                        {
                            NotificationManager.Show(NotificationLevel.Info, "Exporting mapset to zip archive. Please wait!");

                            Mapset.Maps.First().Mapset.ExportToZip();

                            NotificationManager.Show(NotificationLevel.Success,
                                $"Successfully exported {MapManager.Selected.Value.Mapset.Artist} - {MapManager.Selected.Value.Mapset.Title}!");
                        });
                        break;
                    case OnlineListing:
                        MapManager.ViewOnlineListing(Mapset.Maps.First());
                        break;
                }
            };
        }
    }
}