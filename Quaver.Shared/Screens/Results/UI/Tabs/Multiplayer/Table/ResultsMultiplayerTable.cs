using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Table.Scrolling;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Table
{
    public class ResultsMultiplayerTable : Sprite
    {
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private List<ScoreProcessor> Team1Players { get; }

        /// <summary>
        /// </summary>
        private List<ScoreProcessor> Team2Players { get; }

        /// <summary>
        /// </summary>
        private Sprite HeaderContainer { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Ruleset { get; set; }

        /// <summary>
        /// </summary>
        public Dictionary<string, SpriteTextPlus> Headers { get; set; }

        /// <summary>
        /// </summary>
        private ResultsMultiplayerScrollContainer ScrollContainer { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="game"></param>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public ResultsMultiplayerTable(Map map, Bindable<ScoreProcessor> processor, MultiplayerGame game,
            List<ScoreProcessor> team1, List<ScoreProcessor> team2)
        {
            Map = map;
            Processor = processor;
            Game = game;
            Team1Players = team1;
            Team2Players = team2;

            Width = ResultsScreenView.CONTENT_WIDTH - ResultsTabContainer.PADDING_X;

            switch (Game.Ruleset)
            {
                case MultiplayerGameRuleset.Free_For_All:
                case MultiplayerGameRuleset.Battle_Royale:
                    Image = SkinManager.Skin?.Results?.ResultsMultiplayerFFAPanel ?? UserInterface.ResultsMultiplayerFFAPanel;
                    Height = Image.Height + 4;
                    break;
                case MultiplayerGameRuleset.Team:
                    Image = SkinManager.Skin?.Results?.ResultsMultiplayerTeamPanel ?? UserInterface.ResultsMultiplayerTeamPanel;
                    Height = Image.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CreateHeaderContainer();
            CreateRulesetText();
            CreateColumnHeaders();
            CreateScrollContainer();
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderContainer() => HeaderContainer = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 68),
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateRulesetText()
        {
            Ruleset = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                Game.Ruleset.ToString().Replace("_", " "),
                22)
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = 22,
                Tint = ColorHelper.HexToColor("#00D1FF")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateColumnHeaders()
        {
            var headers = new List<string>
            {
                "Rating",
                "Grade",
                "Accuracy",
                "Max Combo",
                "Marv",
                "Perf",
                "Great",
                "Good",
                "Okay",
                "Miss",
                "Mods"
            };

            var lastWidth = 0f;
            var lastX = 0f;

            const int firstColumnPadding = 80;

            Headers = new Dictionary<string, SpriteTextPlus>();

            for (var i = headers.Count - 1; i >= 0; i--)
            {
                // ReSharper disable once ObjectCreationAsStatement
                var txt = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), headers[i], 22)
                {
                    Parent = HeaderContainer,
                    Alignment = Alignment.MidRight,
                };

                if (i == headers.Count - 2)
                    txt.X = lastX - lastWidth - firstColumnPadding;
                else if (i != headers.Count - 1)
                    txt.X = lastX - lastWidth - 60;
                else
                    txt.X = -firstColumnPadding;

                lastWidth = txt.Width;
                lastX = txt.X;

                Headers.Add(headers[i], txt);
            }
        }

        /// <summary>
        /// </summary>
        private void CreateScrollContainer()
        {
            var processors = GetOrderedUserList();

            ScrollContainer = new ResultsMultiplayerScrollContainer(new ScalableVector2(Width, Height - HeaderContainer.Height),
                processors, Game, Headers, Map)
            {
                Parent = this,
                Y = HeaderContainer.Height
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private List<ScoreProcessor> GetOrderedUserList()
        {
            var players = new List<ScoreProcessor>(Team1Players);
            players = players.Concat(Team2Players).ToList();

            var qua = Map.LoadQua();

            switch (Game.Ruleset)
            {
                case MultiplayerGameRuleset.Battle_Royale:
                case MultiplayerGameRuleset.Free_For_All:
                    players = players.OrderByDescending(x => new RatingProcessorKeys(qua.SolveDifficulty(x.Mods, true).OverallDifficulty).CalculateRating(x)).ToList();
                    break;
                case MultiplayerGameRuleset.Team:
                    players = players.OrderByDescending(x => GetTeamFromScoreProcessor(Game, x))
                        .ThenByDescending(x => new RatingProcessorKeys(qua.SolveDifficulty(x.Mods, true).OverallDifficulty).CalculateRating(x)).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return players;
        }

        /// <summary>
        /// </summary>
        /// <param name="processor"></param>
        /// <returns></returns>
        public static MultiplayerTeam GetTeamFromScoreProcessor(MultiplayerGame game, ScoreProcessor processor)
        {
            if (game.RedTeamPlayers.Contains(processor.UserId))
                return MultiplayerTeam.Red;

            if (game.BlueTeamPlayers.Contains(processor.UserId))
                return MultiplayerTeam.Blue;

            return MultiplayerTeam.Red;
        }
    }
}