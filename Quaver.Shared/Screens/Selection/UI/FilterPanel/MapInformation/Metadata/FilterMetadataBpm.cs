using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataBpm : TextKeyValue
    {
        public FilterMetadataBpm() : base("BPM:", "000", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            if (MapManager.Selected.Value != null)
                Value.Text = $"{GetBpm()}";

            ModManager.ModsChanged += OnModsChanged;
            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => ScheduleUpdate(SetText);

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => ScheduleUpdate(SetText);

        private int GetBpm()
        {
            if (MapManager.Selected.Value == null)
                return 0;

            return (int) (MapManager.Selected.Value.Bpm * ModHelper.GetRateFromMods(ModManager.Mods));
        }

        private void SetText() => Value.Text = GetBpm().ToString();
    }
}