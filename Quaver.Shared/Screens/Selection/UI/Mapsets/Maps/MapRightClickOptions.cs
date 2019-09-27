using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps
{
    public class MapRightClickOptions : RightClickOptions
    {
        private DrawableMap DrawableMap { get; }

        private Map Map { get; }

        private const string Play = "Play";

        private const string Edit = "Edit";

        private const string ViewOnlineListing = "Online Listing";

        private const string AddToPlaylist = "Add To Playlist";

        private const string Delete = "Delete Map";

        private const string Export = "Export";

        private const string OpenMapsetFolder = "Open Folder";

        private const string DeleteLocalScores = "Delete Local Scores";

        /// <summary>
        /// </summary>
        public MapRightClickOptions(DrawableMap drawableMap) : base(new Dictionary<string, Color>()
        {
            {Play, Color.White},
            {Edit, ColorHelper.HexToColor("#F2994A")},
            {AddToPlaylist, ColorHelper.HexToColor("#27B06E")},
            {Delete, ColorHelper.HexToColor($"#FF6868")},
            {DeleteLocalScores, ColorHelper.HexToColor($"#FF6868")},
            {Export, ColorHelper.HexToColor("#0787E3")},
            {OpenMapsetFolder, ColorHelper.HexToColor("#9B51E0")},
            {ViewOnlineListing, ColorHelper.HexToColor("#FFE76B")},
        }, new ScalableVector2(200, 40), 22)
        {
            DrawableMap = drawableMap;
            Map = DrawableMap.Item;

            ItemSelected += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                var selectScreen = game.CurrentScreen as SelectionScreen;

                switch (args.Text)
                {
                    case Play:
                        SelectMap(Map);
                        selectScreen?.ExitToGameplay();
                        break;
                    case Edit:
                        SelectMap(Map);
                        selectScreen?.ExitToEditor();
                        break;
                    case ViewOnlineListing:
                        MapManager.ViewOnlineListing(Map);
                        break;
                    case Delete:
                        if (selectScreen == null)
                            return;

                        DialogManager.Show(new DeleteMapDialog(Map, Map.Mapset.Maps.IndexOf(Map)));
                        break;
                    case DeleteLocalScores:
                        DialogManager.Show(new DeleteLocalScoresDialog(Map));
                        break;
                    case AddToPlaylist:
                        break;
                    case Export:
                        ThreadScheduler.Run(() =>
                        {
                            NotificationManager.Show(NotificationLevel.Info, "Exporting mapset to zip archive. Please wait!");

                            Map.Mapset.ExportToZip();

                            NotificationManager.Show(NotificationLevel.Success,$"Successfully exported {Map.Artist} - {Map.Title}!");
                        });
                        break;
                    case OpenMapsetFolder:
                        Map.OpenFolder();
                        break;
                }
            };
        }

        /// <summary>
        ///     Selects a map and sets the appropriate index
        /// </summary>
        /// <param name="m"></param>
        private void SelectMap(Map m)
        {
            MapManager.Selected.Value = m;

            var container = (MapScrollContainer) DrawableMap.Container;

            var index = container.AvailableItems.IndexOf(Map);

            if (index == -1)
                return;

            container.SelectedIndex.Value = index;
            container.ScrollToSelected();
        }
    }
}