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

        private const string MoveLayerUp = "Move Layer Up";

        private const string MoveLayerDown = "Move Layer Down";

        public DrawableEditorLayerRightClickOptions(EditorLayerInfo layer, EditorActionManager manager, Qua workingMap)
            : base(GetOptions(), new ScalableVector2(200, 40), 18)
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
                    case MoveLayerUp:
                        {
                            var targetIndex = workingMap.EditorLayers.IndexOf(layer);
                            if (targetIndex < 1)
                            {
                                NotificationManager.Show(NotificationLevel.Error,
                                    "Cannot move layer past the default layer!");
                            }
                            else
                            {
                                manager.MoveLayer(layer, targetIndex);
                            }
                        }
                        break;
                    case MoveLayerDown:
                        {
                            var targetIndex = workingMap.EditorLayers.IndexOf(layer) + 2;
                            if (targetIndex > workingMap.EditorLayers.Count)
                            {
                                NotificationManager.Show(NotificationLevel.Error,
                                    "Layer is already at the top!");
                            }
                            else
                            {
                                manager.MoveLayer(layer, targetIndex);
                            }
                        }
                        break;
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