/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
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
        private int MaxBars { get; } = 85;

        private int SampleTime => (int) Math.Ceiling(AudioEngine.Track.Length / MaxBars);

        /// <summary>
        /// </summary>
        public Dictionary<int, Sprite> SampleBars { get; } = new Dictionary<int, Sprite>();

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
            var groupedSamples = Qua.HitObjects.GroupBy(u => u.StartTime / SampleTime)
                .Select(grp => grp.ToList())
                .ToList();

            foreach (var s in groupedSamples)
            {
                var width = GetBarWidth(s);

                var bar = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.BotLeft,
                    Size = new ScalableVector2(width, 5),
                    Y = -Height * (float) (s.First().StartTime / SampleTime * SampleTime / AudioEngine.Track.Length) - 2,
                    Tint = GetBarColor(width)
                };

                bar.X = Width - bar.Width - 1;
                SampleBars.Add(GetSample(s.First()), bar);
            }
        }

        /// <summary>
        ///     Gets the color of a bar by its width
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        private Color GetBarColor(int width)
        {
            Color color;

            if (width <= Width / 2)
                color = Color.LimeGreen;
            else if (width <= Width * 0.65f)
                color = Color.Orange;
            else
                color = Color.Crimson;

            return color;
        }

        /// <summary>
        ///     Refreshes a bar sample based on the hitobject received.
        /// </summary>
        /// <param name="h"></param>
        public void RefreshSample(HitObjectInfo h)
        {
            var sample = GetSample(h);

            // Bar needs to be resized
            if (SampleBars.ContainsKey(sample))
            {
                var objs = Qua.HitObjects.FindAll(x => h.StartTime / SampleTime == x.StartTime / SampleTime).ToList();
                var width = GetBarWidth(objs);
                SampleBars[sample].Width = width;
                SampleBars[sample].Tint = GetBarColor(width);
                SampleBars[sample].X = Width - SampleBars[sample].Width - 1;
            }
            // Bar needs to be added
            else
            {
                var objs = Qua.HitObjects.FindAll(x => h.StartTime / SampleTime == x.StartTime / SampleTime).ToList();
                var width = GetBarWidth(objs);

                var bar = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.BotLeft,
                    Size = new ScalableVector2(width, 5),
                    Y = -Height * (float) (objs.First().StartTime / SampleTime * SampleTime / AudioEngine.Track.Length) - 2,
                    Tint = GetBarColor(width)
                };

                bar.X = Width - bar.Width - 1;
                SampleBars.Add(GetSample(objs.First()), bar);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        private int GetSample(HitObjectInfo h) => h.StartTime / SampleTime;

        /// <summary>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private int GetBarWidth(List<HitObjectInfo> s)
        {
            const float scale = 1.55f;
            return (int) MathHelper.Clamp(s.Count / (float) SampleTime * 1000f * scale, 0, Width - 2);
        }
    }
}
