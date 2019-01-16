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
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline
{
    public class EditorTimelineKeys : IDrawable
    {
        /// <summary>
        /// </summary>
        private EditorRulesetKeys Ruleset { get; }

        /// <summary>
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        /// </summary>
        public List<TimelineTickLine> Lines { get; private set; }

        /// <summary>
        ///     Contains cached timeline tick lines for each beat snap.
        /// </summary>
        private Dictionary<int, List<TimelineTickLine>> CachedLines { get; } = new Dictionary<int, List<TimelineTickLine>>();

        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="container"></param>
        public EditorTimelineKeys(EditorRulesetKeys ruleset, EditorScrollContainerKeys container)
        {
            Ruleset = ruleset;
            Container = container;

            InitializeLines();
            Ruleset.Screen.BeatSnap.ValueChanged += OnBeatSnapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            try
            {
                foreach (var line in new List<TimelineTickLine>(Lines))
                {
                    if (line.IsInView)
                        line.Draw(gameTime);
                }
            }
            catch (Exception)
            {
                // ignored. Usually happens when initializing lines on a new thread.
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy()
        {
            foreach (var item in CachedLines)
                item.Value.ForEach(x => x.Destroy());

            // ReSharper disable once DelegateSubtraction
            Ruleset.Screen.BeatSnap.ValueChanged -= OnBeatSnapChanged;
        }

        /// <summary>
        /// </summary>
        private void InitializeLines(bool forceRefresh = false)
        {
            if (CachedLines.ContainsKey(Ruleset.Screen.BeatSnap.Value) && !forceRefresh)
            {
                Lines = CachedLines[Ruleset.Screen.BeatSnap.Value];
                return;
            }

            var lines = new List<TimelineTickLine>();

            // Keeps track of the total amount of measures in the song.
            var measureCount = 0;

            foreach (var tp in Ruleset.WorkingMap.TimingPoints)
            {
                var pointLength = Ruleset.WorkingMap.GetTimingPointLength(tp);
                var startTime = tp.StartTime;
                var numBeatsOffsetted = 0;

                // First point, so we need to make sure that the lines begin from the beginning of the track minus some.
                if (Equals(tp, Ruleset.WorkingMap.TimingPoints.First()) && startTime > 0)
                {
                    while (true)
                    {
                        if (numBeatsOffsetted / Ruleset.Screen.BeatSnap.Value % 4 == 0
                            && numBeatsOffsetted % Ruleset.Screen.BeatSnap.Value == 0 && startTime <= -2000)
                            break;

                        numBeatsOffsetted++;

                        // Move the start time back a beat.
                        startTime -= tp.MillisecondsPerBeat;

                        // Since we're moving back a beat, we still want the point to end at the same position,
                        // so we need to compensate for this.
                        pointLength += tp.MillisecondsPerBeat;
                    }
                }

                // Last point, so the lines have to extend to the end of the song + more,
                if (Equals(tp, Ruleset.WorkingMap.TimingPoints[Ruleset.WorkingMap.TimingPoints.Count - 1]))
                    pointLength = AudioEngine.Track.Length + tp.MillisecondsPerBeat * numBeatsOffsetted + 2000;

                // Create all lines.
                for (var i = 0; i < pointLength / tp.MillisecondsPerBeat * Ruleset.Screen.BeatSnap.Value; i++)
                {
                    var time = startTime + tp.MillisecondsPerBeat / Ruleset.Screen.BeatSnap.Value * i;

                    var measureBeat = i / Ruleset.Screen.BeatSnap.Value % 4 == 0 && i % Ruleset.Screen.BeatSnap.Value == 0;

                    if (measureBeat && time >= tp.StartTime)
                        measureCount++;

                    var height = measureBeat ? 4 : 1;

                    lines.Add(new TimelineTickLine(Container, tp, time, i, measureCount)
                    {
                        Image = UserInterface.BlankBox,
                        Size = new ScalableVector2(Container.Width - 4, 0),
                        X = Container.AbsolutePosition.X + 2,
                        Y = Container.HitPositionY - time * Container.TrackSpeed - height,
                        Tint = GetLineColor(i % Ruleset.Screen.BeatSnap.Value, i),
                        Height = height
                    });
                }
            }

            CachedLines[Ruleset.Screen.BeatSnap.Value] = lines;
            Lines = lines;
        }

        /// <summary>
        ///     Repositions the lines, usually used when the user changes zoom/audio rate.
        /// </summary>
        public void RepositionLines() => ThreadScheduler.Run(() =>
        {
            foreach (var item in CachedLines)
                item.Value.ForEach(x => x.Y = Container.HitPositionY - x.Time * Container.TrackSpeed - x.Height);
        });

        /// <summary>
        ///     Gets an individual lioe color for the snap line.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private Color GetLineColor(int val, int i)
        {
            switch (Ruleset.Screen.BeatSnap.Value)
            {
                // 1/1th
                case 1:
                    return Color.White;
                // 1/2nd
                case 2:
                    switch (val)
                    {
                        case 0:
                            return Color.White;
                        default:
                            return Color.Red;
                    }
                // 1/4th
                case 4:
                    switch (val)
                    {
                        case 0:
                        case 4:
                            return Color.White;
                        case 1:
                        case 3:
                            return Color.CadetBlue;
                        default:
                            return Color.Red;
                    }
                // 1/3rd, 1/6th, 1/12th,
                case 3:
                case 6:
                case 12:
                    if (val % 3 == 0)
                        return Color.Red;
                    else if (val == 0)
                        return Color.White;
                    else
                        return Color.Purple;
                // 1/8th, 1//16th
                case 8:
                case 16:
                    if (val == 0)
                        return Color.White;
                    else if (( i - 1 ) % 2 == 0)
                        return Color.Gold;
                    else if (i % 4 == 0)
                        return Color.Red;
                    else
                        return Colors.MainAccent;
                default:
                    return Color.White;
            }
        }

        /// <summary>
        ///     Called when the user wants to change their selected beat snap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeatSnapChanged(object sender, BindableValueChangedEventArgs<int> e) => ThreadScheduler.Run(() => InitializeLines());
    }
}