using System;
using Quaver.API.Maps;
using TagLib.Matroska;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentSongLength : TournamentOverlaySpriteText
    {
        private int Length { get; }

        public TournamentSongLength(Qua qua, TournamentDrawableSettings settings) : base(settings)
        {
            Length = qua.Length;
            SetText();
        }

        public override void UpdateState()
        {
            Text = $"{TimeSpan.FromMilliseconds(Length):mm\\:ss}";
            base.UpdateState();
        }
    }
}