using System;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;

namespace Quaver.Shared.Screens
{
    /// <summary>
    ///     Constructors for the menu screens which can be replaced by v2 implementations.
    ///     Null entries in the v2 set intentionally fall back to their legacy counterpart.
    /// </summary>
    internal sealed class ScreenFactorySet
    {
        internal Func<QuaverScreen>? MainMenu { get; init; }

        internal Func<SelectScrollContainerType?, SelectContainerPanel, QuaverScreen>? Selection { get; init; }

        internal Func<QuaverScreenType, QuaverScreen>? Downloading { get; init; }

        internal Func<QuaverScreen>? MultiplayerLobby { get; init; }

        internal Func<QuaverScreen>? MusicPlayer { get; init; }

        internal Func<QuaverScreen>? Theater { get; init; }
    }
}
