using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Add
{
    public class EditorActionSetColors : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.SetColor;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// Used to derive colors from the original HitObjectInfo
        /// </summary>
        private List<HitObjectInfo> OriginalHitObjects { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        private List<int> Colors { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObjects"></param>
        /// <param name="color"></param>
        public EditorActionSetColors(EditorActionManager manager, List<HitObjectInfo> hitObjects, List<int> colors)
        {
            ActionManager = manager;
            HitObjects = hitObjects;
            OriginalHitObjects = hitObjects;
            Colors = colors;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            for (var i = 0; i < Colors.Count; i++)
                HitObjects[i].Color = Colors[i];

            ActionManager.TriggerEvent(EditorActionType.SetColor, new EditorColorSetEventArgs(HitObjects, 0));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionSetColor(ActionManager, HitObjects, 0).Perform();
    }
}