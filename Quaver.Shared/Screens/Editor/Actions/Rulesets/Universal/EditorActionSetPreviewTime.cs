/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Maps;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionSetPreviewTime : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.SetPreviewTime;

        /// <summary>
        /// </summary>
        private EditorRuleset Ruleset { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private int PreviousPreviewTime { get; }

        /// <summary>
        /// </summary>
        private int Time { get; }

        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="workingMap"></param>
        /// <param name="time"></param>
        public EditorActionSetPreviewTime(EditorRuleset ruleset, Qua workingMap, int time)
        {
            Ruleset = ruleset;
            WorkingMap = workingMap;
            PreviousPreviewTime = WorkingMap.SongPreviewTime;
            Time = time;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            WorkingMap.SongPreviewTime = Time;

            var formattedTime = TimeSpan.FromMilliseconds(Time).ToString(@"mm\:ss\.fff");

            MovePreviewPointLine(Time);
            NotificationManager.Show(NotificationLevel.Info, $"Set new song preview time to: {formattedTime}");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            WorkingMap.SongPreviewTime = PreviousPreviewTime;

            var formattedTime = TimeSpan.FromMilliseconds(PreviousPreviewTime).ToString(@"mm\:ss\.fff");

            MovePreviewPointLine(PreviousPreviewTime);
            NotificationManager.Show(NotificationLevel.Info, $"Set song preview time back to: {formattedTime}");
        }

        /// <summary>
        ///     Moves the preview point line on the tick graph to a new location.
        /// </summary>
        /// <param name="time"></param>
        private void MovePreviewPointLine(int time)
        {
            switch (Ruleset)
            {
                case EditorRulesetKeys keys:
                    var graph = keys.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;
                    graph?.MovePreviewPointLine(time);
                    break;
            }
        }
    }
}
