using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Quaver.Shared.Screens.Edit.UI.Panels.Layers.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public class DrawableEditorLayerRightClickOptions : RightClickOptions
    {
        private const string EditName = "Edit Name";

        private const string ChangeColor = "Change Color";

        private const string Merge = "Merge";

        public DrawableEditorLayerRightClickOptions(EditorLayerInfo layer, EditorActionManager manager, Qua workingMap)
            : base(GetOptions(), new ScalableVector2(200, 40), 22)
        {
            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case EditName:
                        DialogManager.Show(new DialogRenameLayer(layer, manager, workingMap));
                        break;
                    case ChangeColor:
                        DialogManager.Show(new DialogChangeLayerColor(layer, manager, workingMap));
                        break;
                    case Merge:
                        var destLayer = manager.EditScreen.SelectedLayer.Value;
                        if (layer == destLayer)
                        {
                            NotificationManager.Show(NotificationLevel.Warning, "You cannot merge a layer onto itself!");
                            break;
                        }
                        manager.MergeLayers(layer, destLayer);
                        break;
                }
            };
        }

        private static Dictionary<string, Color> GetOptions() => new Dictionary<string, Color>()
        {
            {EditName, ColorHelper.HexToColor("#0787E3")},
            {ChangeColor, ColorHelper.HexToColor("#27B06E")},
            {Merge, ColorHelper.HexToColor("#8622e3")},
        };
    }
}