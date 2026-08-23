using System.Collections.Generic;
using Quaver.API.Replays;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.V2.Loading
{
    internal sealed class MapLoadingScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Loading;

        public MapLoadingScreen(List<Score> scores, Replay replay = null,
            SpectatorClient spectatorClient = null) =>
            View = new MapLoadingScreenView(this);

        public override UserClientStatus GetClientStatus() => null;
    }
}
