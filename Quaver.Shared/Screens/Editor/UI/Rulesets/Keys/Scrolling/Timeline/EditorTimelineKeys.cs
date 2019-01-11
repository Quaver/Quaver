using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
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
        public void Destroy()
        {
        }

        /// <summary>
        /// </summary>
        private void InitializeLines()
        {
            Lines = new List<TimelineSnapLine>();

            // Make lines starting at the very first ti
            foreach (var tp in Ruleset.WorkingMap.TimingPoints)
            {
                for (var i = 0; i < Ruleset.WorkingMap.GetTimingPointLength(tp) / tp.MillisecondsPerBeat * Ruleset.Screen.BeatSnap.Value; i++)
                {
                    var time = tp.StartTime + tp.MillisecondsPerBeat / Ruleset.Screen.BeatSnap.Value * i;

                    Lines.Add(new TimelineSnapLine(tp, i)
                    {
                        Image = UserInterface.BlankBox,
                        Size = new ScalableVector2(Container.Width - 4, 0),
                        X = Container.AbsolutePosition.X + 2,
                        Y = Container.HitPositionY - time * Container.TrackSpeed,
                        Tint = GetLineColor(i % Ruleset.Screen.BeatSnap.Value, i),
                        Height = i / Ruleset.Screen.BeatSnap.Value % 4 == 0 && i % Ruleset.Screen.BeatSnap.Value == 0 ? 6: 2
                    });
                }
            }
        }

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
                            return Colors.MainAccent;
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
            }

            return Color.White;
        }
    }
}