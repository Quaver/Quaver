using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components
{
    public class DrawableMapButtonEdit : DrawableMapTextButton
    {
        public DrawableMapButtonEdit(Map map) : base(map, "EDIT", ColorHelper.HexToColor("#f2994a"))
        {
        }
    }
}