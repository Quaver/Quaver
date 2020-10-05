using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentDifficultyName : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        public TournamentDifficultyName(Qua qua, TournamentSettingsDifficultyRating settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            base.UpdateState();

            var difficulty = Qua.SolveDifficulty();

            var settings = (TournamentSettingsDifficultyRating) Settings;

            if (settings.UseDefaultColor.Value)
                Tint = ColorHelper.DifficultyToColor(difficulty.OverallDifficulty);

            Text = Qua.DifficultyName;
        }
    }
}