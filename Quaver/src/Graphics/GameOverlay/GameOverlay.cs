using Quaver.Graphics.GameOverlay.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.GameOverlay
{
    class GameOverlay : IGameOverlayComponent
    {
        internal bool MultiplayerActive { get; set; }

        internal bool OverlayActive { get; set; }

        ChatManager ChatManager { get; set; } = new ChatManager();

        MenuOverlay MenuOverlay { get; set; } = new MenuOverlay();

        public void Initialize()
        {
            ChatManager.Initialize();
            MenuOverlay.Initialize();
            GameBase.GlobalInputManager.GameOverlayToggled += ToggleVisiblity;
        }

        public void UnloadContent()
        {
            ChatManager.UnloadContent();
            MenuOverlay.UnloadContent();
            GameBase.GlobalInputManager.GameOverlayToggled -= ToggleVisiblity;
        }

        public void Draw()
        {
            if (OverlayActive)
            {
                MenuOverlay.Draw();

                if (MultiplayerActive)
                    ChatManager.Draw();
            }
        }

        public void Update(double dt)
        {
            if (OverlayActive)
            {
                MenuOverlay.Update(dt);

                if (MultiplayerActive)
                    ChatManager.Update(dt);
            }
        }

        /// <summary>
        ///     Toggles the overlay's visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleVisiblity(object sender, EventArgs e)
        {
            // Toggle on
            if (!MultiplayerActive)
            {
                MultiplayerActive = true;
            }
            // Toggle off
            else
            {
                MultiplayerActive = false;
            }
        }
    }
}
