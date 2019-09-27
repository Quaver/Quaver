using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;

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

        private const string Delete = "Delete";

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
                            SelectMap(Mapset.Maps.First());

                        if (MapsetHelper.IsSingleDifficultySorted())
                            selectScreen?.ExitToGameplay();
                        else if (selectScreen != null)
                            selectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
                        break;
                    case Edit:
                        SelectMap(Mapset.Maps.First());
                        selectScreen?.ExitToEditor();
                        break;
                    case ViewOnlineListing:
                        MapManager.ViewOnlineListing(Mapset.Maps.First());
                        break;
                    case Delete:
                        break;
                    case AddToPlaylist:
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
        /// <param name="m"></param>
        private void SelectMap(Map m)
        {
            MapManager.Selected.Value = m;

            var container = (MapsetScrollContainer) DrawableMapset.Container;

            var index = container.AvailableItems.IndexOf(Mapset);

            if (index == -1)
                return;

            container.SelectedIndex.Value = index;
            container.ScrollToSelected();
        }
    }
}