using Quaver.Shared.Config;
using Quaver.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsLanguage : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public SettingsLanguage(SettingsDialog dialog, string name)
            : base(dialog, name, FpsLimiterTypesToStringList(), (val, i) => OnChange(dialog, val, i), (int)ConfigManager.Language.Value)
            => ConfigManager.Language.ValueChanged += OnBindableValueChanged;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.Language.ValueChanged -= OnBindableValueChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(SettingsDialog dialog, string val, int index) => dialog.NewQueuedLanguage = (LocalizationLanguage)Enum.Parse(typeof(LocalizationLanguage), val);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> FpsLimiterTypesToStringList() => Enum.GetNames(typeof(LocalizationLanguage)).ToList();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableValueChanged(object sender, BindableValueChangedEventArgs<LocalizationLanguage> e) => Selector.SelectIndex((int)e.Value);
    }
}
