using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public class TournamentOverlaySpriteText : SpriteTextPlus
    {
        protected TournamentDrawableSettings Settings { get; }

        public TournamentOverlaySpriteText(TournamentDrawableSettings settings) : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "")
        {
            Settings = settings;

            Settings.Visible.ValueChanged += (sender, args) => SetText();
            Settings.Font.ValueChanged += (sender, args) => SetText();
            Settings.FontSize.ValueChanged += (sender, args) => SetText();
            Settings.Position.ValueChanged += (sender, args) => SetText();
            Settings.Alignment.ValueChanged += (sender, args) => SetText();
            Settings.Tint.ValueChanged += (sender, args) => SetText();
            Settings.Inverted.ValueChanged += (sender, args) => SetText();
            Settings.DimWhenLosing.ValueChanged += (sender, args) => SetText();
            Settings.FontSizeWhenLosing.ValueChanged += (sender, args) => SetText();
        }

        public virtual void SetText() => ScheduleUpdate(UpdateState);

        public virtual void UpdateState()
        {
            if (!string.IsNullOrEmpty(Settings.Font.Value) && FontManager.WobbleFonts.ContainsKey(Settings.Font.Value))
                Font = FontManager.GetWobbleFont(Settings.Font.Value);
            else
                Font = FontManager.GetWobbleFont(Settings.Font.Default);

            FontSize = Settings.FontSize.Value;
            Visible = Settings.Visible.Value;
            Position = new ScalableVector2(Settings.Position.Value.X, Settings.Position.Value.Y);
            Alignment = Settings.Alignment.Value;
            Tint = Settings.Tint.Value;
        }
    }
}