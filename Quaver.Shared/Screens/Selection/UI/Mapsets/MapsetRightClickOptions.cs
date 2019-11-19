using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection.UI.Playlists.Management;
using Quaver.Shared.Screens.Selection.UI.Playlists.Management.Maps;
using Quaver.Shared.Screens.Selection.UI.Playlists.Management.Mapsets;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class MapsetRightClickOptions : RightClickOptions
    {
        private DrawableMapset DrawableMapset { get; }

        private Mapset Mapset { get; }

        private const string Play = "Play";

        private const string Edit = "Edit";

        private const string ViewOnlineListing = "Online Listing";

        private const string AddToPlaylist = "Add To Playlist";

        private const string Delete = "Delete Mapset";

        private const string Export = "Export";

        private const string OpenMapsetFolder = "Open Folder";

        /// <summary>
        /// </summary>
        public MapsetRightClickOptions(DrawableMapset drawableMapset) : base(new Dictionary<string, Color>()
        {
            {Play, Color.White},
            {Edit, ColorHelper.HexToColor("#F2994A")},
            {AddToPlaylist, ColorHelper.HexToColor("#27B06E")},
            {Delete, ColorHelper.HexToColor($"#FF6868")},
            {Export, ColorHelper.HexToColor("#0787E3")},
            {OpenMapsetFolder, ColorHelper.HexToColor("#9B51E0")},
            {ViewOnlineListing, ColorHelper.HexToColor("#FFE76B")},
        }, new ScalableVector2(200, 40), 22)
        {
            DrawableMapset = drawableMapset;
            Mapset = DrawableMapset.Item;

            ItemSelected += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                var selectScreen = game.CurrentScreen as SelectionScreen;

                switch (args.Text)
                {
                    case Play:
                        if (!Mapset.Maps.Contains(MapManager.Selected.Value))
                            SelectMap(DrawableMapset, Mapset.Maps.First(), Mapset);

                        if (MapsetHelper.IsSingleDifficultySorted())
                            selectScreen?.ExitToGameplay();
                        else if (selectScreen != null)
                            selectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
                        break;
                    case Edit:
                        SelectMap(DrawableMapset, Mapset.Maps.First(), Mapset);
                        selectScreen?.ExitToEditor();
                        break;
                    case ViewOnlineListing:
                        MapManager.ViewOnlineListing(Mapset.Maps.First());
                        break;
                    case Delete:
                        if (selectScreen == null)
                            return;

                        // Using First().Mapset to get the *real* and unfiltered mapset
                        DialogManager.Show(new DeleteMapsetDialog(Mapset.Maps.First().Mapset, selectScreen.AvailableMapsets.Value.IndexOf(Mapset)));
                        break;
                    case AddToPlaylist:
                        if (MapsetHelper.IsSingleDifficultySorted())
                            selectScreen?.ActivateCheckboxContainer(new AddMapToPlaylistCheckboxContainer(Mapset.Maps.First()));
                        else
                            selectScreen?.ActivateCheckboxContainer(new AddMapsetToPlaylistCheckboxContainer(Mapset.Maps.First().Mapset));
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
                    case OpenMapsetFolder:
                        Mapset.Maps.First().OpenFolder();
                        break;
                }
            };
        }

        /// <summary>
        ///     Selects a map and sets the appropriate index
        /// </summary>
        /// <param name="drawableMapset"></param>
        /// <param name="map"></param>
        /// <param name="mapset"></param>
        public static void SelectMap(DrawableMapset drawableMapset, Map map, Mapset mapset)
        {
            MapManager.Selected.Value = map;

            var container = (MapsetScrollContainer) drawableMapset.Container;

            var index = container.AvailableItems.IndexOf(mapset);

            if (index == -1)
                return;

            container.SelectedIndex.Value = index;
            container.ScrollToSelected();
        }
    }
}