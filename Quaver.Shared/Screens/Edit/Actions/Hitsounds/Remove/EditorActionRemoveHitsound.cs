using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add;

namespace Quaver.Shared.Screens.Edit.Actions.Hitsounds.Remove
{
    public class EditorActionRemoveHitsound : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveHitsound;

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
        public EditorActionRemoveHitsound(EditorActionManager manager, List<HitObjectInfo> hitObjects, HitSounds sound)
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
                if (!ho.HitSound.HasFlag(Sound))
                    continue;

                ho.HitSound -= Sound;

                if (ho.HitSound < 0)
                    ho.HitSound = 0;
            }

            ActionManager.TriggerEvent(EditorActionType.RemoveHitsound, new EditorHitSoundRemovedEventArgs(HitObjects, Sound));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionAddHitsound(ActionManager, HitObjects, Sound).Perform();
    }
}