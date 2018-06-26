using Quaver.API.Enums;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.States.Edit.UI.Components.Playfield;

namespace Quaver.States.Edit.UI.Modes.Keys.Playfield
{
    internal class EditorKeysPlayfield : IEditorPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Container Container { get; }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public GameMode Mode { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
        }
    }
}