using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.GameOverlay
{
    interface IGameOverlayComponent
    {
        void Initialize();
        void UnloadContent();
        void Update(double dt);
        void Draw();
    }
}
