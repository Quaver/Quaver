using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Colors.Add
{
    public class EditorActionSetColors : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.SetColorBatch;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// Used to derive colors from the original HitObjectInfo
        /// </summary>
        private List<SnapColor> OriginalHitObjectColors { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        private List<SnapColor> Colors { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObjects"></param>
        /// <param name="color"></param>
        public EditorActionSetColors(EditorActionManager manager, List<HitObjectInfo> hitObjects, List<SnapColor> colors)
        {
            ActionManager = manager;
            HitObjects = hitObjects;
            OriginalHitObjectColors = HitObjects.Select(x => (SnapColor)x.Color).ToList();
            Colors = colors;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            for (var i = 0; i < Colors.Count; i++)
                HitObjects[i].Color = (int)Colors[i];

            ActionManager.TriggerEvent(EditorActionType.SetColorBatch, new EditorColorSetEventArgs(HitObjects, OriginalHitObjectColors));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionSetColors(ActionManager, HitObjects, OriginalHitObjectColors).Perform();
    }
}