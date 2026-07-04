using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Timers;
using MoreLinq.Extensions;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Add;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Remove;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Preview;
using Quaver.Shared.Screens.Edit.Actions.SF.Add;
using Quaver.Shared.Screens.Edit.Actions.SF.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.SF.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.SF.Remove;
using Quaver.Shared.Screens.Edit.Actions.SF.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Create;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;
using Quaver.Shared.Screens.Edit.UI.Playfield.Timeline;
using Wobble;
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
        ///     Removing lines is a costly operation, so we only do it every 0.5 seconds
        /// </summary>
        private ContinuousClock RemoveTimer { get; } = new(0.5f);

        /// <summary>
        ///     Whether or not dense lines have been detected
        /// </summary>
        private bool DenseLinesDetected { get; set; }

        /// <summary>
        ///     The update frame counter for the line container
        /// </summary>
        private int _frameCounter;

        /// <summary>
        ///     The number of frames per actual line draw
        /// </summary>
        private int FrameSkipFactor => Math.ILogB(Math.Max(1, LinePool.Count / 1024)) + 1;

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
            ActionManager.ScrollVelocityOffsetBatchChanged += OnScrollVelocityBatchOffsetChanged;
            ActionManager.ScrollSpeedFactorAdded += OnScrollSpeedFactorAdded;
            ActionManager.ScrollSpeedFactorRemoved += OnScrollSpeedFactorRemoved;
            ActionManager.ScrollSpeedFactorBatchAdded += OnScrollSpeedFactorBatchAdded;
            ActionManager.ScrollSpeedFactorBatchRemoved += OnScrollSpeedFactorBatchRemoved;
            ActionManager.ScrollSpeedFactorOffsetBatchChanged += OnScrollSpeedFactorBatchOffsetChanged;
            ActionManager.TimingPointAdded += OnTimingPointAdded;
            ActionManager.TimingPointRemoved += OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded += OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved += OnTimingPointBatchRemoved;
            ActionManager.PreviewTimeChanged += OnPreviewTimeChanged;
            ActionManager.TimingPointOffsetChanged += OnTimingPointOffsetChanged;
            ActionManager.TimingPointOffsetBatchChanged += OnTimingPointOffsetBatchChanged;
            ActionManager.BookmarkAdded += OnBookmarkAdded;
            ActionManager.BookmarkBatchAdded += OnBookmarkBatchAdded;
            ActionManager.BookmarkRemoved += OnBookmarkRemoved;
            ActionManager.BookmarkBatchRemoved += OnBookmarkBatchRemoved;
            ActionManager.BookmarkBatchOffsetChanged += OnBookmarkBatchOffsetChanged;
            ActionManager.TimingGroupCreated += OnTimingGroupCreated;
            ActionManager.TimingGroupDeleted += OnTimingGroupDeleted;
            RemoveTimer.Tick += RemoveLines;
            RemoveTimer.Start();
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            RemoveTimer.Update(gameTime);
            UpdateLinePool(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Skip frame if we're lagging
            if (gameTime.ElapsedGameTime.TotalMilliseconds > 50f && _frameCounter % FrameSkipFactor != 0)
                return;

            for (var i = 0; i < LinePool.Count; i ++)
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
            ActionManager.ScrollVelocityOffsetBatchChanged -= OnScrollVelocityBatchOffsetChanged;
            ActionManager.TimingPointAdded -= OnTimingPointAdded;
            ActionManager.TimingPointRemoved -= OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded -= OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved -= OnTimingPointBatchRemoved;
            ActionManager.PreviewTimeChanged -= OnPreviewTimeChanged;
            ActionManager.TimingPointOffsetChanged -= OnTimingPointOffsetChanged;
            ActionManager.TimingPointOffsetBatchChanged -= OnTimingPointOffsetBatchChanged;
            ActionManager.BookmarkAdded -= OnBookmarkAdded;
            ActionManager.BookmarkBatchAdded -= OnBookmarkBatchAdded;
            ActionManager.BookmarkRemoved -= OnBookmarkRemoved;
            ActionManager.BookmarkBatchRemoved -= OnBookmarkBatchRemoved;
            ActionManager.BookmarkBatchOffsetChanged -= OnBookmarkBatchOffsetChanged;
            ActionManager.TimingGroupCreated -= OnTimingGroupCreated;
            ActionManager.TimingGroupDeleted -= OnTimingGroupDeleted;
            RemoveTimer.Tick -= RemoveLines;
            
            base.Destroy();
        }
        
        /// <summary>
        ///     Initializes and positions all the timing point/sv lines
        /// </summary>
        private void InitializeTicks()
        {
            Lines = new List<DrawableEditorLine>();

            foreach (var timingPointInfo in Map.TimingPoints)
            {
                Lines.Add(new DrawableEditorLineTimingPoint(Playfield, timingPointInfo));
            }

            foreach (var (id, timingGroup) in Map.TimingGroups)
            {
                if (timingGroup is ScrollGroup scrollGroup)
                {
                    foreach (var scrollVelocity in scrollGroup.ScrollVelocities)
                    {
                        Lines.Add(new DrawableEditorLineScrollVelocity(Playfield, scrollVelocity, timingGroup));
                    }
                    foreach (var scrollSpeedFactorInfo in scrollGroup.ScrollSpeedFactors)
                    {
                        Lines.Add(new DrawableEditorLineScrollSpeedFactor(Playfield, scrollSpeedFactorInfo, timingGroup));
                    }
                }
            }

            foreach (var bookmark in Map.Bookmarks)
                Lines.Add(new DrawableEditorLineBookmark(Playfield, bookmark));
            
            PreviewLine = new DrawableEditorLinePreview(Playfield, Map.SongPreviewTime);

            Lines.HybridSort();

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
                LastPooledLineIndex = Lines.FindLastIndex(x => x.StartTime < Track.Time);
        }

        private void RemoveLines(object sender, EventArgs eventArgs)
        {
            if (LinePool.Count > 4096)
            {
                LinePool = LinePool.FindAll(l => l.IsOnScreen());
                return;
            }
            // Check the objects that are in the pool currently to see if they're still in view.
            // if they're not, remove them.
            for (var i = LinePool.Count - 1; i >= 0; i--)
            {
                var line = LinePool[i];

                if (!line.IsOnScreen())
                    LinePool.Remove(line);
            }
        }

        /// <summary>
        ///     Updates the object pool to get rid of old/out of view objects
        /// </summary>
        private void UpdateLinePool(GameTime gameTime)
        {
            if (!DenseLinesDetected && FrameSkipFactor > 1)
            {
                DenseLinesDetected = true;
                NotificationManager.Show(NotificationLevel.Warning, "Dense lines detected. Lines will be drawn less frequently.");
            }
            
            if (_frameCounter++ % FrameSkipFactor != 0)
                return;

            for (var i = 0; i < LinePool.Count; i++)
            {
                var line = LinePool[i];
                line.Update(gameTime);
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
            Lines.InsertSorted(new DrawableEditorLineScrollVelocity(Playfield, e.ScrollVelocity, e.TimingGroup));
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityBatchAdded(object sender, EditorScrollVelocityBatchAddedEventArgs e)
        {
            Lines.InsertSorted(e.ScrollVelocities.Select(sv => new DrawableEditorLineScrollVelocity(Playfield, sv, e.ScrollGroup)));
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityRemoved(object sender, EditorScrollVelocityRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineScrollVelocity line && line.ScrollVelocity == e.ScrollVelocity;
                
                if (found)
                    x.Destroy();

                return found;
            });
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityBatchRemoved(object sender, EditorScrollVelocityBatchRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineScrollVelocity line && e.ScrollVelocities.Contains(line.ScrollVelocity);

                if (found)
                    x.Destroy();

                return found;
            });
            
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedFactorAdded(object sender, EditorScrollSpeedFactorAddedEventArgs e)
        {
            Lines.InsertSorted(new DrawableEditorLineScrollSpeedFactor(Playfield, e.ScrollSpeedFactor, e.ScrollGroup));
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedFactorBatchAdded(object sender, EditorScrollSpeedFactorBatchAddedEventArgs e)
        {
            Lines.InsertSorted(e.ScrollSpeedFactors.Select(sv => new DrawableEditorLineScrollSpeedFactor(Playfield, sv, e.ScrollGroup)));
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedFactorRemoved(object sender, EditorScrollSpeedFactorRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineScrollSpeedFactor line && line.ScrollSpeedFactor == e.ScrollSpeedFactor;
                
                if (found)
                    x.Destroy();

                return found;
            });
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedFactorBatchRemoved(object sender, EditorScrollSpeedFactorBatchRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineScrollSpeedFactor line && e.ScrollSpeedFactors.Contains(line.ScrollSpeedFactor);

                if (found)
                    x.Destroy();

                return found;
            });
            
            InitializeLinePool();
        }
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointAdded(object sender, EditorTimingPointAddedEventArgs e)
        {
            Lines.InsertSorted(new DrawableEditorLineTimingPoint(Playfield, e.TimingPoint));
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointOffsetChanged(object sender, EditorTimingPointOffsetChangedEventArgs e)
        {
            Lines.HybridSort();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointOffsetBatchChanged(object sender, EditorChangedTimingPointOffsetBatchEventArgs e)
        {
            Lines.HybridSort();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollVelocityBatchOffsetChanged(object sender, EditorChangedScrollVelocityOffsetBatchEventArgs e)
        {
            Lines.HybridSort();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedFactorBatchOffsetChanged(object sender, EditorChangedScrollSpeedFactorOffsetBatchEventArgs e)
        {
            Lines.HybridSort();
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointRemoved(object sender, EditorTimingPointAddedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineTimingPoint line && line.TimingPoint == e.TimingPoint;
                
                if (found)
                    x.Destroy();

                return found;
            });
            
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBatchAdded(object sender, EditorTimingPointBatchAddedEventArgs e)
        {
            Lines.InsertSorted(e.TimingPoints.Select(tp => new DrawableEditorLineTimingPoint(Playfield, tp)));
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBatchRemoved(object sender, EditorTimingPointBatchRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineTimingPoint line && e.TimingPoints.Contains(line.TimingPoint);

                if (found)
                    x.Destroy();

                return found;
            });
            
            InitializeLinePool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewTimeChanged(object sender, EditorChangedPreviewTimeEventArgs e) => PreviewLine.Time = e.Time;
        
        private void OnBookmarkAdded(object sender, EditorActionBookmarkAddedEventArgs e)
        {
            Lines.InsertSorted(new DrawableEditorLineBookmark(Playfield, e.Bookmark));
            InitializeLinePool();
        }
        
        private void OnBookmarkBatchAdded(object sender, EditorActionBookmarkBatchAddedEventArgs e)
        {
            Lines.InsertSorted(e.Bookmarks.Select(bookmark => new DrawableEditorLineBookmark(Playfield, bookmark)));
            InitializeLinePool();
        }
        
        private void OnBookmarkRemoved(object sender, EditorActionBookmarkRemovedEventArgs e)
        {
            var line = Lines.Find(x => x is DrawableEditorLineBookmark line && line.Bookmark == e.Bookmark);
            line.Destroy();
            Lines.Remove(line);
            InitializeLinePool();
        }
        
        private void OnBookmarkBatchRemoved(object sender, EditorActionBookmarkBatchRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineBookmark line && e.Bookmarks.Contains(line.Bookmark);

                if (found)
                    x.Destroy();

                return found;
            });
            InitializeLinePool();
        }
        
        private void OnBookmarkBatchOffsetChanged(object sender, EditorActionChangeBookmarkOffsetBatchEventArgs e)
        {
            Lines.HybridSort();
            InitializeLinePool();
        }

        private void OnTimingGroupDeleted(object sender, EditorTimingGroupRemovedEventArgs e)
        {
            if (e.TimingGroup is not ScrollGroup scrollGroup)
                return;

            Lines.RemoveAll(l =>
                l is DrawableEditorLineScrollVelocity { ScrollVelocity: var sv } &&
                scrollGroup.ScrollVelocities.Contains(sv));
            InitializeLinePool();
        }

        private void OnTimingGroupCreated(object sender, EditorTimingGroupCreatedEventArgs e)
        {
            if (e.TimingGroup is not ScrollGroup scrollGroup)
                return;

            Lines.InsertSorted(scrollGroup.ScrollVelocities.Select(sv =>
                new DrawableEditorLineScrollVelocity(Playfield, sv, scrollGroup)));
            InitializeLinePool();
        }
    }
}