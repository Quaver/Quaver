using System.Drawing;
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
using Wobble.Window;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI
{
    public class MapsetSearchBar : ImageButton
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
        ///     Creates the divider line
        /// </summary>
        private Sprite DividerLine { get; set; }

        /// <summary>
        ///     Displays the amount of mapsets available.
        /// </summary>
        private SpriteText SetsAvailableText { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="view"></param>
        public MapsetSearchBar(SelectScreen screen, SelectScreenView view) : base(UserInterface.BlankBox)
        {
            Screen = screen;
            ScreenView = view;

            Size = new ScalableVector2(585, 80);
            Alignment = Alignment.TopRight;
            Y = ScreenView.Toolbar.Y + ScreenView.Toolbar.Height + 1;
            X = 1;
            Tint = Color.Black;
            Alpha = 0.45f;

            CreateSearchBox();
            CreateSearchIcon();
            CreateDividerLine();
            CreateSetsAvailableText();
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

                    var sets = !string.IsNullOrEmpty(text) ? MapsetHelper.SearchMapsets(MapManager.Mapsets, text) : MapManager.Mapsets;

                    // Change SetsAvailableText
                    if (sets.Count == 0)
                    {
                        SetsAvailableText.TextColor = Color.White;
                        SetsAvailableText.Text = "No mapsets found :(";
                        ReadjustSetsAvailableTextPosition();

                        SetsAvailableText.Transformations.Clear();
                        SetsAvailableText.Transformations.Add(new Transformation(Easing.Linear, Color.White, Colors.Negative, 500));
                        return;
                    }

                    SetsAvailableText.TextColor = Color.White;
                    SetsAvailableText.Text = $"Found {sets.Count} mapsets.";
                    ReadjustSetsAvailableTextPosition();
                    SetsAvailableText.Transformations.Clear();
                    SetsAvailableText.Transformations.Add(new Transformation(Easing.Linear, Color.White, Colors.MainAccent, 500));


                    // Set the new available sets, and reinitialize the mapset buttons.
                    Screen.AvailableMapsets = sets;
                    ScreenView.MapsetContainer.InitializeMapsetButtons();

                    // Check to see if the current mapset is already in the new search.
                    var foundMapset = sets.FindIndex(x => x == MapManager.Selected.Value.Mapset);

                    // If the new map is in the search, go straight to it.
                    if (foundMapset != -1)
                    {
                        ScreenView.MapsetContainer.SelectMap(foundMapset, MapManager.Selected.Value, false, true);

                        // Make sure thumbnail is up to date.
                        // TODO: Make this code more DRY
                        var thumbnail = ScreenView.MapsetContainer.MapsetButtons[ScreenView.MapsetContainer.SelectedMapsetIndex].Thumbnail;

                        thumbnail.Image = BackgroundManager.Background.Sprite.Image;
                        thumbnail.Transformations.Clear();
                        var t = new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 250);
                        thumbnail.Transformations.Add(t);
                    }
                    // Select the first map in the first mapset, if it's a completely new mapset.
                    else if (MapManager.Selected.Value != Screen.AvailableMapsets.First().Maps.First())
                        ScreenView.MapsetContainer.SelectMap(0, Screen.AvailableMapsets.First().Maps.First(), true, true);
                    else
                    {
                        // TODO: Make this code more DRY
                        var thumbnail = ScreenView.MapsetContainer.MapsetButtons[ScreenView.MapsetContainer.SelectedMapsetIndex].Thumbnail;

                        thumbnail.Image = BackgroundManager.Background.Sprite.Image;
                        thumbnail.Transformations.Clear();
                        var t = new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 250);
                        thumbnail.Transformations.Add(t);
                    }
                })
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = 10,
                X = -20,
                Tint = Colors.DarkGray,
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
        ///     Creates the divider line sprite under the search box.
        /// </summary>
        private void CreateDividerLine()
        {
            DividerLine = new Sprite()
            {
                Parent = SearchBox,
                Size = new ScalableVector2(SearchBox.Width, 1),
                Tint = Color.White,
                Alignment = Alignment.TopCenter,
                Y = SearchBox.Height + SearchBox.Y + 2
            };
        }

        /// <summary>
        ///     Creates the sets available text.
        /// </summary>
        private void CreateSetsAvailableText()
        {
            SetsAvailableText = new SpriteText(Fonts.Exo2Regular24, $"Found {Screen.AvailableMapsets.Count} mapsets.")
            {
                Parent = DividerLine,
                TextColor = Color.White,
                TextScale = 0.45f,
                Transformations =
                {
                    new Transformation(Easing.Linear, Color.White, Colors.MainAccent, 500)
                }
            };

            ReadjustSetsAvailableTextPosition();
        }

        /// <summary>
        ///    Adjusts the sets available text
        /// </summary>
        private void ReadjustSetsAvailableTextPosition()
        {
            var size = SetsAvailableText.MeasureString() / 2f;

            SetsAvailableText.X = size.X;
            SetsAvailableText.Y = DividerLine.Height + 3 + size.Y;
        }
    }
}