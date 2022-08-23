using System;
using System.Globalization;
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
    public class FilterMetadataLongNotePercentage : TextKeyValue
    {
        private ImageButton ToolTipArea { get; set; }

        public FilterMetadataLongNotePercentage() : base("LNs:", "10%", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            if (MapManager.Selected.Value != null)
            {
                Value.Text = $"{GetPercentage()}";
                CreateTooltip();
            }

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        private void CreateTooltip()
        {
            ToolTipArea = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = Size,
                Alpha = 0f,
            };

            var game = GameBase.Game as QuaverGame;
            ToolTipArea.Hovered += (sender, args) => game?.CurrentScreen?.ActivateTooltip(new Tooltip(GetNoteCounts(), ColorHelper.HexToColor("#5dc7f9")));
            ToolTipArea.LeftHover += (sender, args) => game?.CurrentScreen?.DeactivateTooltip();
        }

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