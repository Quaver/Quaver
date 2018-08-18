using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Screens.Menu;
using Quaver.Screens.Menu.UI.BottomToolbar;
using Quaver.Screens.Select.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;

namespace Quaver.Screens.Select
{
    public class SelectScreenView : ScreenView
    {
        /// <summary>
        ///     The top toolbar for this screen.
        /// </summary>
        private Toolbar Toolbar { get; set;  }

        private BottomBar BottomBar { get; set; }

        /// <summary>
        ///     The scroll container for the mapsets.
        /// </summary>
        private MapsetContainer MapsetContainer { get; set; }

        /// <summary>
        ///     Searches for mapsets.
        /// </summary>
        private Textbox MapsetSearchBar { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectScreenView(Screen screen) : base(screen)
        {
            BackgroundManager.Background.Dim = 0;
            BackgroundManager.Background.Strength = 8;

            Toolbar = new Toolbar(new List<ToolbarItem>
            {
                new ToolbarItem("Home", () => ScreenManager.ChangeScreen(new MainMenuScreen()))
            }, new List<ToolbarItem>())
            {
                Parent = Container
            };

            BottomBar = new BottomBar() {Parent = Container};
            MapsetContainer = new MapsetContainer((SelectScreen) Screen, this)
            {
                Parent = Container,
                X = 200,
                Transformations =
                {
                    new Transformation(TransformationProperty.X, Easing.EaseOutBounce, 200, 0, 1200)
                }
            };

            MapsetSearchBar = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(500, 30),
                Fonts.Exo2Regular24, SelectScreen.PreviousSearchTerm, "Type to search", 0.60f, null,
                text =>
                {
                    // Update previous search term
                    SelectScreen.PreviousSearchTerm = text;

                    var selectScreen = (SelectScreen) Screen;

                    var sets = !string.IsNullOrEmpty(text) ? MapsetHelper.SearchMapsets(MapManager.Mapsets, text) : MapManager.Mapsets;

                    if (sets.Count <= 0)
                        return;

                    // Set the new available sets, and reinitialize the mapset buttons.
                    selectScreen.AvailableMapsets = sets;
                    MapsetContainer.InitializeMapsetButtons();

                    // Check to see if the current mapset is already in the new search.
                    var foundMapset = sets.FindIndex(x => x == MapManager.Selected.Value.Mapset);

                    // If the new map is in the search, go straight to it.
                    if (foundMapset != -1)
                    {
                        MapsetContainer.SelectMap(foundMapset, MapManager.Selected.Value, false, true);

                        // Make sure thumbnail is up to date.
                        // TODO: Make this code more DRY
                        var thumbnail = MapsetContainer.MapsetButtons[MapsetContainer.SelectedMapsetIndex].Thumbnail;

                        thumbnail.Image = BackgroundManager.Background.Sprite.Image;
                        thumbnail.Transformations.Clear();
                        var t = new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 250);
                        thumbnail.Transformations.Add(t);
                    }
                    // Select the first map in the first mapset, if it's a completely new mapset.
                    else if (MapManager.Selected.Value != selectScreen.AvailableMapsets.First().Maps.First())
                        MapsetContainer.SelectMap(0, selectScreen.AvailableMapsets.First().Maps.First(), true, true);
                    else
                    {
                        // Make sure thumbnail is up to date.
                        // TODO: Make this code more DRY!
                        var thumbnail = MapsetContainer.MapsetButtons[MapsetContainer.SelectedMapsetIndex].Thumbnail;

                        thumbnail.Image = BackgroundManager.Background.Sprite.Image;
                        thumbnail.Transformations.Clear();
                        var t = new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 250);
                        thumbnail.Transformations.Add(t);
                    }
                })
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Y = 10,
                X = 10,
                Tint = Color.Black,
                AlwaysFocused = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            BackgroundManager.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}