using System.Globalization;
using Quaver.API.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;
using System;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataNotesPerSecond : TextKeyValue
    {
        public FilterMetadataNotesPerSecond() : base("NPS: ", "0", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            if (MapManager.Selected.Value != null)
                Value.Text = $"{GetNotesPerSecond()}";

            MapManager.Selected.ValueChanged += OnMapChanged;
            ModManager.ModsChanged += OnModsChanged;
        }

        public override string ToolTipText => $"Actions per second: {GetActionsPerSecond()}";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => SetText();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private double GetNotesPerSecond()
        {
            if (MapManager.Selected.Value == null)
                return 0;

            return Math.Floor(MapManager.Selected.Value.NotesPerSecond * 100) / 100;
        }

        private double GetActionsPerSecond()
        {
            if (MapManager.Selected.Value == null)
                return 0;

            return Math.Floor(MapManager.Selected.Value.Qua.GetActionsPerSecond() * 100) / 100;
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => SetText();

        private void SetText() => ScheduleUpdate(() => Value.Text = GetNotesPerSecond().ToString(CultureInfo.InvariantCulture));
    }
}