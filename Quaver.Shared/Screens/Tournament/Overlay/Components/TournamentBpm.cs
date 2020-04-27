using System;
using Quaver.API.Maps;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentBpm : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        public TournamentBpm(Qua qua, TournamentDrawableSettings settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            Text = $"{Qua.GetCommonBpm():n0}";
            base.UpdateState();
        }
    }
}