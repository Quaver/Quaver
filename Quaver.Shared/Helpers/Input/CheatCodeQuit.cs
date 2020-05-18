using System;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Helpers.Input
{
    public class CheatCodeQuit : CheatCode
    {
        /// <summary>
        /// </summary>
        private YesNoDialog Dialog { get; }

        public override Keys[] Combination { get; } =
        {
            Keys.Up,
            Keys.Up,
            Keys.Down,
            Keys.Down,
            Keys.Left,
            Keys.Right,
            Keys.Left,
            Keys.Right,
            Keys.B,
            Keys.A
        };

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public CheatCodeQuit(YesNoDialog dialog) => Dialog = dialog;

        /// <summary>
        /// </summary>
        protected override void OnActivated()
        {
            ScheduleUpdate(() =>
            {
                Dialog.NoButton.Visible = false;

                Dialog.Header.Text = "CREATE QUAVER";
                Dialog.Confirmation.Text = $"You cannot quit Quaver, only create it.";

                Dialog.YesButton.Image = UserInterface.CreateButton;
                Dialog.YesButton.MoveToX(Dialog.Panel.Width / 2f - Dialog.YesButton.Width / 2f, Easing.OutQuint, 450);
            });
        }
    }
}