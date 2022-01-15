using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Actions.Layers.Rename;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorChangeWaveformColorDialog : ColorDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorChangeWaveformColorDialog() : base("CHANGE WAVEFORM COLOR",
            "Enter a new RGB color for your waveform... (default: 0,200,255)")
        {
            var currentEditorWaveformColor = new Color(
                ConfigManager.EditorWaveformColorR.Value,
                ConfigManager.EditorWaveformColorG.Value,
                ConfigManager.EditorWaveformColorB.Value
            );

            UpdateColor(currentEditorWaveformColor);
        }

        protected override void OnColorChange(Color newColor)
        {
            ConfigManager.EditorWaveformColorR.Value = newColor.R;
            ConfigManager.EditorWaveformColorG.Value = newColor.G;
            ConfigManager.EditorWaveformColorB.Value = newColor.B;
        }
    }
}