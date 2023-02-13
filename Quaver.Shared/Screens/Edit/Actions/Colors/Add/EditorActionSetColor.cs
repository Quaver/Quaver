using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

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
        public List<SnapColor> OriginalHitObjectColors { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        private SnapColor Color { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObjects"></param>
        /// <param name="color"></param>
        public EditorActionSetColor(EditorActionManager manager, List<HitObjectInfo> hitObjects, SnapColor color)
        {
            ActionManager = manager;
            HitObjects = hitObjects;
            OriginalHitObjectColors = HitObjects.Select(x => (SnapColor)x.Color).ToList();
            Color = color;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            foreach (var ho in HitObjects)
                ho.Color = (int)Color;

            ActionManager.TriggerEvent(EditorActionType.SetColor, new EditorColorSetEventArgs(HitObjects, new List<SnapColor> { SnapColor.None }));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionSetColors(ActionManager, HitObjects, OriginalHitObjectColors).Perform();
    }
}