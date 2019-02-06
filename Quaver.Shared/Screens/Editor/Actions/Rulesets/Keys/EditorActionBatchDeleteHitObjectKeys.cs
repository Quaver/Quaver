/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionBatchDeleteHitObjectKeys : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.BatchDeleteHitObjects;

        /// <summary>
        /// </summary>
        private EditorRuleset Ruleset { get; }

        /// <summary>
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        public EditorActionBatchDeleteHitObjectKeys(EditorRuleset ruleset, EditorScrollContainerKeys container, List<HitObjectInfo> hitObjects)
        {
            Ruleset = ruleset;
            Container = container;
            HitObjects = hitObjects;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform() => HitObjects.ForEach(x => new EditorActionDeleteHitObjectKeys(Container, x).Perform());

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => HitObjects.ForEach(x => new EditorActionPlaceHitObjectKeys(Container, x).Perform());
    }
}