using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Colors.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Add
{
    public class EditorActionSetColor : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.SetColor;

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
        /// <param name="colorIndex"></param>
        public EditorActionSetColor(EditorActionManager manager, List<HitObjectInfo> hitObjects, int colorIndex)
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
                ho.Color = Color;

            ActionManager.TriggerEvent(EditorActionType.SetColor, new EditorColorSetEventArgs(HitObjects, Color));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionUnsetColor(ActionManager, HitObjects, Color).Perform();
    }
}