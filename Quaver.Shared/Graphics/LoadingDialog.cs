using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Graphics
{
    public class LoadingDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="header"></param>
        /// <param name="confirmationText"></param>
        public LoadingDialog(string header, string confirmationText, Action loadAction) : base(header, confirmationText)
        {
            // No need for these buttons, so they can be safely disposed of
            YesButton.Destroy();
            NoButton.Destroy();

            LoadingWheel = new LoadingWheel()
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(60, 60),
                Y = -40
            };

            ThreadScheduler.Run(() =>
            {
                loadAction();
                Close();
            });
        }

        /// <summary>
        ///     Override and don't call base.HandleInput() because we don't want the user to
        ///     press escape to exit the dialog during this time
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
        }
    }
}