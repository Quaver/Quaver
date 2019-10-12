using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataGameMode : TextKeyValue
    {
        public FilterMetadataGameMode() : base("Mode:", "0K", 20, ColorHelper.HexToColor($"#ffe76b"))
        {
            if (MapManager.Selected.Value != null)
                Value.Text = ModeHelper.ToShortHand(MapManager.Selected.Value.Mode);

            MapManager.Selected.ValueChanged += OnMapChanged;
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

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            ScheduleUpdate(() =>
            {
                if (MapManager.Selected.Value == null)
                {
                    Value.Text = "None";
                    return;
                }

                Value.Text = ModeHelper.ToShortHand(MapManager.Selected.Value.Mode);
            });
        }
    }
}