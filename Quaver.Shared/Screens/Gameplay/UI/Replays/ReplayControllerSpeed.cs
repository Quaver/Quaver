using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.UI.Replays
{
    public class ReplayControllerSpeed : Sprite
    {
        /// <summary>
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Rate { get; set; }

        /// <summary>
        /// </summary>
        private Slider Slider { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="size"></param>
        public ReplayControllerSpeed(GameplayScreen screen, ScalableVector2 size)
        {
            Screen = screen;

            Tint = ColorHelper.HexToColor("#181818");
            Size = size;

            CreateRate();
            CreateSlider();
            AddBorder(Colors.MainAccent, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateRate()
        {
            var value = 100;

            if (AudioEngine.Track != null)
                value = (int) (AudioEngine.Track.Rate * 100);

            Rate = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{value / 100f:0.00}x", 20)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -10
            };
        }

        private void CreateSlider()
        {
            var value = 100;

            if (AudioEngine.Track != null)
                value = (int) (AudioEngine.Track.Rate * 100);

            Slider = new Slider(new BindableInt(value, 10, 200),
                new Vector2(Width * 0.70f, 4),
                FontAwesome.Get(FontAwesomeIcon.fa_circle))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = -Rate.X,
                ActiveColor =
                {
                    Tint = Colors.MainAccent
                },
                ProgressBall =
                {
                    Tint = Colors.MainAccent
                }
            };

            Slider.BindedValue.ValueChanged += (sender, args) =>
            {
                var rate = args.Value / 100f;
                Rate.Text = $"{rate:0.00}x";

                if (AudioEngine.Track != null)
                    AudioEngine.Track.Rate = rate;

                Screen?.HandleReplaySeeking(AudioEngine.Track?.Time ?? 0);
            };
        }
    }
}