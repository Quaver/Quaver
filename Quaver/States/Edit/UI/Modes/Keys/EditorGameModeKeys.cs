using Quaver.API.Enums;
using Quaver.States.Edit.UI.Components.Playfield;

namespace Quaver.States.Edit.UI.Modes.Keys
{
    internal class EditorGameModeKeys : EditorGameMode
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="mode"></param>
        public EditorGameModeKeys(EditorScreen screen, GameMode mode) : base(screen, mode)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override IEditorPlayfield CreatePlayfield() => new EditorPlayfieldKeys(Screen, Mode);
    }
}