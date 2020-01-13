using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble;
using Wobble.Graphics;
using Wobble.Window;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemScreenResolution : OptionsItemDropdown
    {
        /// <summary>
        /// </summary>
        private static string Resolution => $"{GameBase.Game.Graphics.PreferredBackBufferWidth}x{GameBase.Game.Graphics.PreferredBackBufferHeight}";

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="name"></param>
        public OptionsItemScreenResolution(RectangleF containerRect, string name) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex()))
        {
            Dropdown.SelectedText.Text = Resolution;

            Dropdown.ItemSelected += (sender, args) =>
            {
                var split = args.Item.Text.Text.Split("x");

                if (ConfigManager.WindowWidth != null)
                {
                    ConfigManager.WindowWidth.Value = int.Parse(split[0]);
                    ConfigManager.WindowHeight.Value = int.Parse(split[1]);
                }

                WindowManager.ChangeScreenResolution(new Point(int.Parse(split[0]), int.Parse(split[1])));
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>
            {
                "1024x576",
                "1152x648"
            };

            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.AspectRatio >= 1.7 && mode.AspectRatio <= 1.8)
                {
                    var option = $"{mode.Width}x{mode.Height}";

                    if (!options.Contains(option))
                        options.Add(option);
                }
            }

            return options;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            var index = GetOptions().FindIndex(x => x == Resolution);

            if (index == -1)
                return 0;

            return index;
        }
    }
}