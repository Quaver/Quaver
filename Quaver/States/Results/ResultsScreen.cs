using Quaver.GameState;
using Quaver.States.Enums;

namespace Quaver.States.Results
{
    internal class ResultsScreen : IGameState
    {
        public State CurrentState { get; set; } = State.Results;
        public bool UpdateReady { get; set; }
        
        public void Initialize()
        {
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            throw new System.NotImplementedException();
        }

        public void Update(double dt)
        {
            throw new System.NotImplementedException();
        }

        public void Draw()
        {
            throw new System.NotImplementedException();
        }
    }
}