using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create
{
    public class CreatePlaylistDialog : DialogScreen
    {
        /// <summary>
        ///     The time it takes to fade in/out the dialog
        /// </summary>
        private const int FadeTime = 150;

        /// <summary>
        /// </summary>
        private CreatePlaylistContainer CreatePlaylistContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CreatePlaylistDialog() : base(0)
        {
            FadeTo(0.85f, Easing.Linear, FadeTime);
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreatePlaylistContainer = new CreatePlaylistContainer(this)
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
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) && DialogManager.Dialogs.First() == this)
            {
                CreatePlaylistContainer.Close();
            }
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            FadeTo(0, Easing.Linear, FadeTime);
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), FadeTime);
        }
    }
}