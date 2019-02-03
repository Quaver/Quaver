/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Layering;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionRemoveLayer : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveLayer;

        /// <summary>
        /// </summary>
        private EditorLayerCompositor Compositor { get; }

        /// <summary>
        /// </summary>
        private EditorLayerInfo Layer { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> RemovedHitObjects { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="compositor"></param>
        /// <param name="l"></param>
        public EditorActionRemoveLayer(Qua workingMap, EditorLayerCompositor compositor, EditorLayerInfo l)
        {
            WorkingMap = workingMap;
            Compositor = compositor;
            Layer = l;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            var index = Compositor.ScrollContainer.AvailableItems.IndexOf(Layer);

            WorkingMap.EditorLayers.Remove(Layer);
            Compositor.ScrollContainer.RemoveLayer(Layer);
            Compositor.SelectedLayerIndex.Value = index - 1;

            var ruleset = Compositor.Screen.Ruleset as EditorRulesetKeys;

            RemovedHitObjects = WorkingMap.HitObjects.FindAll(x => x.EditorLayer == index).ToList();

            foreach (var hitObject in RemovedHitObjects)
                new EditorActionDeleteHitObjectKeys(ruleset?.ScrollContainer, hitObject).Perform();

            // Find HitObjects at the indices below it and update them
            var hitObjects = Compositor.Screen.WorkingMap.HitObjects.FindAll(x => x.EditorLayer > index);
            hitObjects.ForEach(x => x.EditorLayer--);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            new EditorActionAddLayer(WorkingMap, Compositor, Layer).Perform();

            var ruleset = Compositor.Screen.Ruleset as EditorRulesetKeys;

            foreach (var h in RemovedHitObjects)
            {
                h.EditorLayer = Compositor.SelectedLayerIndex.Value;
                new EditorActionPlaceHitObjectKeys(ruleset?.ScrollContainer, h).Perform();
            }
        }
    }
}
