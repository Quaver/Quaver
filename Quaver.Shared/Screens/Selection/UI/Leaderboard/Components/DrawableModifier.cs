using Quaver.API.Enums;
using Quaver.Shared.Modifiers;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class DrawableModifier : Sprite
    {
        public DrawableModifier(ModIdentifier mod)
        {
            Image = ModManager.GetTexture(mod);
        }
    }
}