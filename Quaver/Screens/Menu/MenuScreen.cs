using Microsoft.Xna.Framework;
using osu_database_reader;
using Quaver.Screens.Menu.UI.Dialogs;
using Wobble.Discord;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Menu
{
    public class MenuScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public MenuScreen()
        {
            DiscordManager.Client.CurrentPresence.Details = "Idle";
            DiscordManager.Client.CurrentPresence.State = "In the menus";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);

            View = new MenuScreenView(this);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles all the input for this screen.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count > 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Show(new QuitDialog());
        }
    }
}