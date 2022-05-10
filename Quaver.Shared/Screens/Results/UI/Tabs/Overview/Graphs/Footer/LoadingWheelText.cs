using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Footer
{
    public class LoadingWheelText : Container
    {
        private SpriteTextPlus TextSubmitting { get; }

        private LoadingWheel Wheel { get; }

        public LoadingWheelText(int fontSize, string text)
        {
            TextSubmitting = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                text, fontSize)
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

        public void FadeIn(int time = 250)
        {
            TextSubmitting.FadeTo(1, Easing.Linear, time);
            Wheel.FadeTo(1, Easing.Linear, time);
        }
    }
}