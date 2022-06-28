using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataMods : TextKeyValue
    {
        public FilterMetadataMods() : base("Mods:", "None", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            ScheduleUpdate(SetText);
            ModManager.ModsChanged += OnModsChanged;
            JudgementWindowsDatabaseCache.Selected.ValueChanged += OnJudgementWindowsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            JudgementWindowsDatabaseCache.Selected.ValueChanged -= OnJudgementWindowsChanged;

            base.Destroy();
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => ScheduleUpdate(SetText);

        private void SetText()
        {
            var windows = JudgementWindowsDatabaseCache.Selected.Value == JudgementWindowsDatabaseCache.Standard
                ? "" : $" ({JudgementWindowsDatabaseCache.Selected.Value.Name})";

            Value.Text = $"{ModHelper.GetModsString(ModManager.Mods)}{windows}";
        }

        private void OnJudgementWindowsChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e) => ScheduleUpdate(SetText);
    }
}