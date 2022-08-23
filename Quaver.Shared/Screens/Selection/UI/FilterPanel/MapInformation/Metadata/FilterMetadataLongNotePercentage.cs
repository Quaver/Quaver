using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataLongNotePercentage : TextKeyValue
    {
        public FilterMetadataLongNotePercentage() : base("LNs:", "10%", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            if (MapManager.Selected.Value != null)
                Value.Text = $"{GetPercentage()}";

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        public override string ToolTipText => GetNoteCounts();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => SetText();

        private string GetPercentage()
        {
            if (MapManager.Selected.Value == null)
                return "0%";

            return ((int)MapManager.Selected.Value.LNPercentage).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private string GetNoteCounts()
        {
            if (MapManager.Selected.Value == null)
                return "No hitobjects";

            return $"{MapManager.Selected.Value.RegularNoteCount} notes, {MapManager.Selected.Value.LongNoteCount} long notes";
        }

        private void SetText() => ScheduleUpdate(() => { Value.Text = GetPercentage(); });
    }
}