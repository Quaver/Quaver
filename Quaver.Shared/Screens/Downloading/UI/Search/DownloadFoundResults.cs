using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online.API.MapsetSearch;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadFoundResults : Container
    {
        /// <summary>
        /// </summary>
        private BindableList<DownloadableMapset> Mapsets { get; }

        /// <summary>
        ///    The amount of maps there are
        /// </summary>
        public SpriteTextPlus TextCount { get; private set; }

        /// <summary>
        ///     The text that displays "Maps Found"
        /// </summary>
        public SpriteTextPlus TextMapsFound { get; private set; }

        /// <summary>
        ///     The amount of space between <see cref="TextCount"/> and <see cref="TextSpacing"/>
        /// </summary>
        private const int TextSpacing = 4;

        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        public DownloadFoundResults(BindableList<DownloadableMapset> availableMapsets)
        {
            Mapsets = availableMapsets;

            CreateTextCount();
            CreateTextMapsFound();

            UpdateText();

            Mapsets.ValueChanged += OnAvailableMapsetsChanged;
            Mapsets.MultipleItemsAdded += OnMultipleItemsAdded;
        }

        /// <summary>
        ///     Creates the text that displays the map count
        /// </summary>
        private void CreateTextCount()
        {
            TextCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0", 21)
            {
                Parent = this,
                Tint = Colors.MainAccent
            };
        }

        /// <summary>
        ///     Creates <see cref="TextMapsFound"/>
        /// </summary>
        private void CreateTextMapsFound()
        {
            TextMapsFound = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "MAPSETS FOUND", 21)
            {
                Parent = this,
                X = TextCount.Width + TextSpacing
            };
        }

        /// <summary>
        ///     Updates the text with the proper state and updates the container size
        /// </summary>
        private void UpdateText()
        {
            ScheduleUpdate(() =>
            {
                var count = Mapsets?.Value?.Count ?? 0;

                TextCount.Text = $"{count:n0}";

                if (count == 0 || count > 1)
                    TextMapsFound.Text = "MAPSETS FOUND";
                else
                    TextMapsFound.Text = "MAPSET FOUND";

                TextMapsFound.X = TextCount.Width + TextSpacing;

                Size = new ScalableVector2((int) (TextCount.Width + TextSpacing + TextMapsFound.Width),
                    (int) TextMapsFound.Height);
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<DownloadableMapset>> e)
            => UpdateText();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultipleItemsAdded(object sender, BindableListMultipleItemsAddedEventArgs<DownloadableMapset> e)
            => UpdateText();
    }
}