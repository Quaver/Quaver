using System.Collections.Generic;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentPlayerAccuracy : TournamentPlayerScoreValue
    {
        public TournamentPlayerAccuracy(TournamentDrawableSettings settings, TournamentPlayer player, List<TournamentPlayer> players) 
            : base(settings, player, players)
        {
            SetText();
        }

        protected override void SetValue() => Text = StringHelper.AccuracyToString(Player.Scoring.Accuracy);
    }
}