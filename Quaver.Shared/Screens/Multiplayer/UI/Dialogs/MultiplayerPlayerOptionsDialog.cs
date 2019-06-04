using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer.UI.Dialogs
{
    public class MultiplayerPlayerOptionsDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private OnlineUser User { get; }

        /// <summary>
        /// </summary>
        private MultiplayerPlayerOptions Options { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        public MultiplayerPlayerOptionsDialog(OnlineUser user) : base(0.85f)
        {
            User = user;
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            Options = new MultiplayerPlayerOptions(this, User)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextBitmap(FontsBitmap.GothamRegular,$"Options for {User.Username ?? "Loading..."}")
            {
                Parent = Options,
                Y = -25,
                FontSize = 16,
            };
        }

        public override void Destroy()
        {
            ButtonManager.Remove(this);
            base.Destroy();
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