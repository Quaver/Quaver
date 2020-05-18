using System;
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

            var length = TimeSpan.FromMilliseconds(Map.Value.SongLength / ModHelper.GetRateFromMods(ModManager.Mods));
            return length.Hours > 0 ? length.ToString(@"hh\:mm\:ss") : length.ToString(@"mm\:ss");
        }

        private void SetText() => ScheduleUpdate(() => Value.Text = GetLength());
    }
}