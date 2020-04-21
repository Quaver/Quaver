using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentPlayerWinCount : TournamentOverlaySpriteText
    {
        private MultiplayerGame Game { get; }

        private TournamentPlayer Player { get; }

        private TournamentDrawableSettings Settings { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="settings"></param>
        public TournamentPlayerWinCount(MultiplayerGame game, TournamentPlayer player, TournamentDrawableSettings settings)
            : base(settings)
        {
            Game = game;
            Player = player;
            Settings = settings;

            SetText();
        }

        public override void UpdateState()
        {
            var wins = Game.PlayerWins?.Find(x => x.UserId == Player.User.OnlineUser.Id)?.Wins ?? 0;
            Text = $"{wins:n0}";
            base.UpdateState();
        }
    }
}