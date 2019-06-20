using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Multiplayer
{
    public class BattleRoyaleAlert : Sprite
    {
        private GameplayScreen Screen { get; }

        private GameplayScreenView View => (GameplayScreenView) Screen.View;

        private ScoreboardUser SelfScoreboard => View.SelfScoreboard;

        private bool DeathAnimationHandled { get; set; }

        private bool LastPlaceAnimationFadingIn { get; set; }

        private bool HandlingLastPlaceWarning { get; set; }

        public BattleRoyaleAlert(GameplayScreen screen)
        {
            Screen = screen;
            Alpha = 0;
            Image = SkinManager.Skin.BattleRoyaleEliminated;
        }

        public override void Update(GameTime gameTime)
        {
            if (SelfScoreboard != null)
            {
                if (SelfScoreboard.Processor.MultiplayerProcessor.IsBattleRoyaleEliminated && !DeathAnimationHandled)
                {
                    Image = SkinManager.Skin.BattleRoyaleEliminated;
                    HandleDeathAnimation();
                    DeathAnimationHandled = true;
                }

                HandleLastPlaceAnimation(gameTime.ElapsedGameTime.TotalMilliseconds);
            }

            base.Update(gameTime);
        }

        private void HandleDeathAnimation()
        {
            ClearAnimations();
            FadeTo(1f, Easing.Linear, 800);
        }

        private void HandleLastPlaceAnimation(double dt)
        {
            if (!DeathAnimationHandled && SelfScoreboard.Rank == SelfScoreboard.Scoreboard.BattleRoyalePlayersLeft.Value && SelfScoreboard.Rank != 1)
            {
                if (Animations.Count == 0)
                {
                    Image = SkinManager.Skin.BattleRoyaleWarning;
                    var target = LastPlaceAnimationFadingIn ? 1f : 0;

                    FadeTo(target, Easing.Linear, 800);
                    LastPlaceAnimationFadingIn = !LastPlaceAnimationFadingIn;
                }

                HandlingLastPlaceWarning = true;
            }
            else
            {
                HandlingLastPlaceWarning = false;

                if (!DeathAnimationHandled && !HandlingLastPlaceWarning)
                    Alpha = MathHelper.Lerp(Alpha, 0, (float) Math.Min(dt / 800, 1));
            }
        }
    }
}