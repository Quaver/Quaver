using System.Collections.Generic;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentPlayerRating : TournamentPlayerScoreValue
    {
        public TournamentPlayerRating(TournamentDrawableSettings settings, TournamentPlayer player, List<TournamentPlayer> players)
            : base(settings, player, players)
        {
            SetText();
        }

        protected override void SetValue() => Text = StringHelper.RatingToString(Player.Rating.CalculateRating(Player.Scoring));
    }
}