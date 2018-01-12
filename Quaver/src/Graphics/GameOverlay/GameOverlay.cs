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
    }
}
