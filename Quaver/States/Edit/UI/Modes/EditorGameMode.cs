using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.GameState;

namespace Quaver.States.Edit.UI.Modes
{
    internal abstract class EditorGameMode : IGameStateComponent
    {
        /// <summary>
        ///     The game mode the editor is for.
        /// </summary>
        internal GameMode Mode { get; }

        /// <summary>
        ///     Reference to the editor screen itself.
        /// </summary>
        internal EditorScreen Screen { get; }

        /// <summary>
        ///     The editor playfield itself.
        /// </summary>
        internal IEditorPlayfield Playfield { get; private set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="mode"></param>
        internal EditorGameMode(EditorScreen screen, GameMode mode)
        {
            Screen = screen;
            Mode = mode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Playfield = CreatePlayfield();
            Playfield.Initialize(state);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
            Playfield.UnloadContent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            Playfield.Update(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            Playfield.Draw();
        }

        /// <summary>
        ///     Creates the editor playfield.
        /// </summary>
        /// <returns></returns>
        public abstract IEditorPlayfield CreatePlayfield();
    }
}