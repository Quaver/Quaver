using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Screens.Tournament.Overlay;
using Wobble;
using Wobble.Graphics;
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
        public TournamentScreen TournamentScreen => (TournamentScreen)Screen;

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private List<SpriteTextPlus> Usernames { get; set; }

        /// <summary>
        /// </summary>
        private TournamentOverlay Overlay { get; set; }

        /// <summary>
        /// </summary>
        public List<TournamentPlayer> TournamentPlayers { get; private set; }

        /// <summary>
        ///     Screen-space clipping bounds for tiled gameplay screens.
        /// </summary>
        private Dictionary<TournamentGameplayScreen, RectangleF> GameplayScreenClipBounds { get; } =
            new Dictionary<TournamentGameplayScreen, RectangleF>();

        /// <summary>
        ///     Rasterizer state used when drawing clipped tournament gameplay views.
        /// </summary>
        private static readonly RasterizerState ScissorRasterizerState = new RasterizerState
        {
            ScissorTestEnable = true
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TournamentScreenView(Screen screen) : base(screen)
        {
            if (TournamentScreen.GameplayScreens.Count == 0)
            {
                OnlineManager.LeaveGame();
                TournamentScreen.Exit(() => new MultiplayerLobbyScreen());
            }

            foreach (var gameplayScreen in TournamentScreen.GameplayScreens)
            {
                var playfield = (GameplayPlayfieldKeys)gameplayScreen.Ruleset.Playfield;
                playfield.PlayfieldMask.Visible = false;
            }

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
            DrawSkipDisplay(gameTime);
            Container?.Draw(gameTime);
            DrawProgressBar(gameTime);
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
                var playfield = (GameplayPlayfieldKeys)screen.Ruleset.Playfield;

                playfield.Container.Width = playfield.Width + playfield.Stage.HealthBar.Width;

                var paddingLeft = SkinManager.Skin.Keys[screen.Map.Mode].CoopPlayfieldPadding;

                if (i + 1 <= TournamentScreen.GameplayScreens.Count / 2f)
                {
                    playfield.Container.Alignment = Alignment.TopLeft;
                    playfield.Container.X = paddingLeft;

                    var healthBar = playfield.Stage.HealthBar;
                    healthBar.Parent = playfield.Stage.StageLeft;
                    healthBar.X = -healthBar.Width;
                    healthBar.SpriteEffect = SpriteEffects.FlipHorizontally;
                    healthBar.ForegroundBar.SpriteEffect = SpriteEffects.FlipHorizontally;

                    var hitBubbles = playfield.Stage.HitBubbles;
                    hitBubbles.Parent = playfield.Stage.StageRight;
                    hitBubbles.Alignment = Alignment.MidRight;
                    hitBubbles.X = -hitBubbles.X + hitBubbles.Width / 2;
                }
                else
                {
                    playfield.Container.Alignment = Alignment.TopRight;
                    playfield.Container.X = -paddingLeft;
                }
            }
        }

        /// <summary>
        ///     Tiles screens to fit the given rectangle
        /// </summary>
        /// <param name="screens"></param>
        /// <param name="rectangle"></param>
        /// <param name="clipBounds"></param>
        private static void TileScreens(List<TournamentGameplayScreen> screens, RectangleF rectangle,
            IDictionary<TournamentGameplayScreen, RectangleF> clipBounds)
        {
            if (screens.Count <= 0)
                return;

            if (screens.Count == 1)
            {
                var gameplayScreen = screens[0];
                var screen = (GameplayPlayfieldKeys)gameplayScreen.Ruleset.Playfield;
                var supposedScale = rectangle.Size / screen.Container.AbsoluteSize;
                var minScale = Math.Min(supposedScale.X, supposedScale.Y);
                var scale = new Vector2(minScale);
                // Console.WriteLine($"Screen size {screen.Container.AbsoluteSize}");
                var pos = rectangle.Position + (supposedScale - scale) / 2 * screen.Container.AbsoluteSize;
                screen.Container.Scale = scale;
                screen.Container.X = pos.X;
                screen.Container.Y = pos.Y;

                // Scaled width accepted to overflow from the playfield container (hit bubble and health bar)
                var allowedOverflowX = 70f * scale.X;
                clipBounds[gameplayScreen] = new RectangleF(pos.X - allowedOverflowX / 2, pos.Y,
                    screen.Container.Width * scale.X + allowedOverflowX, screen.Container.Height * scale.Y);
                // Console.WriteLine($"Screen placed at {pos} with scale {scale} (supposed scale {supposedScale}) to fit {rectangle}");
                return;
            }

            if (screens.Count == 3)
            {
                var tileSize = new Size2(rectangle.Width / 2f, rectangle.Height / 2f);

                TileScreens(new List<TournamentGameplayScreen> { screens[0] },
                    new RectangleF(rectangle.X + rectangle.Width / 4f, rectangle.Y, tileSize.Width, tileSize.Height),
                    clipBounds);
                TileScreens(new List<TournamentGameplayScreen> { screens[1] },
                    new RectangleF(rectangle.X, rectangle.Y + rectangle.Height / 2f, tileSize.Width, tileSize.Height),
                    clipBounds);
                TileScreens(new List<TournamentGameplayScreen> { screens[2] },
                    new RectangleF(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, tileSize.Width, tileSize.Height),
                    clipBounds);
                return;
            }

            var widthHeightRatio = rectangle.Width / rectangle.Height;
            const float horizontalMinimumRatio = 3 / 4f;

            // Tile it vertically or horizontally depending on rectangle width and height
            if (widthHeightRatio > horizontalMinimumRatio)
            {
                // Tile it horizontally
                var size = new Size2(rectangle.Width / 2, rectangle.Height);
                TileScreens(screens.Take(screens.Count / 2).ToList(),
                    new RectangleF(rectangle.Position, size), clipBounds);
                TileScreens(screens.Skip(screens.Count / 2).ToList(),
                    new RectangleF(rectangle.Position + new Size2(size.Width, 0), size), clipBounds);
            }
            else
            {
                // Tile vertically
                var size = new Size2(rectangle.Width, rectangle.Height / 2);
                TileScreens(screens.Take(screens.Count / 2).ToList(),
                    new RectangleF(rectangle.Position, size), clipBounds);
                TileScreens(screens.Skip(screens.Count / 2).ToList(),
                    new RectangleF(rectangle.Position + new Size2(0, size.Height), size), clipBounds);
            }
        }

        /// <summary>
        ///     Sets the playfield positions for a FFA match
        /// </summary>
        private void SetFreeForAllPlayfieldPositions()
        {
            var screensCount = TournamentScreen.GameplayScreens.Count;
            for (var i = 0; i < screensCount; i++)
            {
                var playfield = (GameplayPlayfieldKeys)TournamentScreen.GameplayScreens[i].Ruleset.Playfield;
                playfield.Container.Width = playfield.Width + playfield.Stage.HealthBar.Width;
                playfield.Container.Pivot = Vector2.Zero;
                
                playfield.Container.AddBorder(Color.Red, 3);
            }

            GameplayScreenClipBounds.Clear();

            TileScreens(TournamentScreen.GameplayScreens,
                    new RectangleF(0, 0, WindowManager.Width, WindowManager.Height), GameplayScreenClipBounds);
        }

        /// <summary>
        ///     Positions the scores of each playfield
        /// </summary>
        private void PositionPlayfieldItems()
        {
            if (TournamentScreen.GameplayScreens.Count == 2 && !ConfigManager.TournamentDisplay1v1PlayfieldScores.Value)
                return;

            foreach (var screen in TournamentScreen.GameplayScreens)
            {
                var view = (GameplayScreenView)screen.View;

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
            if (TournamentScreen.GameplayScreens.Count == 2 && !ConfigManager.TournamentDisplay1v1PlayfieldScores.Value)
                return;

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
            TournamentPlayers = new List<TournamentPlayer>();

            if (TournamentScreen.GameplayScreens.Count > 2 || !ConfigManager.Display1v1TournamentOverlay.Value)
                return;

            // Create overlay for spectator
            if (OnlineManager.CurrentGame != null)
            {
                foreach (var screen in TournamentScreen.GameplayScreens)
                {
                    var difficulty = screen.Map.SolveDifficulty(screen.Ruleset.ScoreProcessor.Mods).OverallDifficulty;

                    TournamentPlayers.Add(new TournamentPlayer(screen.SpectatorClient.Player, screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor, difficulty));
                }

                Overlay = new TournamentOverlay(TournamentScreen.MainGameplayScreen.Map, OnlineManager.CurrentGame, TournamentPlayers) { Parent = Container };
                return;
            }

            // Create players for "local multiplayer game"
            TournamentPlayers.AddRange(TournamentScreen.GameplayScreens.Select((screen, i) => new TournamentPlayer(new User(new OnlineUser
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

            Overlay = new TournamentOverlay(TournamentScreen.MainGameplayScreen.Map, game, TournamentPlayers) { Parent = Container };
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdatePlayfields(GameTime gameTime)
        {
            foreach (var screen in TournamentScreen.GameplayScreens)
            {
                var view = (GameplayScreenView)screen.View;

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
            {
                if (!GameplayScreenClipBounds.TryGetValue(screen, out var clipBounds))
                {
                    screen.Ruleset?.Playfield.Draw(gameTime);
                    continue;
                }

                DrawPlayfieldClipped(screen, clipBounds, gameTime);
            }
        }

        /// <summary>
        ///     Draws a playfield clipped to its tournament tile.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="clipBounds"></param>
        /// <param name="gameTime"></param>
        private static void DrawPlayfieldClipped(TournamentGameplayScreen screen, RectangleF clipBounds, GameTime gameTime)
        {
            var graphicsDevice = GameBase.Game.GraphicsDevice;
            var previousScissorRectangle = graphicsDevice.ScissorRectangle;
            var previousDefaultRasterizerState = GameBase.DefaultSpriteBatchOptions.RasterizerState;
            var changedSpriteBatchOptions = new List<(Drawable Drawable, RasterizerState RasterizerState)>();

            GameBase.Game.TryEndBatch();

            try
            {
                graphicsDevice.ScissorRectangle = ToBackBufferRectangle(clipBounds);
                GameBase.DefaultSpriteBatchOptions.RasterizerState = ScissorRasterizerState;

                var playfield = (GameplayPlayfieldKeys)screen.Ruleset.Playfield;
                EnableScissorOnSpriteBatchOptions(playfield.Container, changedSpriteBatchOptions);

                screen.Ruleset?.Playfield.Draw(gameTime);
                GameBase.Game.TryEndBatch();
            }
            finally
            {
                GameBase.Game.TryEndBatch();

                foreach (var item in changedSpriteBatchOptions)
                    item.Drawable.SpriteBatchOptions.RasterizerState = item.RasterizerState;

                GameBase.DefaultSpriteBatchOptions.RasterizerState = previousDefaultRasterizerState;
                graphicsDevice.ScissorRectangle = previousScissorRectangle;
            }
        }

        /// <summary>
        ///     Converts virtual screen coordinates to back-buffer coordinates for scissor testing.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private static Rectangle ToBackBufferRectangle(RectangleF rectangle)
        {
            var graphics = GameBase.Game.Graphics;
            var widthScale = graphics.PreferredBackBufferWidth / WindowManager.Width;
            var heightScale = graphics.PreferredBackBufferHeight / WindowManager.Height;

            return new Rectangle
            {
                X = (int)(rectangle.X * widthScale),
                Y = (int)(rectangle.Y * heightScale),
                Width = (int)(rectangle.Width * widthScale),
                Height = (int)(rectangle.Height * heightScale)
            };
        }

        /// <summary>
        ///     Ensures custom sprite batches inside the playfield do not bypass the active scissor rectangle.
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="changedSpriteBatchOptions"></param>
        private static void EnableScissorOnSpriteBatchOptions(Drawable drawable,
            ICollection<(Drawable Drawable, RasterizerState RasterizerState)> changedSpriteBatchOptions)
        {
            if (drawable.SpriteBatchOptions != null)
            {
                changedSpriteBatchOptions.Add((drawable, drawable.SpriteBatchOptions.RasterizerState));
                drawable.SpriteBatchOptions.RasterizerState = ScissorRasterizerState;
            }

            foreach (var child in drawable.Children)
                EnableScissorOnSpriteBatchOptions(child, changedSpriteBatchOptions);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawScreenTransitioner(GameTime gameTime)
        {
            var view = (GameplayScreenView)TournamentScreen.MainGameplayScreen.View;
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

            var view = (GameplayScreenView)TournamentScreen.MainGameplayScreen.View;
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
            var view = (GameplayScreenView)TournamentScreen.MainGameplayScreen.View;
            view.SkipDisplay.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateProgressBar(GameTime gameTime)
        {
            if (!ConfigManager.DisplaySongTimeProgress.Value)
                return;

            var view = (GameplayScreenView)TournamentScreen.MainGameplayScreen.View;
            view.ProgressBar?.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawProgressBar(GameTime gameTime)
        {
            if (!ConfigManager.DisplaySongTimeProgress.Value)
                return;

            var view = (GameplayScreenView)TournamentScreen.MainGameplayScreen.View;
            view.ProgressBar?.Draw(gameTime);
        }
    }
}
