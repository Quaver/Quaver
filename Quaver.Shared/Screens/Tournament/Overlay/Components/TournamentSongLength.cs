using System;
using Quaver.API.Maps;
using TagLib.Matroska;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentSongLength : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }

        public TournamentSongLength(Qua qua, TournamentDrawableSettings settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            Text = $"{TimeSpan.FromMilliseconds(Qua.Length):mm\\:ss}";
            base.UpdateState();
        }
    }
}