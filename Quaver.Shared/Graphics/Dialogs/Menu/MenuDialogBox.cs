using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Dialogs.Menu
{
    public class MenuDialogBox : Sprite
    {
        /// <summary>
        /// </summary>
        private MenuDialog Dialog { get; }

        /// <summary>
        /// </summary>
        private MenuDialogScrollContainer Container { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public MenuDialogBox(MenuDialog dialog)
        {
            Dialog = dialog;

            Image = UserInterface.PlayerOptionsPanel;
            Size = new ScalableVector2(450, 354);

            CreateContainer();
        }

        /// <summary>
        ///     Creates the ScrollContainer for the options
        /// </summary>
        private void CreateContainer()
        {
            Container = new MenuDialogScrollContainer(Dialog, Dialog.Options)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
        }
    }
}