using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class GameplayNumberDisplay : NumberDisplay
    {
        internal GameplayNumberDisplay(NumberDisplayType type, string startingValue, Vector2 imageScale) : base(type, startingValue, imageScale)
        {
        }

        public override void Update(GameTime gameTime)
        {
            Visible = ConfigManager.DisplayGameplayOverlay?.Value ?? true;
            base.Update(gameTime);
        }
    }
}