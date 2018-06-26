using Quaver.API.Enums;
using Quaver.Graphics.Base;

namespace Quaver.States.Edit.UI.Components.Playfield
{
    internal interface IEditorPlayfield : IGameStateComponent
    {
        /// <summary>
        ///     The actual editor score we're on.
        /// </summary>
        EditorScreen Screen { get; }
        
        /// <summary>
        ///     Container for the entire playfield/
        /// </summary>
        Container Container { get; }
        
        /// <summary>
        ///     The game mode this playfield is for.
        /// </summary>
        GameMode Mode { get; }
    }
}