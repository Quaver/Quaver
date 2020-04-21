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
        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private TournamentPlayer Player { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> DisplayWinCounts { get; }

        /// <summary>
        /// </summary>
        private BindableInt TextSize { get; }

        /// <summary>
        /// </summary>
        private Bindable<Vector2> TextPosition { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="displayWinCounts"></param>
        /// <param name="textSize"></param>
        /// <param name="position"></param>
        public TournamentPlayerWinCount(MultiplayerGame game, TournamentPlayer player, Bindable<bool> displayWinCounts,
            BindableInt textSize, Bindable<Vector2> position) : base(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "0", 22)
        {
            Game = game;
            Player = player;
            DisplayWinCounts = displayWinCounts;
            TextSize = textSize;
            TextPosition = position;

            SetText();
            DisplayWinCounts.ValueChanged += (sender, args) => SetText();
            TextSize.ValueChanged += (sender, args) => SetText();
            TextPosition.ValueChanged += (sender, args) => SetText();
        }

        /// <summary>
        /// </summary>
        private void SetText()
        {
            var wins = Game.PlayerWins?.Find(x => x.UserId == Player.User.OnlineUser.Id)?.Wins ?? 0;

            ScheduleUpdate(() =>
            {
                FontSize = TextSize.Value;
                Text = $"{wins:n0}";
                Visible = DisplayWinCounts.Value;
                Position = new ScalableVector2(TextPosition.Value.X, TextPosition.Value.Y);
            });
        }
    }
}