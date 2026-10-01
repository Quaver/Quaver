using System;
using System.Collections.Generic;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Downloading;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Initialization;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Results;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Theater;
using Quaver.Shared.Screens.V2;
using Wobble.Logging;

namespace Quaver.Shared.Screens
{
    /// <summary>
    ///     Creates screens from either the legacy implementation set or the replacement set.
    /// </summary>
    internal static class QuaverScreenFactory
    {
        private static readonly ScreenFactorySet Legacy = new()
        {
            Initialization = () => new InitializationScreen(),
            MainMenu = () => new MainMenuScreen(),
            Selection = (activeScrollContainer, activeLeftPanel) =>
                new SelectionScreen(activeScrollContainer, activeLeftPanel),
            Downloading = previousScreen => new DownloadingScreen(previousScreen),
            MultiplayerLobby = () => new MultiplayerLobbyScreen(),
            MusicPlayer = () => new MusicPlayerScreen(),
            Theater = () => new TheaterScreen(),
            Importing = (multiplayerScreen, fromSelect, fullSync, selectMapIdAfterImport) =>
                new ImportingScreen(multiplayerScreen, fromSelect, fullSync, selectMapIdAfterImport),
            MapLoading = (scores, replay, spectatorClient) =>
                new MapLoadingScreen(scores, replay, spectatorClient),
            MultiplayerGame = () => new MultiplayerGameScreen(),
            Multiplayer = (game, playTrackOnFirstUpdate) =>
                new MultiplayerScreen(game, playTrackOnFirstUpdate),
            Results = CreateLegacyResults
        };

        private static readonly ScreenFactorySet NewScreens = NewScreenRegistry.CreateFactorySet();

        /// <summary>
        ///     The startup snapshot of the replacement-screen configuration value.
        /// </summary>
        private static bool UseNewScreens { get; set; }

        internal static void Initialize(bool useNewScreens) => UseNewScreens = useNewScreens;

        internal static QuaverScreen CreateMainMenu() =>
            Resolve(nameof(ScreenFactorySet.MainMenu), Legacy.MainMenu!, NewScreens.MainMenu)();

        internal static QuaverScreen CreateInitialization() =>
            Resolve(nameof(ScreenFactorySet.Initialization), Legacy.Initialization!,
                NewScreens.Initialization)();

        internal static QuaverScreen CreateSelection(
            SelectScrollContainerType? activeScrollContainer = null,
            SelectContainerPanel activeLeftPanel = SelectContainerPanel.Leaderboard)
        {
            if (MapsetImporter.Queue.Count > 0 || QuaverSettingsDatabaseCache.OutdatedMaps.Count != 0 ||
                MapDatabaseCache.MapsToUpdate.Count != 0)
                return CreateImporting(null, true);

            return Resolve(nameof(ScreenFactorySet.Selection), Legacy.Selection!, NewScreens.Selection)(
                activeScrollContainer, activeLeftPanel);
        }

        internal static QuaverScreen CreateDownloading(QuaverScreenType previousScreen = QuaverScreenType.Menu) =>
            Resolve(nameof(ScreenFactorySet.Downloading), Legacy.Downloading!, NewScreens.Downloading)(previousScreen);

        internal static QuaverScreen CreateMultiplayerLobby() =>
            Resolve(nameof(ScreenFactorySet.MultiplayerLobby), Legacy.MultiplayerLobby!, NewScreens.MultiplayerLobby)();

        internal static QuaverScreen CreateMusicPlayer() =>
            Resolve(nameof(ScreenFactorySet.MusicPlayer), Legacy.MusicPlayer!, NewScreens.MusicPlayer)();

        internal static QuaverScreen CreateTheater() =>
            Resolve(nameof(ScreenFactorySet.Theater), Legacy.Theater!, NewScreens.Theater)();

        internal static QuaverScreen CreateImporting(MultiplayerScreen? multiplayerScreen = null,
            bool fromSelect = false, bool fullSync = false, int? selectMapIdAfterImport = null) =>
            Resolve(nameof(ScreenFactorySet.Importing), Legacy.Importing!, NewScreens.Importing)(
                multiplayerScreen, fromSelect, fullSync, selectMapIdAfterImport);

        internal static QuaverScreen CreateMapLoading(List<Score> scores, Replay? replay = null,
            SpectatorClient? spectatorClient = null) =>
            Resolve(nameof(ScreenFactorySet.MapLoading), Legacy.MapLoading!, NewScreens.MapLoading)(
                scores, replay, spectatorClient);

        internal static QuaverScreen CreateMultiplayerGame() =>
            Resolve(nameof(ScreenFactorySet.MultiplayerGame), Legacy.MultiplayerGame!,
                NewScreens.MultiplayerGame)();

        internal static QuaverScreen CreateMultiplayer(MultiplayerGame game,
            bool playTrackOnFirstUpdate = false) =>
            Resolve(nameof(ScreenFactorySet.Multiplayer), Legacy.Multiplayer!,
                NewScreens.Multiplayer)(game, playTrackOnFirstUpdate);

        internal static QuaverScreen CreateResults(GameplayScreen screen) =>
            CreateResults(ResultsScreenContext.FromGameplay(screen));

        internal static QuaverScreen CreateResults(GameplayScreen screen, MultiplayerGame game,
            List<ScoreProcessor> team1, List<ScoreProcessor> team2) =>
            CreateResults(ResultsScreenContext.FromMultiplayerGameplay(screen, game, team1, team2));

        internal static QuaverScreen CreateResults(Map map, MultiplayerGame game, Score score,
            List<ScoreProcessor> team1, List<ScoreProcessor> team2) =>
            CreateResults(ResultsScreenContext.FromMultiplayerScore(map, game, score, team1, team2));

        internal static QuaverScreen CreateResults(Map map, Score score) =>
            CreateResults(ResultsScreenContext.FromScore(map, score));

        internal static QuaverScreen CreateResults(Map map, Replay replay) =>
            CreateResults(ResultsScreenContext.FromReplay(map, replay));

        private static QuaverScreen CreateResults(ResultsScreenContext context) =>
            Resolve(nameof(ScreenFactorySet.Results), Legacy.Results!, NewScreens.Results)(context);

        private static QuaverScreen CreateLegacyResults(ResultsScreenContext context)
        {
            switch (context.Type)
            {
                case ResultsScreenContextType.Gameplay:
                    return new ResultsScreen(context.Gameplay);
                case ResultsScreenContextType.MultiplayerGameplay:
                    return new ResultsScreen(context.Gameplay, context.MultiplayerGame,
                        context.Team1, context.Team2);
                case ResultsScreenContextType.MultiplayerScore:
                    return new ResultsScreen(context.Map, context.MultiplayerGame, context.Score,
                        context.Team1, context.Team2);
                case ResultsScreenContextType.Score:
                    return new ResultsScreen(context.Map, context.Score);
                case ResultsScreenContextType.Replay:
                    return new ResultsScreen(context.Map, context.Replay);
                default:
                    throw new ArgumentOutOfRangeException(nameof(context.Type), context.Type, null);
            }
        }

        private static T Resolve<T>(string screen, T legacy, T? replacement) where T : Delegate
        {
            if (!UseNewScreens)
                return legacy;

            if (replacement != null)
            {
                Logger.Debug($"New screens resolved `{screen}` to its replacement implementation.", LogType.Runtime);
                return replacement;
            }

            Logger.Debug($"New screens has no `{screen}` implementation; falling back to legacy.", LogType.Runtime);
            return legacy;
        }
    }
}
