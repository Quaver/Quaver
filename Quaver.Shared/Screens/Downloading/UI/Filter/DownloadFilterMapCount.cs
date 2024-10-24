using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Downloading.UI.Filter
{
    public class DownloadFilterMapCount : TextKeyValue
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        /// <param name="selectedMapset"></param>
        public DownloadFilterMapCount(Bindable<DownloadableMapset> selectedMapset)
            : base("Maps: ", "1", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            SelectedMapset = selectedMapset;

            Value.Text = SelectedMapset.Value?.Maps.Count.ToString() ?? "1";
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
            Value.Text = SelectedMapset.Value.Maps.Count.ToString();
        });
    }
}