using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState
{
    internal interface IHelper
    {
        void Initialize(IGameState state);
        void UnloadContent();
        void Update(double dt);
        void Draw();
    }
}
