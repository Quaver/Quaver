

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay.UI.Multiplayer
{
    public class BattleRoyaleBackgroundAlerter : Sprite
    {
        private GameplayScreenView View { get; }

        private ScoreboardUser SelfScoreboard => View.SelfScoreboard;

        private bool DeathAnimationHandled { get; set; }

        private bool LastPlaceAnimationFadingIn { get; set; }

        private bool HandlingLastPlaceWarning { get; set; }

        private Color WarningColor { get; } = new Color(253,193,0);

        private Color EliminatedColor { get; } = new Color(145, 0, 0);

        public BattleRoyaleBackgroundAlerter(GameplayScreenView view)
        {
            View = view;
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Tint = WarningColor;
            Alpha = 0;
            Image = UserInterface.BattleRoyaleGradient;
        }

        public override void Update(GameTime gameTime)
        {
            if (SelfScoreboard != null)
            {
                if (SelfScoreboard.Processor.MultiplayerProcessor.IsBattleRoyaleEliminated && !DeathAnimationHandled)
                {
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
            FadeTo(0.4f, Easing.Linear, 800);
            FadeToColor(EliminatedColor, Easing.Linear, 800);
        }

        private void HandleLastPlaceAnimation(double dt)
        {
            if (!DeathAnimationHandled && SelfScoreboard.Rank == SelfScoreboard.Scoreboard.BattleRoyalePlayersLeft.Value && SelfScoreboard.Rank != 1)
            {
                if (Animations.Count == 0)
                {
                    var target = LastPlaceAnimationFadingIn ? 0.4f : 0;
                    FadeToColor(WarningColor, Easing.Linear, 800);
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