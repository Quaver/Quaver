using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultMenuFooter : MenuFooter
    {
        public ResultMenuFooter(List<ButtonText> leftAligned, List<ButtonText> rightAlighed) : base(leftAligned, rightAlighed,
            ColorHelper.HexToColor("#236f9c"))
        {
        }
    }
}