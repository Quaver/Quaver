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
    ///     Creates menu screens from either the legacy implementation set or the partial v2 set.
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

        private static readonly ScreenFactorySet V2 = V2ScreenRegistry.CreateFactorySet();

        /// <summary>
        ///     The startup snapshot of the v2 configuration value.
        /// </summary>
        private static bool UseV2 { get; set; }

        internal static void Initialize(bool useV2) => UseV2 = useV2;

        internal static QuaverScreen CreateMainMenu() =>
            Resolve(nameof(ScreenFactorySet.MainMenu), Legacy.MainMenu!, V2.MainMenu)();

        internal static QuaverScreen CreateSelection(
            SelectScrollContainerType? activeScrollContainer = null,
            SelectContainerPanel activeLeftPanel = SelectContainerPanel.Leaderboard) =>
            Resolve(nameof(ScreenFactorySet.Selection), Legacy.Selection!, V2.Selection)(activeScrollContainer, activeLeftPanel);

        internal static QuaverScreen CreateDownloading(QuaverScreenType previousScreen = QuaverScreenType.Menu) =>
            Resolve(nameof(ScreenFactorySet.Downloading), Legacy.Downloading!, V2.Downloading)(previousScreen);

        internal static QuaverScreen CreateMultiplayerLobby() =>
            Resolve(nameof(ScreenFactorySet.MultiplayerLobby), Legacy.MultiplayerLobby!, V2.MultiplayerLobby)();

        internal static QuaverScreen CreateMusicPlayer() =>
            Resolve(nameof(ScreenFactorySet.MusicPlayer), Legacy.MusicPlayer!, V2.MusicPlayer)();

        internal static QuaverScreen CreateTheater() =>
            Resolve(nameof(ScreenFactorySet.Theater), Legacy.Theater!, V2.Theater)();

        private static T Resolve<T>(string screen, T legacy, T? v2) where T : Delegate
        {
            if (!UseV2)
                return legacy;

            if (v2 != null)
            {
                Logger.Debug($"Screens v2 resolved `{screen}` to its v2 implementation.", LogType.Runtime);
                return v2;
            }

            Logger.Debug($"Screens v2 has no `{screen}` implementation; falling back to legacy.", LogType.Runtime);
            return legacy;
        }
    }
}
