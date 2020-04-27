using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public sealed class TournamentSongArtistAndTitle : TournamentOverlaySpriteText
    {
        private Qua Qua { get; }
        
        public TournamentSongArtistAndTitle(Qua qua, TournamentDrawableSettings settings) : base(settings)
        {
            Qua = qua;
            SetText();
        }

        public override void UpdateState()
        {
            Text = $"{Qua.Artist} - {Qua.Title}";
            base.UpdateState();
        }
    }
}