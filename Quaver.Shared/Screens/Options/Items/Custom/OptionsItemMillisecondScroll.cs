using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Quaver.Shared.Config;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemMillisecondScroll : OptionsItem
    {
        /// <summary>
        /// </summary>
        protected Slider Slider { get; }

        /// <summary>
        /// </summary>
        protected SpriteTextPlus Value { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="bindedValue"></param>
        /// <param name="valueModifier"></param>
        public OptionsItemMillisecondScroll(RectangleF containerRect, string name, MSScroll keymode)
            : base(containerRect, name)
        {
            var configVal = enumToConfigValue(keymode);

            Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                FontSize = 20,
                UsePreviousSpriteBatchOptions = true,
                X = -Name.X,
                Text = configVal.Value.ToString()
            };

            configVal.ValueChanged += (sender, args) => ScheduleUpdate(() => {
                Value.Text = calculateMSScrollSpeed(configVal.Value).ToString();
            });
        }

        private int calculateMSScrollSpeed(int scrollSpeedQuaver)
        {
            var scrollSpeed = ((float)ConfigManager.WindowHeight.Value / (float)scrollSpeedQuaver)*100;
            Console.WriteLine(scrollSpeed);
            return (int)scrollSpeed;
        }

        private BindableInt enumToConfigValue(MSScroll m)
        {
            var ret = m == MSScroll.Keymode4k ? ConfigManager.ScrollSpeed4K : ConfigManager.ScrollSpeed7K;
            return ret;
        }

        public enum MSScroll
        {
            Keymode4k,
            Keymode7k
        }
    }
}
