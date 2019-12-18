using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class FilterMetadataCreator : TextKeyValue
    {
        public FilterMetadataCreator() : base("By:", "", 21, ColorHelper.HexToColor("#10C8F6"))
        {
            if (MapManager.Selected.Value != null)
                Value.Text = MapManager.Selected.Value.Creator;

            Value.TruncateWithEllipsis(300);

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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (MapManager.Selected.Value != null)
            {
                Value.Text = MapManager.Selected.Value.Creator;
                Value.TruncateWithEllipsis(300);
            }
        }
    }
}