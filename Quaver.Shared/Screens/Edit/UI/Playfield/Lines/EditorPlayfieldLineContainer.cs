using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Preview;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
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

        private EditorActionManager ActionManager { get; }

        private List<DrawableEditorLine> Lines { get; set; }

        /// <summary>
        ///     The lines that are visible and ready to be drawn to the screen
        /// </summary>
        private List<DrawableEditorLine> LinePool { get; set; }

        /// <summary>
        /// </summary>
        private DrawableEditorLinePreview PreviewLine { get; set; }

        /// <summary>
        ///     The index of the last object that was added to the pool
        /// </summary>
        private int LastPooledLineIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="map"></param>
        /// <param name="track"></param>
        /// <param name="actionManager"></param>
        public EditorPlayfieldLineContainer(EditorPlayfield playfield, Qua map, IAudioTrack track, EditorActionManager actionManager)
        {
            Playfield = playfield;
            Map = map;
            Track = track;
            ActionManager = actionManager;

            InitializeTicks();
            Track.Seeked += OnTrackSeeked;
            ActionManager.ScrollVelocityAdded += OnScrollVelocityAdded;
            ActionManager.ScrollVelocityRemoved += OnScrollVelocityRemoved;
            ActionManager.ScrollVelocityBatchAdded += OnScrollVelocityBatchAdded;
            ActionManager.ScrollVelocityBatchRemoved += OnScrollVelocityBatchRemoved;
            ActionManager.TimingPointAdded += OnTimingPointAdded;
            ActionManager.TimingPointRemoved += OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded += OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved += OnTimingPointBatchRemoved;
            ActionManager.PreviewTimeChanged += OnPreviewTimeChanged;
        }

        /// <inheritdoc />
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

            if (PreviewLine.IsOnScreen())
            {
                PreviewLine.SetPosition();
                PreviewLine.SetSize();
                PreviewLine.Draw(gameTime);
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

            PreviewLine?.Destroy();

            Track.Seeked -= OnTrackSeeked;
            ActionManager.ScrollVelocityAdded -= OnScrollVelocityAdded;
            ActionManager.ScrollVelocityRemoved -= OnScrollVelocityRemoved;
            ActionManager.ScrollVelocityBatchAdded -= OnScrollVelocityBatchAdded;
            ActionManager.ScrollVelocityBatchRemoved -= OnScrollVelocityBatchRemoved;
            ActionManager.TimingPointAdded -= OnTimingPointAdded;
            ActionManager.TimingPointRemoved -= OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded -= OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved -= OnTimingPointBatchRemoved;
            ActionManager.PreviewTimeChanged -= OnPreviewTimeChanged;

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

            PreviewLine = new DrawableEditorLinePreview(Playfield, Map.SongPreviewTime);
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

            if (LastPooledLineIndex == -1)
                LastPooledLineIndex = Lines.FindLastIndex(x => x.GetTime() < Track.Time);
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

        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => InitializeLinePool();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityAdded(object sender, EditorScrollVelocityAddedEventArgs e)
        {
            Lines.Add(new DrawableEditorLineScrollVelocity(Playfield, e.ScrollVelocity));
            Lines = Lines.OrderBy(x => x.GetTime()).ToList();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityBatchAdded(object sender, EditorScrollVelocityBatchAddedEventArgs e)
        {
            foreach (var sv in e.ScrollVelocities)
                Lines.Add(new DrawableEditorLineScrollVelocity(Playfield, sv));

            Lines = Lines.OrderBy(x => x.GetTime()).ToList();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityRemoved(object sender, EditorScrollVelocityRemovedEventArgs e)
        {
            Lines.RemoveAll(x => x is DrawableEditorLineScrollVelocity line && line.ScrollVelocity == e.ScrollVelocity);
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityBatchRemoved(object sender, EditorScrollVelocityBatchRemovedEventArgs e)
        {
            foreach (var sv in e.ScrollVelocities)
                Lines.RemoveAll(x => x is DrawableEditorLineScrollVelocity line && line.ScrollVelocity == sv);

            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointAdded(object sender, EditorTimingPointAddedEventArgs e)
        {
            Lines.Add(new DrawableEditorLineTimingPoint(Playfield, e.TimingPoint));
            Lines = Lines.OrderBy(x => x.GetTime()).ToList();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointRemoved(object sender, EditorTimingPointAddedEventArgs e)
        {
            Lines.RemoveAll(x => x is DrawableEditorLineTimingPoint line && line.TimingPoint == e.TimingPoint);
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBatchAdded(object sender, EditorTimingPointBatchAddedEventArgs e)
        {
            foreach (var sv in e.TimingPoints)
                Lines.Add(new DrawableEditorLineTimingPoint(Playfield, sv));

            Lines = Lines.OrderBy(x => x.GetTime()).ToList();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBatchRemoved(object sender, EditorTimingPointBatchRemovedEventArgs e)
        {
            foreach (var tp in e.TimingPoints)
                Lines.RemoveAll(x => x is DrawableEditorLineTimingPoint line && line.TimingPoint == tp);

            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewTimeChanged(object sender, EditorChangedPreviewTimeEventArgs e) => PreviewLine.Time = e.Time;
    }
}