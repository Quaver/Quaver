using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.GameState.States;

namespace Quaver.Gameplay.GameplayRendering
{
    internal interface IGameplayRendering
    {
        void Initialize(PlayScreenState playScreen);
        void UnloadContent();
        void Update(double dt);
        void Draw();
    }
}
