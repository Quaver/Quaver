using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsEditorSnapColors : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public SettingsEditorSnapColors(SettingsDialog dialog)
            : base(dialog, "Beat Snap Color Type", BeatSnapColorTypesToList(), OnChange, (int) ConfigManager.EditorBeatSnapColorType.Value)
            => ConfigManager.EditorBeatSnapColorType.ValueChanged += OnBindableValueChanged;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.EditorBeatSnapColorType.ValueChanged -= OnBindableValueChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(string val, int index)
            => ConfigManager.EditorBeatSnapColorType.Value = (EditorBeatSnapColor) Enum.Parse(typeof(EditorBeatSnapColor), val);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> BeatSnapColorTypesToList() => Enum.GetNames(typeof(EditorBeatSnapColor)).ToList();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableValueChanged(object sender, BindableValueChangedEventArgs<EditorBeatSnapColor> e)
            => Selector.SelectIndex((int) e.Value);
    }
}