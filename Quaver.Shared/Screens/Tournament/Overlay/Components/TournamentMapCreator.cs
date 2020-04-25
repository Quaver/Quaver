using Quaver.API.Maps;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentMapCreator : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        public TournamentMapCreator(Qua qua, TournamentDrawableSettings settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            Text = Qua.Creator;
            base.UpdateState();
        }
    }
}