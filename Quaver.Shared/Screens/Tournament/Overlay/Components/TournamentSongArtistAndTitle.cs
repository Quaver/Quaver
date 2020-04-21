using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentSongArtistAndTitle : SpriteTextPlus
    {
        private Qua Qua { get; }

        private TournamentDrawableSettings Settings { get; }

        public TournamentSongArtistAndTitle(Qua qua, TournamentDrawableSettings settings)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
        {
            Qua = qua;
            Settings = settings;

            SetText();

            Settings.Visible.ValueChanged += (sender, args) => SetText();
            Settings.FontSize.ValueChanged += (sender, args) => SetText();
            Settings.Position.ValueChanged += (sender, args) => SetText();
            Settings.Alignment.ValueChanged += (sender, args) => SetText();
            Settings.Tint.ValueChanged += (sender, args) => SetText();
        }

        /// <summary>
        /// </summary>
        private void SetText()
        {
            ScheduleUpdate(() =>
            {
                FontSize = Settings.FontSize.Value;
                Text = $"{Qua.Artist} - {Qua.Title}";
                Visible = Settings.Visible.Value;
                Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);
                Alignment = Settings.Alignment.Value;
                Tint = Settings.Tint.Value;
            });
        }
    }
}