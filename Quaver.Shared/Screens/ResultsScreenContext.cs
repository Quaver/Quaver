using System.Collections.Generic;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Screens.Gameplay;

namespace Quaver.Shared.Screens
{
    internal enum ResultsScreenContextType
    {
        Gameplay,
        MultiplayerGameplay,
        MultiplayerScore,
        Score,
        Replay
    }

    /// <summary>
    ///     Type-safe input shared by legacy and replacement results-screen factories.
    /// </summary>
    internal sealed class ResultsScreenContext
    {
        internal ResultsScreenContextType Type { get; }

        internal GameplayScreen Gameplay { get; }

        internal Map Map { get; }

        internal MultiplayerGame MultiplayerGame { get; }

        internal Score Score { get; }

        internal Replay Replay { get; }

        internal List<ScoreProcessor> Team1 { get; }

        internal List<ScoreProcessor> Team2 { get; }

        private ResultsScreenContext(ResultsScreenContextType type, GameplayScreen gameplay = null,
            Map map = null, MultiplayerGame multiplayerGame = null, Score score = null,
            Replay replay = null, List<ScoreProcessor> team1 = null, List<ScoreProcessor> team2 = null)
        {
            Type = type;
            Gameplay = gameplay;
            Map = map;
            MultiplayerGame = multiplayerGame;
            Score = score;
            Replay = replay;
            Team1 = team1;
            Team2 = team2;
        }

        internal static ResultsScreenContext FromGameplay(GameplayScreen gameplay) =>
            new ResultsScreenContext(ResultsScreenContextType.Gameplay, gameplay: gameplay);

        internal static ResultsScreenContext FromMultiplayerGameplay(GameplayScreen gameplay,
            MultiplayerGame game, List<ScoreProcessor> team1, List<ScoreProcessor> team2) =>
            new ResultsScreenContext(ResultsScreenContextType.MultiplayerGameplay, gameplay,
                multiplayerGame: game, team1: team1, team2: team2);

        internal static ResultsScreenContext FromMultiplayerScore(Map map, MultiplayerGame game,
            Score score, List<ScoreProcessor> team1, List<ScoreProcessor> team2) =>
            new ResultsScreenContext(ResultsScreenContextType.MultiplayerScore, map: map,
                multiplayerGame: game, score: score, team1: team1, team2: team2);

        internal static ResultsScreenContext FromScore(Map map, Score score) =>
            new ResultsScreenContext(ResultsScreenContextType.Score, map: map, score: score);

        internal static ResultsScreenContext FromReplay(Map map, Replay replay) =>
            new ResultsScreenContext(ResultsScreenContextType.Replay, map: map, replay: replay);
    }
}
