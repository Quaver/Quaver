/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Graphing.Graphs
{
    public class EditorTickGraph : EditorVisualizationGraph
    {
        /// <summary>
        /// </summary>
        private Dictionary<TimingPointInfo, Sprite> TimingPointLines { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<SliderVelocityInfo, Sprite> SliderVelocityLines { get; set; }

        /// <summary>
        /// </summary>
        private Sprite PreviewPoint { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="qua"></param>
        /// <param name="ruleset"></param>
        public EditorTickGraph(EditorVisualizationGraphContainer container, Qua qua, EditorRuleset ruleset) : base(container, qua, ruleset)
            => CreateTickLines();

        /// <summary>
        /// </summary>
        private void CreateTickLines()
        {
            TimingPointLines = new Dictionary<TimingPointInfo, Sprite>();
            SliderVelocityLines = new Dictionary<SliderVelocityInfo, Sprite>();

            foreach (var tp in Qua.TimingPoints)
                TimingPointLines[tp] = CreateTimingPointLine(tp);

            foreach (var sv in Qua.SliderVelocities)
                SliderVelocityLines[sv] = CreateSliderVelocityLine(sv);

            PreviewPoint = CreatePreviewPointLine(Qua.SongPreviewTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        private Sprite CreateTimingPointLine(TimingPointInfo tp) => new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Size = new ScalableVector2(Width - 4, 2),
            Tint = Color.Crimson,
            Y = Height - Height * (float) (tp.StartTime / (AudioEngine.Track.Length)),
            Image = Pixel,
            Alpha = 0.85f
        };

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        /// <returns></returns>
        public Sprite CreateSliderVelocityLine(SliderVelocityInfo sv) => new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Size = new ScalableVector2(Width - 4, 2),
            Tint = Color.LimeGreen,
            Y = Height - Height * (float) (sv.StartTime / (AudioEngine.Track.Length)),
            Image = Pixel,
            Alpha = 0.85f
        };

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private Sprite CreatePreviewPointLine(int time) => new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Size = new ScalableVector2(Width - 4, 2),
            Tint = Color.Gold,
            Y = Height - Height * (float) (time/ AudioEngine.Track.Length),
            Image = Pixel,
            Alpha = 0.85f
        };

        /// <summary>
        /// </summary>
        public void MovePreviewPointLine(int time)
        {
            PreviewPoint.Y = Height - Height * (float) (time / AudioEngine.Track.Length);
            Container.ForceRecache();
        }

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        public void AddSliderVelocityLine(SliderVelocityInfo sv)
        {
            SliderVelocityLines[sv] = CreateSliderVelocityLine(sv);
            Container.ForceRecache();
        }

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        public void RemoveSliderVelocityLine(SliderVelocityInfo sv)
        {
            if (sv == null)
                return;

            if (SliderVelocityLines.ContainsKey(sv))
            {
                SliderVelocityLines[sv].Destroy();
                SliderVelocityLines.Remove(sv);
            }

            Container.ForceRecache();
        }

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void AddTimingPointLine(TimingPointInfo tp)
        {
            TimingPointLines[tp] = CreateTimingPointLine(tp);
            Container.ForceRecache();
        }

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void RemoveTimingPointLine(TimingPointInfo tp)
        {
            if (tp == null)
                return;

            if (TimingPointLines.ContainsKey(tp))
            {
                TimingPointLines[tp].Destroy();
                TimingPointLines.Remove(tp);
            }

            Container.ForceRecache();
        }

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        public void MoveSliderVelocityLine(SliderVelocityInfo sv)
        {
            if (sv == null || !SliderVelocityLines.ContainsKey(sv))
                return;

            SliderVelocityLines[sv].Y = Height - Height * (float) (sv.StartTime / (AudioEngine.Track.Length));
            Container.ForceRecache();
        }

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void MoveTimingPointLine(TimingPointInfo tp)
        {
            if (tp == null || !TimingPointLines.ContainsKey(tp))
                return;

            TimingPointLines[tp].Y = Height - Height * (float) (tp.StartTime / AudioEngine.Track.Length);

            if (TimingPointLines[tp].Y < 0)
                TimingPointLines[tp].Y = 0;

            Container.ForceRecache();
        }
    }
}
