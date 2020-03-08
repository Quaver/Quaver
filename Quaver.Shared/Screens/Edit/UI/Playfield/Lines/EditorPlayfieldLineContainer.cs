using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Edit.UI.Playfield.Timeline;
using Wobble.Audio.Tracks;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    /// <summary>
    ///     Responsible for drawing timing points/sv lines
    /// </summary>
    public class EditorPlayfieldLineContainer : Container
    {
        private EditorPlayfield Playfield { get; }

        private Qua Map { get; }

        private IAudioTrack Track { get; }

        private List<DrawableEditorLine> Lines { get; set; }

        /// <summary>
        ///     The lines that are visible and ready to be drawn to the screen
        /// </summary>
        private List<DrawableEditorLine> LinePool { get; set; }

        /// <summary>
        ///     The index of the last object that was added to the pool
        /// </summary>
        private int LastPooledLineIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="map"></param>
        /// <param name="track"></param>
        public EditorPlayfieldLineContainer(EditorPlayfield playfield, Qua map, IAudioTrack track)
        {
            Playfield = playfield;
            Map = map;
            Track = track;

            InitializeTicks();
            Track.Seeked += OnTrackSeeked;
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateLinePool();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            for (var i = 0; i < LinePool.Count; i++)
            {
                var line = LinePool[i];

                if (!line.IsOnScreen())
                    continue;

                line.SetPosition();
                line.SetSize();
                line.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            foreach (var line in Lines)
                line.Destroy();

            Track.Seeked -= OnTrackSeeked;
            base.Destroy();
        }

        /// <summary>
        ///     Initializes and positions all the timing point/sv lines
        /// </summary>
        private void InitializeTicks()
        {
            Lines = new List<DrawableEditorLine>();

            var timingPointIndex = 0;
            var svIndex = 0;

            // SV & Timing points are guaranteed to already be sorted, so there's no need to resort.
            while (Lines.Count != Map.TimingPoints.Count + Map.SliderVelocities.Count)
            {
                var pointExists = timingPointIndex < Map.TimingPoints.Count;
                var svExists = svIndex < Map.SliderVelocities.Count;

                if (pointExists && svExists)
                {
                    if (Map.TimingPoints[timingPointIndex].StartTime < Map.SliderVelocities[svIndex].StartTime)
                    {
                        Lines.Add(new DrawableEditorLineTimingPoint(Playfield, Map.TimingPoints[timingPointIndex]));
                        timingPointIndex++;
                    }
                    else
                    {
                        Lines.Add(new DrawableEditorLineScrollVelocity(Playfield, Map.SliderVelocities[svIndex]));
                        svIndex++;
                    }
                }
                else if (pointExists)
                {
                    Lines.Add(new DrawableEditorLineTimingPoint(Playfield, Map.TimingPoints[timingPointIndex]));
                    timingPointIndex++;
                }
                else if (svExists)
                {
                    Lines.Add(new DrawableEditorLineScrollVelocity(Playfield, Map.SliderVelocities[svIndex]));
                    svIndex++;
                }
            }

            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        private void InitializeLinePool()
        {
            LinePool = new List<DrawableEditorLine>();
            LastPooledLineIndex = -1;

            for (var i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];

                if (!line.IsOnScreen())
                    continue;

                LinePool.Add(line);
                LastPooledLineIndex = i;
            }
        }

        /// <summary>
        ///     Updates the object pool to get rid of old/out of view objects
        /// </summary>
        private void UpdateLinePool()
        {
            // Check the objects that are in the pool currently to see if they're still in view.
            // if they're not, remove them.
            for (var i = LinePool.Count - 1; i >= 0; i--)
            {
                var line = LinePool[i];

                if (!line.IsOnScreen())
                    LinePool.Remove(line);
            }

            // Add any objects that are now on-screen
            for (var i = LastPooledLineIndex + 1; i < Lines.Count; i++)
            {
                var line = Lines[i];

                if (!line.IsOnScreen())
                    break;

                LinePool.Add(line);
                LastPooledLineIndex = i;
            }
        }

        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e)
        {
            InitializeLinePool();
        }
    }
}