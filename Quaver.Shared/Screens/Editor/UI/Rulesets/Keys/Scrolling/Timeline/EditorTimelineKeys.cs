using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
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
        public List<TimelineSnapLine> Lines { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="container"></param>
        public EditorTimelineKeys(EditorRulesetKeys ruleset, EditorScrollContainerKeys container)
        {
            Ruleset = ruleset;
            Container = container;
            InitializeLines();
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
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy() => Lines.ForEach(x => x.Destroy());

        /// <summary>
        /// </summary>
        private void InitializeLines()
        {
            Lines = new List<TimelineSnapLine>();

            // Make lines starting at the very first ti
            var measureCount = 0;

            foreach (var tp in Ruleset.WorkingMap.TimingPoints)
            {
                var pointLength = Ruleset.WorkingMap.GetTimingPointLength(tp);
                var startTime = tp.StartTime;

                // First point, so we need to make sure that the lines begin from the beginning of the track minus some.
                if (tp == Ruleset.WorkingMap.TimingPoints.First() && startTime > 0)
                {
                    var beatsBack = 0;

                    while (true)
                    {
                        if (beatsBack / Ruleset.Screen.BeatSnap.Value % 4 == 0 && beatsBack % Ruleset.Screen.BeatSnap.Value == 0 && startTime <= -2000)
                            break;

                        startTime -= tp.MillisecondsPerBeat;
                        beatsBack++;
                    }
                }

                // Last point, so the lines have to extend to the end of the song + more,
                if (tp == Ruleset.WorkingMap.TimingPoints[Ruleset.WorkingMap.TimingPoints.Count - 1])
                    pointLength = AudioEngine.Track.Length + 2000;

                // Create all lines.
                for (var i = 0; i < pointLength / tp.MillisecondsPerBeat * Ruleset.Screen.BeatSnap.Value; i++)
                {
                    var time = startTime + tp.MillisecondsPerBeat / Ruleset.Screen.BeatSnap.Value * i;

                    var measureBeat = i / Ruleset.Screen.BeatSnap.Value % 4 == 0 && i % Ruleset.Screen.BeatSnap.Value == 0;

                    if (measureBeat && time >= tp.StartTime)
                        measureCount++;

                    var height = measureBeat ? 4 : 1;

                    Lines.Add(new TimelineSnapLine(Container, tp, time, i, measureCount)
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
        }

        /// <summary>
        ///     Repositions the lines, usually used when the user changes zoom/audio rate.
        /// </summary>
        public void RepositionLines() => Lines.ForEach(x => x.Y = Container.HitPositionY - x.Time * Container.TrackSpeed - x.Height);

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
    }
}