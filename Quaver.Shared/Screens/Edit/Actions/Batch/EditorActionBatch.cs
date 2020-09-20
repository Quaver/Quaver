using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Edit.Actions.Batch
{
    public class EditorActionBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.Batch;

        private EditorActionManager ActionManager { get; }

        private List<IEditorAction> EditorActions { get; }

        public EditorActionBatch(EditorActionManager manager, List<IEditorAction> actions)
        {
            ActionManager = manager;
            EditorActions = actions;
        }

        public void Perform() => EditorActions.ForEach(x => x.Perform());

        public void Undo()
        {
            for (int i = EditorActions.Count - 1; i >= 0; i--)
                EditorActions[i].Undo();
        }
    }
}
