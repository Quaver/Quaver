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
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public class DrawableEditorLayerRightClickOptions : RightClickOptions
    {
        private static string EditName => LocalizationManager.Get("Screen_Editor_EditName");

        private static string ChangeColor => LocalizationManager.Get("Screen_Editor_ChangeColor");

        private static string MoveLayerUp => LocalizationManager.Get("Screen_Editor_MoveLayerUp");

        private static string MoveLayerDown => LocalizationManager.Get("Screen_Editor_MoveLayerDown");

        public DrawableEditorLayerRightClickOptions(EditorLayerInfo layer, EditorActionManager manager, Qua workingMap)
            : base(GetOptions(), new ScalableVector2(200, 40), 18)
        {
            ItemSelected += (sender, args) =>
            {
                if (args.Text == EditName)
                {
                    DialogManager.Show(new DialogRenameLayer(layer, manager, workingMap));
                }
                else if (args.Text == ChangeColor)
                {
                    DialogManager.Show(new DialogChangeLayerColor(layer, manager, workingMap));
                }
                else if (args.Text == MoveLayerUp)
                {
                    var targetIndex = workingMap.EditorLayers.IndexOf(layer);
                    if (targetIndex < 1)
                    {
                        NotificationManager.Show(NotificationLevel.Error,
                            LocalizationManager.Get("Screen_Editor_CannotMoveLayerPastDefault"));
                    }
                    else
                    {
                        manager.MoveLayer(layer, targetIndex);
                    }
                }
                else if (args.Text == MoveLayerDown)
                {
                    var targetIndex = workingMap.EditorLayers.IndexOf(layer) + 2;
                    if (targetIndex > workingMap.EditorLayers.Count)
                    {
                        NotificationManager.Show(NotificationLevel.Error,
                            LocalizationManager.Get("Screen_Editor_LayerAlreadyAtTop"));
                    }
                    else
                    {
                        manager.MoveLayer(layer, targetIndex);
                    }
                }
            };
        }

        private static Dictionary<string, Color> GetOptions() => new Dictionary<string, Color>()
        {
            {EditName, ColorHelper.HexToColor("#0787E3")},
            {ChangeColor, ColorHelper.HexToColor("#27B06E")},
            {MoveLayerUp, ColorHelper.HexToColor("#007BFF")},
            {MoveLayerDown, ColorHelper.HexToColor("#007BFF")},
        };
    }
}
