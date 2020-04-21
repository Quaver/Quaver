using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentPlayerWinCount : SpriteTextPlus
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
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "0", 22)
        {
            Game = game;
            Player = player;
            Settings = settings;

            SetText();

            Settings.Visible.ValueChanged += (sender, args) => SetText();
            Settings.FontSize.ValueChanged += (sender, args) => SetText();
            Settings.Position.ValueChanged += (sender, args) => SetText();
            Settings.Alignment.ValueChanged += (sender, args) => SetText();
            Settings.Tint.ValueChanged += (sender, args) => SetText();
        }

        /// <summary>
        /// </summary>
        private void SetText()
        {
            var wins = Game.PlayerWins?.Find(x => x.UserId == Player.User.OnlineUser.Id)?.Wins ?? 0;

            ScheduleUpdate(() =>
            {
                FontSize = Settings.FontSize.Value;
                Text = $"{wins:n0}";
                Visible = Settings.Visible.Value;
                Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);
                Alignment = Settings.Alignment.Value;
                Tint = Settings.Tint.Value;
            });
        }
    }
}