using System;
using Quaver.Shared.Screens.Downloading;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Theater;
using Quaver.Shared.Screens.V2;
using Wobble.Logging;

namespace Quaver.Shared.Screens
{
    /// <summary>
    ///     Creates menu screens from either the legacy implementation set or the replacement set.
    /// </summary>
    internal static class QuaverScreenFactory
    {
        private static readonly ScreenFactorySet Legacy = new()
        {
            MainMenu = () => new MainMenuScreen(),
            Selection = (activeScrollContainer, activeLeftPanel) =>
                new SelectionScreen(activeScrollContainer, activeLeftPanel),
            Downloading = previousScreen => new DownloadingScreen(previousScreen),
            MultiplayerLobby = () => new MultiplayerLobbyScreen(),
            MusicPlayer = () => new MusicPlayerScreen(),
            Theater = () => new TheaterScreen()
        };

        private static readonly ScreenFactorySet NewScreens = NewScreenRegistry.CreateFactorySet();

        /// <summary>
        ///     The startup snapshot of the replacement-screen configuration value.
        /// </summary>
        private static bool UseNewScreens { get; set; }

        internal static void Initialize(bool useNewScreens) => UseNewScreens = useNewScreens;

        internal static QuaverScreen CreateMainMenu() =>
            Resolve(nameof(ScreenFactorySet.MainMenu), Legacy.MainMenu!, NewScreens.MainMenu)();

        internal static QuaverScreen CreateSelection(
            SelectScrollContainerType? activeScrollContainer = null,
            SelectContainerPanel activeLeftPanel = SelectContainerPanel.Leaderboard) =>
            Resolve(nameof(ScreenFactorySet.Selection), Legacy.Selection!, NewScreens.Selection)(activeScrollContainer, activeLeftPanel);

        internal static QuaverScreen CreateDownloading(QuaverScreenType previousScreen = QuaverScreenType.Menu) =>
            Resolve(nameof(ScreenFactorySet.Downloading), Legacy.Downloading!, NewScreens.Downloading)(previousScreen);

        internal static QuaverScreen CreateMultiplayerLobby() =>
            Resolve(nameof(ScreenFactorySet.MultiplayerLobby), Legacy.MultiplayerLobby!, NewScreens.MultiplayerLobby)();

        internal static QuaverScreen CreateMusicPlayer() =>
            Resolve(nameof(ScreenFactorySet.MusicPlayer), Legacy.MusicPlayer!, NewScreens.MusicPlayer)();

        internal static QuaverScreen CreateTheater() =>
            Resolve(nameof(ScreenFactorySet.Theater), Legacy.Theater!, NewScreens.Theater)();

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
