using System.Collections.Generic;
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
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        private List<int> OriginalHitObjectColors { get; }

        /// <summary>
        /// </summary>
        private int Color { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObjects"></param>
        /// <param name="colorIndex"></param>
        public EditorActionSetColor(EditorActionManager manager, List<HitObjectInfo> hitObjects, List<int> originalHitObjectColors, int colorIndex)
        {
            ActionManager = manager;
            HitObjects = hitObjects;
            OriginalHitObjectColors = originalHitObjectColors;
            Color = colorIndex;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            if (OriginalHitObjectColors.Count > 0)
                for (var i = 0; i < HitObjects.Count; i++)
                    HitObjects[i].Color = OriginalHitObjectColors[i];
            else
            {
                foreach (var ho in HitObjects)
                {
                    OriginalHitObjectColors.Add(ho.Color);
                    ho.Color = Color;
                }
            }

            ActionManager.TriggerEvent(EditorActionType.SetColor, new EditorColorSetEventArgs(HitObjects, Color));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionSetColor(ActionManager, HitObjects, OriginalHitObjectColors, Color).Perform();
    }
}