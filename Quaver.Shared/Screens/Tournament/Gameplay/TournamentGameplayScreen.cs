using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay;

namespace Quaver.Shared.Screens.Tournament.Gameplay
{
    public class TournamentGameplayScreen : GameplayScreen
    {
        /// <summary>
        /// </summary>
        public TournamentScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Creating a tournament gameplay screen from a pre-made replay
        /// </summary>
        /// <param name="map"></param>
        /// <param name="md5"></param>
        /// <param name="replay"></param>m&gt;
        public TournamentGameplayScreen(Qua map, string md5, Replay replay) : base(map, md5, new List<Score>(), replay)
        {
            Type = TournamentScreenType.Replay;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creating a tournament gameplay screen from a SpectatorClient (streaming)
        /// </summary>
        /// <param name="map"></param>
        /// <param name="md5"></param>
        /// <param name="spectatorClient"></param>
        public TournamentGameplayScreen(Qua map, string md5, SpectatorClient spectatorClient) : base(map, md5,
            new List<Score>(), null, false, 0, false, spectatorClient)
        {
            Type = TournamentScreenType.Spectator;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creating a playable gameplay tournament screen (Co-Op Mode)
        /// </summary>
        /// <param name="map"></param>
        /// <param name="md5"></param>
        /// <param name="options"></param>
        public TournamentGameplayScreen(Qua map, string md5, TournamentPlayerOptions options) : base(map, md5, new List<Score>(),
            null, false, 0, false, null, options)
        {
            Type = TournamentScreenType.Coop;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime) => Ruleset?.HandleInput(gameTime);
    }
}