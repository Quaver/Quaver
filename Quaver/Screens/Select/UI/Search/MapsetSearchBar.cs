using System.Collections.Generic;
using System.Linq;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.Search
{
    public class MapsetSearchBar : Sprite
    {
        /// <summary>
        ///     Reference to the select screen.
        /// </summary>
        public SelectScreen Screen { get; }

        /// <summary>
        ///     Reference to the select ScreenView.
        /// </summary>
        public SelectScreenView ScreenView { get; }

        /// <summary>
        ///     The actual textbox to start searching in.
        /// </summary>
        public Textbox SearchBox { get; private set; }

        /// <summary>
        ///     The little search icon to the right of the text box.
        /// </summary>
        private Sprite SearchIcon { get; set; }

        /// <summary>
        ///     Displays the amount of mapsets available.
        /// </summary>
        private SpriteText SetsAvailableText { get; set; }

        /// <summary>
        ///     The interface to order the mapsets.
        /// </summary>
        private MapsetSearchOrderer Orderer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="view"></param>
        public MapsetSearchBar(SelectScreen screen, SelectScreenView view)
        {
            Screen = screen;
            ScreenView = view;

            Size = new ScalableVector2(585, 70);
            Alignment = Alignment.TopRight;
            Y = ScreenView.Toolbar.Y + ScreenView.Toolbar.Height + 1;
            X = 1;
            Tint = Color.Black;
            Alpha = 0.45f;

            CreateSearchBox();
            CreateSearchIcon();
            CreateSetsAvailableText();
            Orderer = new MapsetSearchOrderer(this) { Parent = this };
        }

        /// <summary>
        ///     Creates the box to search for mapsets.
        /// </summary>
        private void CreateSearchBox()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            SearchBox = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(550, 30), Fonts.Exo2Regular24,
                SelectScreen.PreviousSearchTerm, "Start typing to search...", 0.60f, null, text =>
                {
                    // Update previous search term
                    SelectScreen.PreviousSearchTerm = text;

                    // Search for new mapsets.
                    var sets = !string.IsNullOrEmpty(text) ? MapsetHelper.SearchMapsets(MapManager.Mapsets, text) : MapManager.Mapsets;
                    sets = MapsetHelper.OrderMapsetByConfigValue(sets);

                    ReadjustSetsAvailableText(sets);
                    ScreenView.MapsetContainer.ReInitializeMapsetButtonsWithNewSets(sets);
                })
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = 10,
                X = -20,
                Image = UserInterface.SearchBar,
                AlwaysFocused = true,
                StoppedTypingActionCalltime = 300
            };
        }

        /// <summary>
        ///     Creates the search icon at the right of the search box.
        /// </summary>
        private void CreateSearchIcon()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            SearchIcon = new Sprite()
            {
                Parent = SearchBox,
                Alignment = Alignment.MidRight,
                Image = FontAwesome.Search,
                Size = new ScalableVector2(SearchBox.Height * 0.50f, SearchBox.Height * 0.50f),
                Tint = Color.White,
                X = -10
            };
        }

        /// <summary>
        ///     Creates the sets available text.
        /// </summary>
        private void CreateSetsAvailableText()
        {
            SetsAvailableText = new SpriteText(Fonts.Exo2Regular24, $"Found {Screen.AvailableMapsets.Count} mapsets.")
            {
                Parent = SearchBox,
                TextColor = Color.White,
                TextScale = 0.45f,
                Transformations =
                {
                    new Transformation(Easing.Linear, Color.White, Colors.MainAccent, 500)
                }
            };

            ReadjustSetsAvailableText(Screen.AvailableMapsets);
        }

        /// <summary>
        ///    Adjusts the sets available text
        /// </summary>
        private void ReadjustSetsAvailableText(IReadOnlyCollection<Mapset> sets)
        {
            // Initially change color to white.
            SetsAvailableText.TextColor = Color.White;

            Color newColor;

            if (sets.Count > 0)
            {
                SetsAvailableText.Text = $"Found {sets.Count} mapsets";
                newColor = Colors.MainAccent;
            }
            else
            {
                SetsAvailableText.Text = "No mapsets found :(";
                newColor = Colors.Negative;
            }

            // Readjust position.
            var size = SetsAvailableText.MeasureString() / 2f;
            SetsAvailableText.X = size.X;
            SetsAvailableText.Y = SearchBox.Height + 5 + size.Y;

            // Fade to the new color.
            SetsAvailableText.Transformations.Clear();
            SetsAvailableText.Transformations.Add(new Transformation(Easing.Linear, Color.White, newColor, 500));
        }
    }
}