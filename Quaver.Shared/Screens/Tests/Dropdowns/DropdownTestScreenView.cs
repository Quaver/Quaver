using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Dropdowns
{
    public class DropdownTestScreenView : ScreenView
    {
        private Dropdown Dropdown { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public DropdownTestScreenView(Screen screen) : base(screen)
        {
            Dropdown = new Dropdown(new List<string>
            {
                "Artist",
                "Title",
                "BPM",
                "Creator",
                "Difficulty",
                "Mode",
                "Length"
            }, new ScalableVector2(200, 45), 22)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            Dropdown.ItemSelected += (sender, args) =>
            {
                Logger.Important($"{args.Text} [{args.Index}] was clicked in the dropdown!", LogType.Runtime);
            };

            // ReSharper disable once ObjectCreationAsStatement
            new LabelledDropdown("Favorite Foods:", 22, new Dropdown(new List<string>()
            {
                "Pizza",
                "Chicken",
                "Ice Cream",
                "Pasta"
            }, new ScalableVector2(200, 40), 22))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                X = 400
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
            {
                if (Dropdown.Opened)
                    Dropdown.Close();
                else
                    Dropdown.Open();
            }

            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}