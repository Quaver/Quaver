using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Window;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
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
            new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex(),
                0, 590))
        {
            Dropdown.ItemContainer.Scrollbar.Tint = Color.White;
            Dropdown.ItemContainer.Scrollbar.Width = 2;
            Dropdown.SelectedText.Text = Resolution;

            Dropdown.ItemContainer.EasingType = Easing.OutQuint;
            Dropdown.ItemContainer.TimeToCompleteScroll = 1200;
            Dropdown.ItemContainer.ScrollSpeed = 220;

            Dropdown.ItemSelected += (sender, args) =>
            {
                if (!QuaverWindowManager.CanChangeResolutionOnScene)
                {
                    Dropdown.SelectedIndex = GetSelectedIndex();
                    Dropdown.SelectedText.Text = Dropdown.Items[Dropdown.SelectedIndex].Text.Text;

                    NotificationManager.Show(NotificationLevel.Warning, "You cannot change resolutions while on this screen!");
                    return;
                }

                var split = args.Item.Text.Text.Split("x");

                if (ConfigManager.WindowWidth != null)
                {
                    ConfigManager.WindowWidth.Value = int.Parse(split[0]);
                    ConfigManager.WindowHeight.Value = int.Parse(split[1]);
                }

                var game = GameBase.Game as QuaverGame;
                game?.ChangeResolution();
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Dropdown.ItemContainer.Scrollbar.Visible = Dropdown.Opened;
            Dropdown.ItemContainer.InputEnabled = Dropdown.Opened;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>
            {
                "640x360",
                "1024x576",
                "1152x648"
            };

            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                var option = $"{mode.Width}x{mode.Height}";

                if (!options.Contains(option))
                    options.Add(option);
            }

            options = options.OrderBy(x => int.Parse(x.Split("x")[0])).ToList();
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