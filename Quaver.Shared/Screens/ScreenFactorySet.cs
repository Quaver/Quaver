using System;
using System.Collections.Generic;
using Quaver.API.Replays;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;

namespace Quaver.Shared.Screens
{
    /// <summary>
    ///     Constructors for screens which can be replaced by new implementations.
    ///     Null entries in the replacement set intentionally fall back to their legacy counterpart.
    /// </summary>
    internal sealed class ScreenFactorySet
    {
        internal Func<QuaverScreen>? Initialization { get; init; }

        internal Func<QuaverScreen>? MainMenu { get; init; }

        internal Func<SelectScrollContainerType?, SelectContainerPanel, QuaverScreen>? Selection { get; init; }

        internal Func<QuaverScreenType, QuaverScreen>? Downloading { get; init; }

        internal Func<QuaverScreen>? MultiplayerLobby { get; init; }

        internal Func<QuaverScreen>? MusicPlayer { get; init; }

        internal Func<QuaverScreen>? Theater { get; init; }

        internal Func<MultiplayerScreen?, bool, bool, int?, QuaverScreen>? Importing { get; init; }

        internal Func<List<Score>, Replay?, SpectatorClient?, QuaverScreen>? MapLoading { get; init; }

        internal Func<QuaverScreen>? MultiplayerGame { get; init; }

        internal Func<MultiplayerGame, bool, QuaverScreen>? Multiplayer { get; init; }

        internal Func<ResultsScreenContext, QuaverScreen>? Results { get; init; }
    }
}
