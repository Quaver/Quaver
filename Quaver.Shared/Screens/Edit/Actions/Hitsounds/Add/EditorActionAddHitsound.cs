using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add
{
    public class EditorActionAddHitsound : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.AddHitsound;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        private HitSounds Sound { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObjects"></param>
        /// <param name="sound"></param>
        public EditorActionAddHitsound(EditorActionManager manager, List<HitObjectInfo> hitObjects, HitSounds sound)
        {
            ActionManager = manager;
            HitObjects = hitObjects;
            Sound = sound;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            foreach (var ho in HitObjects)
            {
                if (ho.HitSound.HasFlag(Sound))
                    continue;

                ho.HitSound |= Sound;
            }

            ActionManager.TriggerEvent(EditorActionType.AddHitsound, new EditorHitsoundAddedEventArgs(HitObjects, Sound));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionRemoveHitsound(ActionManager, HitObjects, Sound).Perform();
    }
}