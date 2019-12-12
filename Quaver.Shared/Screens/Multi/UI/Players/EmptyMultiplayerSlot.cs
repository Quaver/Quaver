using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Multi.UI.Players
{
    public class EmptyMultiplayerSlot : MultiplayerSlot
    {
        public EmptyMultiplayerSlot()
        {
            Tint = ColorHelper.HexToColor("#2A2A2A");
            // Alpha = 0.25f;
        }
    }
}