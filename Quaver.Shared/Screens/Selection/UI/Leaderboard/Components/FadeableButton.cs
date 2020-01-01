using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Screens.Menu.UI.Jukebox;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class FadeableButton : IconButton
    {
        public FadeableButton(Texture2D image, EventHandler clickAction = null) : base(image, clickAction)
        {
            IsPerformingFadeAnimations = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (Animations.Count == 0)
                IsPerformingFadeAnimations = true;

            base.Update(gameTime);
        }
    }
}