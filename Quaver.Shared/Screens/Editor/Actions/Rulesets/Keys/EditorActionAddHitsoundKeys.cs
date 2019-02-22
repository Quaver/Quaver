using System.Collections.Generic;
using System.Linq;
using osu.Shared;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Editor.UI.Hitsounds;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionAddHitsoundKeys : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.AddHitsound;

        /// <summary>
        /// </summary>
        private EditorRulesetKeys Ruleset { get; }

        /// <summary>
        /// </summary>
        private List<DrawableEditorHitObject> HitObjects { get; }

        /// <summary>
        /// </summary>
        private HitSounds Hitsound { get; }

        /// <summary>
        /// </summary>
        public EditorActionAddHitsoundKeys(EditorRulesetKeys ruleset, List<DrawableEditorHitObject> hitobjects, HitSounds hitsound)
        {
            Ruleset = ruleset;
            HitObjects = hitobjects;
            Hitsound = hitsound;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            HitObjects.ForEach(x => x.Info.HitSound |= Hitsound);
            Ruleset.SetSelectedHitsounds();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            HitObjects.ForEach(x => x.Info.HitSound -= Hitsound);
            Ruleset.SetSelectedHitsounds();
        }
    }
}