using Quaver.GameState;

namespace Quaver.States
{
    internal interface IGameStateComponent
    {
        void Initialize(IGameState state);
        void UnloadContent();
        void Update(double dt);
        void Draw();
    }
}
