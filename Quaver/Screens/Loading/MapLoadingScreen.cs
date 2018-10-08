using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Database.Scores;
using Quaver.Server.Common.Objects;
using Wobble.Audio.Tracks;
using Wobble.Screens;

namespace Quaver.Screens.Loading
{
    public class MapLoadingScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Loading;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The local scores from the leaderboard that'll be used during gameplay.
        /// </summary>
        private List<LocalScore> Scores { get; }

        /// <summary>
        /// </summary>
        public MapLoadingScreen(List<LocalScore> scores)
        {
            Scores = scores;
            View = new MapLoadingScreenView(this);
            AudioTrack.AllowPlayback = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}
