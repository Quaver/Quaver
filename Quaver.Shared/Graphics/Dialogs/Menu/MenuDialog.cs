using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Dialogs.Menu
{
    public class MenuDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private MenuDialogBox DialogBox { get; set; }

        /// <summary>
        /// </summary>
        public List<IMenuDialogOption> Options { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuDialog(List<IMenuDialogOption> options) : base(0.85f)
        {
            Options = options;
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            DialogBox = new MenuDialogBox(this)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }
    }
}