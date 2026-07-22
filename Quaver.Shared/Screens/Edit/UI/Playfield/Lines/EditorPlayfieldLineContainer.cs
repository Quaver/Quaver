using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Timers;
using MoreLinq.Extensions;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Wobble.Managers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Add;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Remove;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Preview;
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

        /// <summary>
        ///     The cached SV/SF graph layer.
        /// </summary>
        private EditorPlayfieldScrollGraphCache ScrollGraph { get; }

        private List<DrawableEditorLine> Lines { get; set; }

        /// <summary>
        ///     Bookmark lines own globally registered buttons and subscribed text drawables, unlike ordinary timing
        ///     and scroll lines, so only these lines require explicit destruction.
        /// </summary>
        private List<DrawableEditorLineBookmark> BookmarkLines { get; } = new List<DrawableEditorLineBookmark>();

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
            ScrollGraph = new EditorPlayfieldScrollGraphCache(playfield, map);

            InitializeTicks();
            Track.Seeked += OnTrackSeeked;
            ActionManager.ScrollVelocityAdded += OnScrollGraphChanged;
            ActionManager.ScrollVelocityRemoved += OnScrollGraphChanged;
            ActionManager.ScrollVelocityBatchAdded += OnScrollGraphChanged;
            ActionManager.ScrollVelocityBatchRemoved += OnScrollGraphChanged;
            ActionManager.ScrollVelocityOffsetBatchChanged += OnScrollGraphChanged;
            ActionManager.ScrollVelocityMultiplierBatchChanged += OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorAdded += OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorRemoved += OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorBatchAdded += OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorBatchRemoved += OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorOffsetBatchChanged += OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorMultiplierBatchChanged += OnScrollGraphChanged;
            ActionManager.TimingPointAdded += OnScrollGraphChanged;
            ActionManager.TimingPointRemoved += OnScrollGraphChanged;
            ActionManager.TimingPointBatchAdded += OnScrollGraphChanged;
            ActionManager.TimingPointBatchRemoved += OnScrollGraphChanged;
            ActionManager.PreviewTimeChanged += OnPreviewTimeChanged;
            ActionManager.TimingPointOffsetChanged += OnScrollGraphChanged;
            ActionManager.TimingPointOffsetBatchChanged += OnScrollGraphChanged;
            ActionManager.BookmarkAdded += OnBookmarkAdded;
            ActionManager.BookmarkBatchAdded += OnBookmarkBatchAdded;
            ActionManager.BookmarkRemoved += OnBookmarkRemoved;
            ActionManager.BookmarkBatchRemoved += OnBookmarkBatchRemoved;
            ActionManager.BookmarkBatchOffsetChanged += OnBookmarkBatchOffsetChanged;
            ActionManager.TimingGroupCreated += OnScrollGraphChanged;
            ActionManager.TimingGroupDeleted += OnScrollGraphChanged;
            ActionManager.TimingGroupColorChanged += OnScrollGraphChanged;
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
            ScrollGraph.Update(gameTime);
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

            ScrollGraph.Draw(gameTime);

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
            foreach (var line in BookmarkLines)
                line.Destroy();

            PreviewLine?.Destroy();
            ScrollGraph.Destroy();

            Track.Seeked -= OnTrackSeeked;
            ActionManager.ScrollVelocityAdded -= OnScrollGraphChanged;
            ActionManager.ScrollVelocityRemoved -= OnScrollGraphChanged;
            ActionManager.ScrollVelocityBatchAdded -= OnScrollGraphChanged;
            ActionManager.ScrollVelocityBatchRemoved -= OnScrollGraphChanged;
            ActionManager.ScrollVelocityOffsetBatchChanged -= OnScrollGraphChanged;
            ActionManager.ScrollVelocityMultiplierBatchChanged -= OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorAdded -= OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorRemoved -= OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorBatchAdded -= OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorBatchRemoved -= OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorOffsetBatchChanged -= OnScrollGraphChanged;
            ActionManager.ScrollSpeedFactorMultiplierBatchChanged -= OnScrollGraphChanged;
            ActionManager.TimingPointAdded -= OnScrollGraphChanged;
            ActionManager.TimingPointRemoved -= OnScrollGraphChanged;
            ActionManager.TimingPointBatchAdded -= OnScrollGraphChanged;
            ActionManager.TimingPointBatchRemoved -= OnScrollGraphChanged;
            ActionManager.PreviewTimeChanged -= OnPreviewTimeChanged;
            ActionManager.TimingPointOffsetChanged -= OnScrollGraphChanged;
            ActionManager.TimingPointOffsetBatchChanged -= OnScrollGraphChanged;
            ActionManager.BookmarkAdded -= OnBookmarkAdded;
            ActionManager.BookmarkBatchAdded -= OnBookmarkBatchAdded;
            ActionManager.BookmarkRemoved -= OnBookmarkRemoved;
            ActionManager.BookmarkBatchRemoved -= OnBookmarkBatchRemoved;
            ActionManager.BookmarkBatchOffsetChanged -= OnBookmarkBatchOffsetChanged;
            ActionManager.TimingGroupCreated -= OnScrollGraphChanged;
            ActionManager.TimingGroupDeleted -= OnScrollGraphChanged;
            ActionManager.TimingGroupColorChanged -= OnScrollGraphChanged;
            RemoveTimer.Tick -= RemoveLines;
            
            base.Destroy();
        }
        
        /// <summary>
        ///     Initializes and positions the non-cached editor lines
        /// </summary>
        private void InitializeTicks()
        {
            Lines = new List<DrawableEditorLine>();

            foreach (var bookmark in Map.Bookmarks)
            {
                var line = new DrawableEditorLineBookmark(Playfield, bookmark);
                BookmarkLines.Add(line);
                Lines.Add(line);
            }
            
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
                NotificationManager.Show(NotificationLevel.Warning,
                    LocalizationManager.Get("Screen_Editor_DenseLinesDetected"));
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

        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e)
        {
            InitializeLinePool();
            ScrollGraph.Invalidate(true);
        }

        private void OnScrollGraphChanged(object sender, EventArgs e) => ScrollGraph.Invalidate();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewTimeChanged(object sender, EditorChangedPreviewTimeEventArgs e) => PreviewLine.Time = e.Time;
        
        private void OnBookmarkAdded(object sender, EditorActionBookmarkAddedEventArgs e)
        {
            var line = new DrawableEditorLineBookmark(Playfield, e.Bookmark);
            BookmarkLines.Add(line);
            Lines.InsertSorted(line);
            InitializeLinePool();
        }
        
        private void OnBookmarkBatchAdded(object sender, EditorActionBookmarkBatchAddedEventArgs e)
        {
            var lines = e.Bookmarks.Select(bookmark => new DrawableEditorLineBookmark(Playfield, bookmark)).ToList();
            BookmarkLines.AddRange(lines);
            Lines.InsertSorted(lines);
            InitializeLinePool();
        }
        
        private void OnBookmarkRemoved(object sender, EditorActionBookmarkRemovedEventArgs e)
        {
            var line = Lines.Find(x => x is DrawableEditorLineBookmark line && line.Bookmark == e.Bookmark);

            if (line == null)
                return;

            line.Destroy();
            Lines.Remove(line);
            BookmarkLines.Remove((DrawableEditorLineBookmark)line);
            InitializeLinePool();
        }
        
        private void OnBookmarkBatchRemoved(object sender, EditorActionBookmarkBatchRemovedEventArgs e)
        {
            Lines.RemoveAll(x =>
            {
                var found = x is DrawableEditorLineBookmark line && e.Bookmarks.Contains(line.Bookmark);

                if (found)
                {
                    x.Destroy();
                    BookmarkLines.Remove((DrawableEditorLineBookmark)x);
                }

                return found;
            });
            InitializeLinePool();
        }
        
        private void OnBookmarkBatchOffsetChanged(object sender, EditorActionChangeBookmarkOffsetBatchEventArgs e)
        {
            Lines.HybridSort();
            InitializeLinePool();
        }

    }
}
