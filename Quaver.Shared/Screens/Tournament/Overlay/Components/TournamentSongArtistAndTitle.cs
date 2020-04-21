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

        private Bindable<bool> ShouldDisplay { get; }

        private BindableInt TextSize { get; }

        private Bindable<Vector2> TextPosition { get; }

        private Bindable<Alignment> Aligning { get; }

        private Bindable<Color> Color { get; }

        public TournamentSongArtistAndTitle(Qua qua, Bindable<bool> shouldDisplay, BindableInt textSize, Bindable<Vector2> position,
            Bindable<Alignment> alignment, Bindable<Color> col)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
        {
            Qua = qua;
            ShouldDisplay = shouldDisplay;
            TextSize = textSize;
            TextPosition = position;
            Aligning = alignment;
            Color = col;

            SetText();

            ShouldDisplay.ValueChanged += (sender, args) => SetText();
            TextSize.ValueChanged += (sender, args) => SetText();
            TextPosition.ValueChanged += (sender, args) => SetText();
            Aligning.ValueChanged += (sender, args) => SetText();
            Color.ValueChanged += (sender, args) => SetText();
        }

        /// <summary>
        /// </summary>
        private void SetText()
        {
            ScheduleUpdate(() =>
            {
                FontSize = TextSize.Value;
                Text = $"{Qua.Artist} - {Qua.Title}";
                Visible = ShouldDisplay.Value;
                Position = new ScalableVector2(TextPosition.Value.X, TextPosition.Value.Y);
                Alignment = Aligning.Value;
                Tint = Color.Value;
            });
        }
    }
}