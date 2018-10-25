using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
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
            Y = ScreenView.Toolbar.Y + ScreenView.Toolbar.Height;
            X = 1;
            Alpha = 0.90f;
            Image = UserInterface.SelectSearchBackground;

            CreateSearchBox();
            CreateSearchIcon();
            CreateSetsAvailableText();
            Orderer = new MapsetSearchOrderer(this) { Parent = this };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SearchBox.AlwaysFocused = DialogManager.Dialogs.Count == 0;
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the box to search for mapsets.
        /// </summary>
        private void CreateSearchBox() =>
            // ReSharper disable once ArrangeMethodOrOperatorBody
            SearchBox = new Textbox(new ScalableVector2(550, 30), BitmapFonts.Exo2Regular, 16,
                SelectScreen.PreviousSearchTerm, "Start typing to search...", null, text =>
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

        /// <summary>
        ///     Creates the search icon at the right of the search box.
        /// </summary>
        private void CreateSearchIcon() =>
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

        /// <summary>
        ///     Creates the sets available text.
        /// </summary>
        private void CreateSetsAvailableText()
        {
            SetsAvailableText = new SpriteText(BitmapFonts.Exo2Regular, $"Found {Screen.AvailableMapsets.Count} mapsets.", 16)
            {
                Parent = SearchBox,
                Tint = Color.White,
                Animations =
                {
                    new Animation(Easing.Linear, Color.White, Colors.MainAccent, 500)
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
            SetsAvailableText.Tint = Color.White;

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
            SetsAvailableText.Y = SearchBox.Height + 5;

            // Fade to the new color.
            SetsAvailableText.Animations.Clear();
            SetsAvailableText.Animations.Add(new Animation(Easing.Linear, Color.White, newColor, 500));
        }
    }
}