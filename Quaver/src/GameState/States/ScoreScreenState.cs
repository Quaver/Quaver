using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.GameState.States
{
    class ScoreScreenState : IGameState
    {
        public State CurrentState { get; set; } = State.ScoreScreen;
        public bool UpdateReady { get; set; }

        public void Initialize() { }

        public void LoadContent() { }

        public void UnloadContent() { }

        public void Update(GameTime gameTime) { }

        public void Draw() { }
    }
}
