using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataLength : TextKeyValue
    {
        public Bindable<Map> Map { get; }

        public FilterMetadataLength(Bindable<Map> map = null) : base("Length:", "00:00", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            Map = map ?? MapManager.Selected;

            if (Map.Value != null)
                Value.Text = $"{GetLength()}";

            ModManager.ModsChanged += OnModsChanged;
            Map.ValueChanged += OnMapChanged;
        }

        public override string ToolTipText => $"Drain time: {GetDrainTime()}";

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

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => SetText();

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => SetText();

        private string GetLength()
        {
            if (Map.Value == null)
                return "00:00";

            return FormatMilliseconds(Map.Value.SongLength);
        }

        private string GetDrainTime()
        {
            if (Map.Value == null || Map.Value.Qua.HitObjects.Count < 2)
                return "00:00";

            var first = Map.Value.Qua.HitObjects.First();
            var last = Map.Value.Qua.HitObjects.Last();

            var drainTime = Math.Max(last.StartTime, last.EndTime) - first.StartTime;

            return FormatMilliseconds(drainTime);
        }

        private string FormatMilliseconds(int milliseconds)
        {
            var length = TimeSpan.FromMilliseconds(milliseconds / ModHelper.GetRateFromMods(ModManager.Mods));
            return length.Hours > 0 ? length.ToString(@"hh\:mm\:ss") : length.ToString(@"mm\:ss");
        }

        private void SetText() => ScheduleUpdate(() => Value.Text = GetLength());
    }
}