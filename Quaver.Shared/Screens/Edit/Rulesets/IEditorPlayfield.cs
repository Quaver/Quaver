using Quaver.API.Enums;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Rulesets
{
    public interface IEditorPlayfield : IGameScreenComponent
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
