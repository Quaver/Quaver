using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove
{
    [MoonSharpUserData]
    public class EditorActionRemoveHitObject : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveHitObject;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private HitObjectInfo HitObject { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObject"></param>
        [MoonSharpVisible(false)]
        public EditorActionRemoveHitObject(EditorActionManager actionManager, Qua workingMap, HitObjectInfo hitObject)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            HitObject = hitObject;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.HitObjects.Remove(HitObject);
            WorkingMap.Sort();

            ActionManager.TriggerEvent(EditorActionType.RemoveHitObject, new EditorHitObjectRemovedEventArgs(HitObject));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionPlaceHitObject(ActionManager, WorkingMap, HitObject).Perform();
    }
}
