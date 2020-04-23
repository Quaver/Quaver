using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Screens.Tournament.Overlay;
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

        /// <summary>
        /// </summary>
        private TournamentOverlay Overlay { get; set; }

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
            CreateOverlay();
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

            Background?.Draw(gameTime);
            DrawPlayfields(gameTime);
            DrawProgressBar(gameTime);
            DrawSkipDisplay(gameTime);
            Container?.Draw(gameTime);
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
            if (TournamentScreen.GameplayScreens.Count == 2)
                Set1V1PlayfieldPositions();
            else
                SetFreeForAllPlayfieldPositions();
        }

        /// <summary>
        ///     Sets the playfield positions for a 1v1 match
        /// </summary>
        private void Set1V1PlayfieldPositions()
        {
            for (var i = 0; i < TournamentScreen.GameplayScreens.Count; i++)
            {
                var screen = TournamentScreen.GameplayScreens[i];
                var playfield = (GameplayPlayfieldKeys) screen.Ruleset.Playfield;

                playfield.Container.Width = playfield.Width + playfield.Stage.HealthBar.Width;

                var padingLeft = 92;

                if (i + 1 <= TournamentScreen.GameplayScreens.Count / 2f)
                {
                    playfield.Container.Alignment = Alignment.TopLeft;
                    playfield.Container.X = padingLeft;

                    var healthBar = playfield.Stage.HealthBar;
                    healthBar.Parent = playfield.Stage.StageLeft;
                    healthBar.X = -healthBar.Width;
                    healthBar.SpriteEffect = SpriteEffects.FlipHorizontally;
                    healthBar.ForegroundBar.SpriteEffect = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    playfield.Container.Alignment = Alignment.TopRight;
                    playfield.Container.X = -padingLeft;
                }
            }
        }

        /// <summary>
        ///     Sets the playfield positions for a FFA match
        /// </summary>
        private void SetFreeForAllPlayfieldPositions()
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
        private void CreateOverlay()
        {
            if (TournamentScreen.GameplayScreens.Count > 2 || !ConfigManager.Display1v1TournamentOverlay.Value)
                return;

            var players = new List<TournamentPlayer>();

            // Create overlay for spectator
            if (OnlineManager.CurrentGame != null)
            {
                foreach (var screen in TournamentScreen.GameplayScreens)
                {
                    var difficulty = screen.Map.SolveDifficulty(screen.Ruleset.ScoreProcessor.Mods).OverallDifficulty;

                    players.Add(new TournamentPlayer(screen.SpectatorClient.Player, screen.Ruleset.ScoreProcessor, difficulty));
                }

                Overlay = new TournamentOverlay(TournamentScreen.MainGameplayScreen.Map, OnlineManager.CurrentGame, players) { Parent = Container };
                return;
            }

            // Create players for "local multiplayer game"
            players.AddRange(TournamentScreen.GameplayScreens.Select((screen, i) => new TournamentPlayer(new User(new OnlineUser
            {
                Username = screen.LoadedReplay?.PlayerName ?? $"Player {i + 1}",
                CountryFlag = "US",
                Id = i + 1,
                UserGroups = UserGroups.Normal
            }), screen.Ruleset.ScoreProcessor, screen.Map.SolveDifficulty(screen.Ruleset.ScoreProcessor.Mods).OverallDifficulty)));

            var game = new MultiplayerGame
            {
                Type = MultiplayerGameType.Friendly,
                Ruleset = MultiplayerGameRuleset.Free_For_All,
            };

            Overlay = new TournamentOverlay(TournamentScreen.MainGameplayScreen.Map, game, players) { Parent = Container };
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
            if (!ConfigManager.DisplaySongTimeProgress.Value)
                return;

            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.ProgressBar?.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawProgressBar(GameTime gameTime)
        {
            if (!ConfigManager.DisplaySongTimeProgress.Value)
                return;

            var view = (GameplayScreenView) TournamentScreen.MainGameplayScreen.View;
            view.ProgressBar?.Draw(gameTime);
        }
    }
}