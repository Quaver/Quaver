using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataBpm : TextKeyValue
    {
        public Bindable<Map> Map { get; }

        public FilterMetadataBpm(Bindable<Map> map = null) : base("BPM:", "000", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            Map = map ?? MapManager.Selected;

            if (Map.Value != null)
                Value.Text = $"{GetBpm()}";

            ModManager.ModsChanged += OnModsChanged;
            Map.ValueChanged += OnMapChanged;
        }

        public override string ToolTipText => GetTooltipText();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Map.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => ScheduleUpdate(SetText);

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => ScheduleUpdate(SetText);

        private int GetBpm()
        {
            if (Map.Value == null)
                return 0;

            return (int) (Map.Value.Bpm * ModHelper.GetRateFromMods(ModManager.Mods));
        }

        private string GetTooltipText()
        {
            if (Map.Value == null)
                return "";

            var timingPoints = Map.Value.Qua.TimingPoints;
            var scrollVelocities = Map.Value.Qua.SliderVelocities;
            var text = $"Timing Point Count: {timingPoints.Count}\n" +
                       $"Scroll Velocity Count: {scrollVelocities.Count}\n" +
                       $"BPM Range: {timingPoints.Min().Bpm}-{timingPoints.Max().Bpm}";

            return text;
        }

        private void SetText() => Value.Text = GetBpm().ToString();
    }
}