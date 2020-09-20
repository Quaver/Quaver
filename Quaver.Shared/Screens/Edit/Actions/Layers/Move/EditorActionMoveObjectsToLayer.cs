using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Move
{
    [MoonSharpUserData]
    public class EditorActionMoveObjectsToLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.MoveToLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private List<HitObjectInfo> HitObjects { get; }

        private List<int> OriginalLayers { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="workingMap"></param>
        /// <param name="layer"></param>
        /// <param name="hitObjects"></param>
        [MoonSharpVisible(false)]
        public EditorActionMoveObjectsToLayer(EditorActionManager manager, Qua workingMap, EditorLayerInfo layer,
            List<HitObjectInfo> hitObjects)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;
            HitObjects = hitObjects;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            OriginalLayers = new List<int>();

            HitObjects.ForEach(x =>
            {
                OriginalLayers.Add(x.EditorLayer);

                if (Layer == null)
                    x.EditorLayer = 0;
                else
                {
                    var index = WorkingMap.EditorLayers.IndexOf(Layer) + 1;
                    x.EditorLayer = index;
                }
            });
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < HitObjects.Count; i++)
                HitObjects[i].EditorLayer = OriginalLayers[i];
        }
    }
}
