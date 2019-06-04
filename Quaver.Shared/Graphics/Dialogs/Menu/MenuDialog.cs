using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
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

        /// <summary>
        /// </summary>
        private string Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuDialog(string name, List<IMenuDialogOption> options) : base(0.85f)
        {
            Name = name;
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

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextBitmap(FontsBitmap.GothamRegular, Name)
            {
                Parent = DialogBox,
                Y = -25,
                FontSize = 16,
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