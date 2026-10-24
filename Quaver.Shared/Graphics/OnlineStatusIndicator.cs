using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics;

public static class OnlineStatusIndicator
{
    public static Color OfflineColor { get; } = new Color(255, 58, 111);

    public static void SetOnline(Sprite indicator, bool online, Color? offlineColor = null)
    {
        indicator.Image = online ? UserInterface.HubOnlineIconV2 : RoundedRectTextureCache.Get(20, 20, 10);
        indicator.Tint = online ? Color.White : offlineColor ?? OfflineColor;
    }
}
