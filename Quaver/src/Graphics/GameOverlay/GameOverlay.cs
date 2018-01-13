using Quaver.Graphics.GameOverlay.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.GameOverlay
{
    class GameOverlay : IGameOverlayComponent
    {
        bool Active { get; set; } = false;

        ChatManager ChatManager { get; set; } = new ChatManager();

        public void Initialize()
        {
            ChatManager.Initialize();
            GameBase.GlobalInputManager.GameOverlayToggled += ToggleVisiblity;
        }

        public void UnloadContent()
        {
            ChatManager.UnloadContent();
        }

        public void Draw()
        {
            if (Active)
                ChatManager.Draw();
        }

        public void Update(double dt)
        {
            if (Active)
                ChatManager.Update(dt);
        }

        /// <summary>
        ///     Toggles the overlay's visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleVisiblity(object sender, EventArgs e)
        {
            // Toggle on
            if (!Active)
            {
                Active = true;
            }
            // Toggle off
            else
            {
                Active = false;
            }
        }
    }
}
