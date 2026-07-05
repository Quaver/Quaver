using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Selection;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel
{
    public class FilterPanelMapsAvailable : Container
    {
        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private bool MapsetsOnly { get; }

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
        /// <param name="mapsetsOnly"></param>
        public FilterPanelMapsAvailable(Bindable<List<Mapset>> availableMapsets, bool mapsetsOnly = false)
        {
            AvailableMapsets = availableMapsets;
            MapsetsOnly = mapsetsOnly;

            CreateTextCount();
            CreateTextMapsFound();

            UpdateText();

            AvailableMapsets.ValueChanged += OnAvailableMapsetsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            AvailableMapsets.ValueChanged -= OnAvailableMapsetsChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the text that displays the map count
        /// </summary>
        private void CreateTextCount()
        {
            TextCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "0", 18)
            {
                Parent = this,
                Tint = Colors.MainAccent
            };
        }

        /// <summary>
        ///     Creates <see cref="TextMapsFound"/>
        ///
        ///     TODO: Localize
        /// </summary>
        private void CreateTextMapsFound()
        {
            TextMapsFound = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), SelectionLocalization.Get("Maps Found"), 18)
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
                var count = GetMapCount();

                TextCount.Text = $"{count:n0}";

                if (count == 0 || count > 1)
                    TextMapsFound.Text = MapsetsOnly ? SelectionLocalization.Get("Results Found").ToUpper() : SelectionLocalization.Get("Maps Found").ToUpper();
                else
                    TextMapsFound.Text = MapsetsOnly ? SelectionLocalization.Get("Result Found").ToUpper() : SelectionLocalization.Get("Map Found").ToUpper();

                TextMapsFound.X = TextCount.Width + TextSpacing;

                Size = new ScalableVector2((int) (TextCount.Width + TextSpacing + TextMapsFound.Width),
                    (int) TextMapsFound.Height);
            });
        }

        /// <summary>
        ///     Gets the total amount of maps in <see cref="AvailableMapsets"/>
        /// </summary>
        /// <returns></returns>
        private int GetMapCount()
        {
            if (MapsetsOnly)
                return AvailableMapsets.Value.Count;

            var total = 0;

            foreach (var mapset in AvailableMapsets.Value)
            {
                if (mapset.Maps == null || mapset.Maps.Count == 0)
                    continue;

                total += mapset.Maps.Count;
            }

            return total;
        }

        /// <summary>
        ///     Called when the available mapsets has changed.
        ///     Updates the displayed count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e) => UpdateText();
    }
}
