using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create;
using Quaver.Shared.Screens.Selection.UI.Playlists.Management.Maps;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Maps
{
    public class MapRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private DrawableMap DrawableMap { get; }

        /// <summary>
        ///     If <see cref="MapsetHelper.IsSingleDifficultySorted()"/> is true,
        ///     we'll need the DrawableMapset and use it as if it were a map.
        /// </summary>
        private DrawableMapset DrawableMapset { get; }

        /// <summary>
        ///     <see cref="DrawableMapset"/>
        /// </summary>
        private Mapset Mapset { get; }

        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private static ScalableVector2 OptionsSize { get; } = new ScalableVector2(200, 40);

        /// <summary>
        /// </summary>
        private const int FontSize = 22;

        private const string Play = "Play";

        private const string Edit = "Edit";

        private const string EditorV2 = "Editor v2";

        private const string ViewOnlineListing = "Online Listing";

        private const string AddToPlaylist = "Add To Playlist";

        private const string Delete = "Delete Map";

        private const string Export = "Export Mapset";

        private const string OpenMapsetFolder = "Open Folder";

        private const string DeleteLocalScores = "Delete Local Scores";

        private const string DeleteMapset = "Delete Mapset";
        /// <summary>
        /// </summary>
        public MapRightClickOptions(DrawableMap drawableMap) : base(GetOptions(), OptionsSize, FontSize)
        {
            DrawableMap = drawableMap;
            Map = DrawableMap.Item;

            SubscribeToItemSelected();
        }

        /// <summary>
        /// </summary>
        /// <param name="drawableMapset"></param>
        public MapRightClickOptions(DrawableMapset drawableMapset) : base(GetOptions(), OptionsSize, FontSize)
        {
            DrawableMapset = drawableMapset;
            Map = DrawableMapset.Item.Maps.First();
            Mapset = DrawableMapset.Item;
            SubscribeToItemSelected();
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

        /// <summary>
        /// </summary>
        private void SubscribeToItemSelected()
        {
            ItemSelected += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                var selectScreen = game.CurrentScreen as SelectionScreen;

                switch (args.Text)
                {
                    case Play:
                        if (MapsetHelper.IsSingleDifficultySorted())
                            MapsetRightClickOptions.SelectMap(DrawableMapset, Map, Mapset);
                        else
                            SelectMap(Map);

                        selectScreen?.ExitToGameplay();
                        break;
                    case Edit:
                        if (MapsetHelper.IsSingleDifficultySorted())
                            MapsetRightClickOptions.SelectMap(DrawableMapset, Map, Mapset);
                        else
                            SelectMap(Map);

                        selectScreen?.ExitToEditor();
                        break;
                    case EditorV2:
                        if (MapsetHelper.IsSingleDifficultySorted())
                            MapsetRightClickOptions.SelectMap(DrawableMapset, Map, Mapset);
                        else
                            SelectMap(Map);

                        selectScreen?.ExitToEditor(true);
                        break;
                    case ViewOnlineListing:
                        MapManager.ViewOnlineListing(Map);
                        break;
                    case Delete:
                        if (selectScreen == null)
                            return;

                        DialogManager.Show(new DeleteMapDialog(Map, Map.Mapset.Maps.IndexOf(Map)));
                        break;
                    case DeleteMapset:
                        if (selectScreen == null)
                            return;

                        DialogManager.Show(new DeleteMapsetDialog(Map.Mapset, selectScreen.AvailableMapsets.Value.IndexOf(Map.Mapset)));
                        break;
                    case DeleteLocalScores:
                        DialogManager.Show(new DeleteLocalScoresDialog(Map));
                        break;
                    case AddToPlaylist:
                        if (PlaylistManager.Playlists.FindAll(x => x.PlaylistGame == MapGame.Quaver).Count == 0)
                        {
                            DialogManager.Show(new CreatePlaylistDialog());
                            return;
                        }

                        selectScreen?.ActivateCheckboxContainer(new AddMapToPlaylistCheckboxContainer(Map));
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
        ///     Returns the options to select
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions() => new Dictionary<string, Color>()
        {
            {Play, Color.White},
            {Edit, ColorHelper.HexToColor("#F2994A")},
            {EditorV2, ColorHelper.HexToColor("#F2994A")},
            {AddToPlaylist, ColorHelper.HexToColor("#27B06E")},
            {Delete, ColorHelper.HexToColor($"#FF6868")},
            {DeleteMapset, ColorHelper.HexToColor($"#FF6868")},
            {DeleteLocalScores, ColorHelper.HexToColor($"#FF6868")},
            {Export, ColorHelper.HexToColor("#0787E3")},
            {OpenMapsetFolder, ColorHelper.HexToColor("#9B51E0")},
            {ViewOnlineListing, ColorHelper.HexToColor("#FFE76B")},
        };
    }
}