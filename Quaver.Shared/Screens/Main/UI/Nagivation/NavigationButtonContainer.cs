using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Main.UI.Nagivation
{
    public class NavigationButtonContainer : Container
    {
        private List<NavigationButton> Buttons { get; }

        private const int SPACING_Y = 16;

        private int SelectedIndex { get; set; }

        public NavigationButtonContainer(List<NavigationButton> buttons)
        {
            Buttons = buttons;
            AlignButtons();
        }

        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen is {Exiting: true})
                return;

            if (DialogManager.Dialogs.Count != 0 || KeyboardManager.IsAltDown())
                return;

            HandleKeyPressUp();
            HandleKeyPressEnter();
        }

        private void HandleKeyPressUp()
        {
            var selected = Buttons[SelectedIndex];
            NavigationButton newButton = null;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                var index = SelectedIndex - 1;

                if (index < 0)
                    return;

                newButton = Buttons[index];
                SelectedIndex = index;
            }
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                var index = SelectedIndex + 1;

                if (index == Buttons.Count)
                    return;

                newButton = Buttons[index];
                SelectedIndex = index;
            }

            if (newButton == null)
                return;

            selected.Deselect();
            newButton.Select();
        }

        private void HandleKeyPressEnter()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                return;

            Buttons[SelectedIndex].FireButtonClickEvent();
        }

        private void AlignButtons()
        {
            var totalY = 0f;

            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                btn.Parent = this;
                btn.X = -X - btn.Width - 50;

                if (i == 0)
                    btn.Select(true);

                btn.MoveToX(0, Easing.OutQuint, 450 + 50 * (i + 1));

                if (i == 0)
                {
                    totalY += btn.Height;
                    totalY += SPACING_Y;
                    continue;
                }

                btn.Y = totalY;

                totalY += btn.Height;

                if (i != Buttons.Count - 1)
                    totalY += SPACING_Y;
            }

            Size = new ScalableVector2(Buttons.First().Width, totalY);
        }

        public void Exit()
        {
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                btn.MoveToX(-X - btn.Width - 50, Easing.OutQuint, 450 + 50 * (i + 1));
            }
        }
    }
}