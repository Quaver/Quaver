using System.Collections.Generic;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Table;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer
{
    public class ResultsMultiplayerTab : ResultsTabContainer
    {
        /// <summary>
        /// </summary>
        private Container ContentContainer { get; set; }

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
        private SpriteTextPlus MatchPlayedDate { get; set; }

        /// <summary>
        /// </summary>
        private ResultsMultiplayerTable Table { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="activeTab"></param>
        /// <param name="game"></param>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public ResultsMultiplayerTab(Map map, Bindable<ScoreProcessor> processor, Bindable<ResultsScreenTabType> activeTab,
            MultiplayerGame game, List<ScoreProcessor> team1, List<ScoreProcessor> team2) : base(map, processor, activeTab,
            null, null)
        {
            Game = game;
            Team1Players = team1;
            Team2Players = team2;

            CreateContentContainer();
            CreateMatchPlayedText();
            CreateTable();
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainer() => ContentContainer = new Container
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width, 645)
        };

        /// <summary>
        /// </summary>
        private void CreateMatchPlayedText()
        {
            var time = $"{Processor.Value.Date:hh:mm:ss tt}";

            MatchPlayedDate = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"Match played on {Processor.Value.Date.ToShortDateString()} @ {time}", 22)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                Tint = ColorHelper.HexToColor("#808080")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTable() => Table = new ResultsMultiplayerTable(Map, Processor, Game, Team1Players, Team2Players)
        {
            Parent = ContentContainer,
            Alignment = Alignment.BotLeft,
            X = PADDING_X
        };
    }
}