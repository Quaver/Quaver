using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components
{
    public class DrawableMapButtonPlay : DrawableMapTextButton
    {
        public DrawableMapButtonPlay(Map map) : base(map, "PLAY", ColorHelper.HexToColor("#0fbae5"))
        {
        }
    }
}