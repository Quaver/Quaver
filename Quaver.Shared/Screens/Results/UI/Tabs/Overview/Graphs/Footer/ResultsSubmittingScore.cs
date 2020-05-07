using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Footer
{
    public class ResultsSubmittingScore : Container
    {
        private SpriteTextPlus TextSubmitting { get; }

        private LoadingWheel Wheel { get; }

        public ResultsSubmittingScore(int fontSize)
        {
            TextSubmitting = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "SUBMITTING SCORE...", fontSize)
            {
                Parent = this
            };

            const int spacing = 12;

            Wheel = new LoadingWheel
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = TextSubmitting.Width + spacing,
                Size = new ScalableVector2(fontSize, fontSize)
            };

            Size = new ScalableVector2( TextSubmitting.Width + spacing + Wheel.Width,
                Math.Max(TextSubmitting.Height, Wheel.Height));
        }

        public void FadeOut(int time = 250)
        {
            TextSubmitting.FadeTo(0, Easing.Linear, time);
            Wheel.FadeTo(0, Easing.Linear, time);
        }
    }
}