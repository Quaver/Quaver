/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionManagerKeys : EditorActionManager
    {
        /// <summary>
        /// </summary>
        private EditorRulesetKeys Ruleset { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        public EditorActionManagerKeys(EditorRulesetKeys ruleset) : base(ruleset.Screen) => Ruleset = ruleset;

        /// <summary>
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="time"></param>
        public void PlaceHitObject(int lane, double time) => Perform(new EditorActionPlaceHitObjectKeys(Ruleset.ScrollContainer, new HitObjectInfo
        {
            StartTime = (int) time,
            Lane = lane,
            EditorLayer = ((EditorScreenView)Ruleset.Screen.View).LayerCompositor.SelectedLayerIndex.Value
        }));

        /// <summary>
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="time"></param>
        public void PlaceLongNote(int lane, double time) => Perform(new EditorActionPlaceHitObjectKeys(Ruleset.ScrollContainer, new HitObjectInfo
        {
            StartTime = (int) time,
            EndTime = (int) time + 1,
            Lane = lane,
            EditorLayer = ((EditorScreenView)Ruleset.Screen.View).LayerCompositor.SelectedLayerIndex.Value
        }));

        /// <summary>
        /// </summary>
        public void DeleteHitObject(HitObjectInfo h) => Perform(new EditorActionDeleteHitObjectKeys(Ruleset.ScrollContainer, h));
    }
}
