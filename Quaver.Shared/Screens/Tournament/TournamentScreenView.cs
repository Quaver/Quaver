using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tournament
{
    public class TournamentScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        public TournamentScreen TournamentScreen => (TournamentScreen) Screen;

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private List<SpriteTextPlus> Usernames { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TournamentScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            SetPlayfieldPositions();
            PositionPlayfieldItems();
            CreateUsernames();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            Background?.Update(gameTime);

            if (!TournamentScreen.Exiting)
                UpdatePlayfields(gameTime);

            UpdateProgressBar(gameTime);
            UpdateSkipDisplay(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);

            Container?.Draw(gameTime);
            Background?.Draw(gameTime);
            DrawPlayfields(gameTime);
            DrawProgressBar(gameTime);
            DrawSkipDisplay(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the <see cref="Background"/> for the map
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(BackgroundHelper.RawTexture,
            100 - ConfigManager.BackgroundBrightness.Value, false)
        {
        };

        /// <summary>
        ///     Sets the positions of each playfield
        /// </summary>
        private void SetPlayfieldPositions()
        {
            var widthSum = TournamentScreen.GameplayScreens.Sum(x =>
            {
                var playfield = (GameplayPlayfieldKeys) x.Ruleset.Playfield;
                return playfield.Width + playfield.Stage.HealthBar.Width;
            });

            var widthPer = (WindowManager.Width - widthSum) / (TournamentScreen.GameplayScreens.Count + 1);

            for (var i = 0; i < TournamentScreen.GameplayScreens.Count; i++)
            {
                var playfield = (GameplayPlayfieldKeys)  TournamentScreen.GameplayScreens[i].Ruleset.Playfield;
                playfield.Container.Width = playfield.Width + playfield.Stage.HealthBar.Width;
                playfield.Container.X = widthPer;

                if (i != 0)
                {
                    var last = TournamentScreen.GameplayScreens[i - 1].Ruleset.Playfield;
                    playfield.Container.X = last.Container.X + last.Container.Width + widthPer;
                }
            }
        }

        /// <summary>
        ///     Positions the scores of each playfield
        /// </summary>
        private void PositionPlayfieldItems()
        {
            foreach (var screen in TournamentScreen.GameplayScreens)
            {
                var view = (GameplayScreenView) screen.View;

                view.ScoreDisplay.Visible = false;
                view.KpsDisplay.Visible = false;

                if (view.JudgementCounter != null)
                    view.JudgementCounter.Visible = false;

                view.RatingDisplay.Parent = screen.Ruleset.Playfield.Container;
                view.RatingDisplay.Alignment = Alignment.TopCenter;
                view.RatingDisplay.Y = 200;
                view.RatingDisplay.X = 0;

                view.AccuracyDisplay.Parent = screen.Ruleset.Playfield.Container;
                view.AccuracyDisplay.Alignment = Alignment.TopCenter;
                view.AccuracyDisplay.Y = 250;
                view.AccuracyDisplay.X = 0;

                view.GradeDisplay.Parent = screen.Ruleset.Playfield.Container;
                view.GradeDisplay.Alignment = Alignment.TopCenter;
                view.GradeDisplay.Y = view.AccuracyDisplay.Y;
            }
        }

        /// <summary>
        /// </summary>
        private void CreateUsernames()
        {
            Usernames = new List<SpriteTextPlus>();

            for (var i = 0; i < TournamentScreen.GameplayScreens.Count; i++)
            {
                var screen = TournamentScreen.GameplayScreens[i];

                var username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                    screen.LoadedReplay?.PlayerName ?? $"Player {i + 1}", 24)
                {
                    Parent = screen.Ruleset.Playfield.Container,
                    Alignment = Alignment.TopCenter,
                    Y = 300
                };

                Usernames.Add(username);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdatePlayfields(GameTime gameTime)
        {
            foreach (var screen in TournamentScreen.GameplayScreens)
            {
                var view = (GameplayScreenView) screen.View;

               view.UpdateGradeDisplay();
               view.GradeDisplay.X = -view.AccuracyDisplay.Width / 2f - view.GradeDisplay.Width - 4;
               screen.Ruleset?.Playfield.Update(gameTime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawPlayfields(GameTime gameTime)
        {
            foreach (var screen in TournamentScreen.GameplayScreens)
                screen.Ruleset?.Playfield.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawScreenTransitioner(GameTime gameTime)
        {
            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.Transitioner.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateSkipDisplay(GameTime gameTime)
        {
            if (TournamentScreen.MainGameplayScreen.Type != TournamentScreenType.Coop
                && TournamentScreen.MainGameplayScreen.Type != TournamentScreenType.Replay)
            {
                return;
            }

            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.SkipDisplay.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawSkipDisplay(GameTime gameTime)
        {
            if (TournamentScreen.MainGameplayScreen.Type != TournamentScreenType.Coop
                && TournamentScreen.MainGameplayScreen.Type != TournamentScreenType.Replay)
            {
                return;
            }
            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.SkipDisplay.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateProgressBar(GameTime gameTime)
        {
            if (TournamentScreen.MainGameplayScreen.Type != TournamentScreenType.Coop)
                return;

            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.ProgressBar?.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawProgressBar(GameTime gameTime)
        {
            if (TournamentScreen.MainGameplayScreen.Type != TournamentScreenType.Coop)
                return;

            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.ProgressBar?.Draw(gameTime);
        }
    }
}