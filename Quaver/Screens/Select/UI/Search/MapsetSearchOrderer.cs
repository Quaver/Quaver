using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;

namespace Quaver.Screens.Select.UI.Search
{
    public class MapsetSearchOrderer : Sprite
    {
        /// <summary>
        ///     Reference to the parent mapset search bar.
        /// </summary>
        private MapsetSearchBar SearchBar { get; set; }

        /// <summary>
        ///     Text that displays "Order By"
        /// </summary>
        private SpriteText TextOrderBy { get; set; }

        /// <summary>
        ///     The buttons to change the ordering filter.
        /// </summary>
        private Dictionary<OrderMapsetsBy, TextButton> FilterButtons { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="searchBar"></param>
        public MapsetSearchOrderer(MapsetSearchBar searchBar)
        {
            SearchBar = searchBar;

            Alignment = Alignment.TopLeft;
            Y = SearchBar.SearchBox.Height + 5;

            CreateFilterButtons();
            CreateOrderByText();
        }

        /// <summary>
        ///     Creates the text that states order by.
        /// </summary>
        private void CreateOrderByText()
        {

        }

        private void CreateFilterButtons()
        {
            FilterButtons = new Dictionary<OrderMapsetsBy, TextButton>();

            var enumValues = Enum.GetNames(typeof(OrderMapsetsBy)).ToList();

            // Start from the last value and loop backwards, as we're initializing the text from last to first.
            for (var i = enumValues.Count - 1; i >= 0; i--)
            {
                var orderOption = enumValues[i];

                var button = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, orderOption, 0.45f)
                {
                    Parent = this,
                    Alpha = 0,
                    Text =
                    {
                        TextColor = (OrderMapsetsBy) i == ConfigManager.SelectOrderMapsetsBy.Value ? Color.White : ColorHelper.HexToColor("#75e475")
                    }
                };

                var textSize = button.Text.MeasureString();
                button.Size = new ScalableVector2(textSize.X, textSize.Y);

                button.X = SearchBar.SearchBox.X + SearchBar.SearchBox.Width - textSize.X / 2f;
                button.Y += textSize.Y / 2f;

                // When the button is clicked
                var option = (OrderMapsetsBy) i;
                button.Clicked += (o, e) => OnFilterButtonClicked(button, option);

                FilterButtons.Add((OrderMapsetsBy) i, button);

                if (i == enumValues.Count - 1)
                    continue;

                var previousButton = FilterButtons[(OrderMapsetsBy) (i + 1)];

                button.X -= previousButton.Width + (5 * enumValues.Count - i) + (button.X - previousButton.X);

                // Aligning text is so annoying.. but it works.
                if (i == enumValues.Count - 2)
                    button.X += 25;
            }
        }

        /// <summary>
        ///     When
        /// </summary>
        /// <param name="button"></param>
        /// <param name="orderMapsetsBy"></param>
        private void OnFilterButtonClicked(TextButton button, OrderMapsetsBy orderMapsetsBy)
        {
            // Change the color of the button.
            button.Text.Transformations.Clear();
            button.Text.Transformations.Add(new Transformation(Easing.Linear, button.Text.TextColor, Color.White, 200));

            // Change the color of the other buttons.
            foreach (var item in FilterButtons)
            {
                if (item.Key == orderMapsetsBy)
                    continue;

                item.Value.Text.Transformations.Clear();
                item.Value.Text.Transformations.Add(new Transformation(Easing.Linear, button.Text.TextColor,
                                                        ColorHelper.HexToColor("#75e475"), 200));
            }

            ConfigManager.SelectOrderMapsetsBy.Value = orderMapsetsBy;
            var orderedSets = MapsetHelper.OrderMapsetByConfigValue(SearchBar.Screen.AvailableMapsets);
            SearchBar.ScreenView.MapsetContainer.ReInitializeMapsetButtonsWithNewSets(orderedSets);
        }
    }
}