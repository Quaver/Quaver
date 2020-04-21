using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Tournament.Overlay;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tests.Tournaments
{
    public class TestTournamentOverlayScreenView : ScreenView
    {
        public TestTournamentOverlayScreenView(Screen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new BackgroundImage(UserInterface.MenuBackgroundClear, 80, false) { Parent = Container };
            InitializePlayfields();
            InitializeOverlay();
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();

        private void InitializePlayfields()
        {
            var playfield1 = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                X = 92,
                Size = new ScalableVector2(500, WindowManager.Width + 2),
                Tint = ColorHelper.HexToColor("#181818")
            };

            playfield1.AddBorder(Colors.MainBlue, 2);

            var playfield2 = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
                X = -92,
                Size = new ScalableVector2(500, WindowManager.Width + 2),
                Tint = ColorHelper.HexToColor("#181818")
            };

            playfield2.AddBorder(Colors.MainBlue, 2);
        }

        private void InitializeOverlay()
        {

            var map = Qua.Parse(GameBase.Game.Resources.Get($"Quaver.Resources/Maps/PrincessOfWinter/2044.qua"));
            var diffculty = new DifficultyProcessorKeys(map, new StrainConstantsKeys(), 0);

            var players = new List<TournamentPlayer>()
            {
                new TournamentPlayer(new User(new OnlineUser()
                {
                    Username = "Player1",
                    CountryFlag = "US",
                    Id = 1,
                    UserGroups = UserGroups.Normal
                }), new ScoreProcessorKeys(map, 0), diffculty.OverallDifficulty),
                new TournamentPlayer(new User(new OnlineUser()
                {
                    Username = "Player2",
                    CountryFlag = "CA",
                    Id = 2,
                    UserGroups = UserGroups.Normal
                }), new ScoreProcessorKeys(map, 0), diffculty.OverallDifficulty),
            };

            var game = new MultiplayerGame()
            {
                Type = MultiplayerGameType.Friendly,
                PlayerWins = new List<MultiplayerPlayerWins>()
                {
                    new MultiplayerPlayerWins()
                    {
                        UserId = 1,
                        Wins = 10
                    },
                    new MultiplayerPlayerWins()
                    {
                        UserId = 2,
                        Wins = 5
                    }
                }
            };

            // ReSharper disable once ObjectCreationAsStatement
            new TournamentOverlay(game, players)
            {
                Parent = Container,
            };
        }
    }
}