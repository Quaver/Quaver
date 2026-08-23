namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Explicit registration point for completed replacement screens.
    /// </summary>
    internal static class NewScreenRegistry
    {
        internal static ScreenFactorySet CreateFactorySet() => new()
        {
#if DEBUG
            MainMenu = () => new Main.MainMenuScreen(),
            Initialization = () => new Initialization.InitializationScreen(),
            Importing = (multiplayerScreen, fromSelect, fullSync, selectMapIdAfterImport) =>
                new Importing.ImportingScreen(multiplayerScreen, fromSelect, fullSync, selectMapIdAfterImport),
            MapLoading = (scores, replay, spectatorClient) =>
                new Loading.MapLoadingScreen(scores, replay, spectatorClient),
            Selection = (activeScrollContainer, activeLeftPanel) =>
                new Selection.SelectionScreen(activeScrollContainer, activeLeftPanel),
            Downloading = previousScreen => new Downloading.DownloadingScreen(previousScreen),
            MultiplayerLobby = () => new MultiplayerLobby.MultiplayerLobbyScreen(),
            MultiplayerGame = () => new Multi.MultiplayerGameScreen(),
            Multiplayer = (game, playTrackOnFirstUpdate) =>
                new Multiplayer.MultiplayerScreen(game, playTrackOnFirstUpdate),
            MusicPlayer = () => new Music.MusicPlayerScreen(),
            Theater = () => new Theater.TheaterScreen(),
            Results = context => new Results.ResultsScreen(context)
#else
            MainMenu = () => new Main.MainMenuScreen(),
#endif
        };
    }
}