using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Batch
{
    [MoonSharpUserData]
    public class EditorActionBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.Batch;

        private EditorActionManager ActionManager { get; }

        private List<IEditorAction> EditorActions { get; }

        [MoonSharpVisible(false)]
        public EditorActionBatch(EditorActionManager manager, List<IEditorAction> actions)
        {
            ActionManager = manager;
            EditorActions = actions;
        }

        [MoonSharpVisible(false)]
        public void Perform() => EditorActions.ForEach(x => x.Perform());

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (int i = EditorActions.Count - 1; i >= 0; i--)
                EditorActions[i].Undo();
        }
    }
}