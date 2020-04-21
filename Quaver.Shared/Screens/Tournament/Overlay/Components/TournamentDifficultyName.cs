using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentDifficultyName : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        public TournamentDifficultyName(Qua qua, TournamentDrawableSettings settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            Text = Qua.DifficultyName;
            base.UpdateState();
        }
    }
}