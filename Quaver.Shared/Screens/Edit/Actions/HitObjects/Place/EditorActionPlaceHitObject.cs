using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Place
{
    [MoonSharpUserData]
    public class EditorActionPlaceHitObject : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.PlaceHitObject;

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
        public EditorActionPlaceHitObject(EditorActionManager actionManager, Qua workingMap, HitObjectInfo hitObject)
        {
            WorkingMap = workingMap;
            HitObject = hitObject;
            ActionManager = actionManager;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.HitObjects.Add(HitObject);
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorHitObjectPlacedEventArgs(HitObject));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveHitObject(ActionManager, WorkingMap, HitObject).Perform();
    }
}
