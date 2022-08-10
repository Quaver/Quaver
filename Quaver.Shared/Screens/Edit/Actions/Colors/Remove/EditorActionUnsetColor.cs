using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Colors.Add;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Remove
{
    public class EditorActionUnsetColor : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.UnsetColor;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        private int Color { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObjects"></param>
        /// <param name="sound"></param>
        public EditorActionUnsetColor(EditorActionManager manager, List<HitObjectInfo> hitObjects, int colorIndex)
        {
            ActionManager = manager;
            HitObjects = hitObjects;
            Color = colorIndex;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            foreach (var ho in HitObjects)
                ho.Color = 0;

            ActionManager.TriggerEvent(EditorActionType.UnsetColor, new EditorColorUnsetEventArgs(HitObjects, Color));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionSetColor(ActionManager, HitObjects, Color).Perform();
    }
}