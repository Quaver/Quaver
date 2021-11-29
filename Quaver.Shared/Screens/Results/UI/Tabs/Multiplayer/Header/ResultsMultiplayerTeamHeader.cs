using System;
using System.Collections.Generic;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Heading;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Header
{
    public class ResultsMultiplayerTeamHeader : ResultsOverviewScoreContainer
    {
        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        private List<ScoreProcessor> RedTeam { get; }

        private List<ScoreProcessor> BlueTeam { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="game"></param>
        /// <param name="redTeam"></param>
        /// <param name="blueTeam"></param>
        public ResultsMultiplayerTeamHeader(Map map, Bindable<ScoreProcessor> processor, MultiplayerGame game,
            List<ScoreProcessor> redTeam, List<ScoreProcessor> blueTeam) : base(map, processor)
        {
            Game = game;
            RedTeam = redTeam;
            BlueTeam = blueTeam;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void SetItems()
        {
            Items.AddRange(new []
            {
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelBlueTeam ?? UserInterface.ResultsLabelBlueTeam,
                    StringHelper.RatingToString(GetTeamAverageRating(Map, BlueTeam)), ColorHelper.HexToColor("#0587E5")),
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelScore ?? UserInterface.ResultsLabelScore,
                    $"{Game.BlueTeamWins:n0} : {Game.RedTeamWins:n0}"),
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelRedTeam ?? UserInterface.ResultsLabelRedTeam,
                    StringHelper.RatingToString(GetTeamAverageRating(Map, RedTeam)), ColorHelper.HexToColor("#F9645D")),
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public static double GetTeamAverageRating(Map map, List<ScoreProcessor> team)
        {
            if (team == null || team.Count == 0)
                return 0;

            var sum = 0d;

            foreach (var player in team)
            {
                var rating = new RatingProcessorKeys(map.DifficultyFromMods(player.Mods)).CalculateRating(player);
                sum += rating;
            }

            return sum / team.Count;
        }
    }
}