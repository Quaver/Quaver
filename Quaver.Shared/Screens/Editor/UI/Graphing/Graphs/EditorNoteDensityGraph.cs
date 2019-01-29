/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using TagLib.Riff;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Graphing.Graphs
{
    public class EditorNoteDensityGraph : EditorVisualizationGraph
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="qua"></param>
        /// <param name="ruleset"></param>
        public EditorNoteDensityGraph(EditorVisualizationGraphContainer container, Qua qua, EditorRuleset ruleset)
            : base(container, qua, ruleset) => CreateBars();

        /// <summary>
        ///    Creates the bars for the density graph
        /// </summary>
        private void CreateBars()
        {
            const int maxBars = 85;
            var sampleTime = (int) Math.Ceiling(AudioEngine.Track.Length / maxBars);

            var groupedSamples = Qua.HitObjects.GroupBy(u => u.StartTime / sampleTime)
                .Select(grp => grp.ToList())
                .ToList();

            const float scale = 1.55f;
            foreach (var s in groupedSamples)
            {
                var width = (int) MathHelper.Clamp((s.Count / (float) sampleTime * 1000f * scale), 0, Width - 2);

                Color color;
                if (width <= Width / 2)
                    color = Color.LimeGreen;
                else if (width <= Width * 0.65f)
                    color = Color.Orange;
                else
                    color = Color.Crimson;

                // ReSharper disable once ObjectCreationAsStatement
                var bar = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(width, 5),
                    Y = Height * (float) (s.First().StartTime / sampleTime * sampleTime / AudioEngine.Track.Length) + 2,
                    Tint = color
                };

                bar.X = Width - bar.Width - 1;
            }

        }
    }
}