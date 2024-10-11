using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Downloading.UI.Filter
{
    public class DownloadFilterLongNotePercentage : TextKeyValue
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        /// <param name="selectedMapset"></param>
        public DownloadFilterLongNotePercentage(Bindable<DownloadableMapset> selectedMapset)
            : base("LNs: ", "50-50%", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            SelectedMapset = selectedMapset;

            SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedMapset.ValueChanged -= OnSelectedMapsetChanged;
            base.Destroy();
        }

        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
            => SetText();

        private void SetText() => ScheduleUpdate(() =>
        {
            var minPercent = SelectedMapset.Value.Maps.Min(x => x.LongNotePercentage);
            var maxPercent = SelectedMapset.Value.Maps.Max(x => x.LongNotePercentage);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (minPercent == maxPercent)
                Value.Text = $"{(int) minPercent}%";
            else
                Value.Text = $"{(int) minPercent}-{(int) maxPercent}%";
        });
    }
}