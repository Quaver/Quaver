using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpmBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignature;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignatureBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Timeline
{
    public class EditorPlayfieldTimeline : Container
    {
        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        /// </summary>
        private EditorPlayfield Playfield { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        private BindableInt BeatSnap { get; }

        /// <summary>
        /// </summary>
        private BindableInt ScrollSpeed { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> ScaleScrollSpeedWithAudioRate { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorBeatSnapColor> BeatSnapColor { get; }

        /// <summary>
        /// </summary>
        public List<EditorPlayfieldTimelineTick> Lines { get; private set; }

        /// <summary>
        ///     Contains cached timeline tick lines for each beat snap.
        /// </summary>
        private Dictionary<int, List<EditorPlayfieldTimelineTick>> CachedLines { get; } = new Dictionary<int, List<EditorPlayfieldTimelineTick>>();

        /// <summary>
        ///     The lines that are visible and ready to be drawn to the screen
        /// </summary>
        private List<EditorPlayfieldTimelineTick> LinePool { get; set; }

        /// <summary>
        ///     The index of the last object that was added to the pool
        /// </summary>
        private int LastPooledLineIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="map"></param>
        /// <param name="playfield"></param>
        /// <param name="track"></param>
        /// <param name="beatSnap"></param>
        /// <param name="scrollSpeed"></param>
        /// <param name="scaleScrollSpeedWithAudioRate"></param>
        /// <param name="beatSnapColor"></param>
        public EditorPlayfieldTimeline(EditorActionManager manager, Qua map, EditorPlayfield playfield, IAudioTrack track, BindableInt beatSnap,
            BindableInt scrollSpeed, Bindable<bool> scaleScrollSpeedWithAudioRate, Bindable<EditorBeatSnapColor> beatSnapColor)
        {
            Map = map;
            Playfield = playfield;
            Track = track;
            BeatSnap = beatSnap;
            ScrollSpeed = scrollSpeed;
            ScaleScrollSpeedWithAudioRate = scaleScrollSpeedWithAudioRate;
            BeatSnapColor = beatSnapColor;
            ActionManager = manager;

            InitializeLines();

            BeatSnap.ValueChanged += OnBeatSnapChanged;
            ScrollSpeed.ValueChanged += OnScrollSpeedChanged;
            Track.Seeked += OnTrackSeeked;
            Track.RateChanged += OnTrackRateChanged;
            ScaleScrollSpeedWithAudioRate.ValueChanged += OnScaleScrollSpeedWithRateChanged;
            ActionManager.TimingPointAdded += OnTimingPointAdded;
            ActionManager.TimingPointRemoved += OnTimingPointRemoved;
            ActionManager.TimingPointBatchAdded += OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved += OnTimingPointBatchRemoved;
            ActionManager.TimingPointOffsetChanged += OnTimingPointOffsetChanged;
            ActionManager.TimingPointOffsetBatchChanged += OnTimingPointOffsetBatchChanged;
            ActionManager.TimingPointBpmChanged += OnTimingPointBpmChanged;
            ActionManager.TimingPointBpmBatchChanged += OnTimingPointBpmBatchChanged;
            ActionManager.TimingPointSignatureChanged += OnTimingPointSignatureChanged;
            ActionManager.TimingPointSignatureBatchChanged += OnTimingPointSignatureBatchChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => UpdateLinePool();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime) => DrawLines(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            foreach (var item in CachedLines)
                item.Value.ForEach(x => x.Destroy());

            // ReSharper disable twice DelegateSubtraction
            BeatSnap.ValueChanged -= OnBeatSnapChanged;
            ScrollSpeed.ValueChanged -= OnScrollSpeedChanged;
            Track.Seeked -= OnTrackSeeked;
            Track.RateChanged -= OnTrackRateChanged;
            ScaleScrollSpeedWithAudioRate.ValueChanged -= OnScaleScrollSpeedWithRateChanged;
            ActionManager.TimingPointAdded -= OnTimingPointAdded;
            ActionManager.TimingPointBatchAdded -= OnTimingPointBatchAdded;
            ActionManager.TimingPointBatchRemoved -= OnTimingPointBatchRemoved;
            ActionManager.TimingPointOffsetChanged -= OnTimingPointOffsetChanged;
            ActionManager.TimingPointOffsetBatchChanged -= OnTimingPointOffsetBatchChanged;
            ActionManager.TimingPointBpmChanged -= OnTimingPointBpmChanged;
            ActionManager.TimingPointBpmBatchChanged -= OnTimingPointBpmBatchChanged;
            ActionManager.TimingPointSignatureChanged -= OnTimingPointSignatureChanged;
            ActionManager.TimingPointSignatureBatchChanged -= OnTimingPointSignatureBatchChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        public void InitializeLines(bool forceRefresh = false)
        {
            if (CachedLines.ContainsKey(BeatSnap.Value) && !forceRefresh)
            {
                Lines = CachedLines[BeatSnap.Value];
                InitializeLinePool();
                return;
            }

            var lines = new List<EditorPlayfieldTimelineTick>();

            // Keeps track of the total amount of measures in the song.
            var measureCount = 0;

            foreach (var tp in Map.TimingPoints)
            {
                if (tp.StartTime > Track.Length)
                    continue;

                if (float.IsInfinity(tp.Bpm))
                    continue;

                var pointLength = Map.GetTimingPointLength(tp);
                var startTime = tp.StartTime;
                var numBeatsOffsetted = 0;

                // First point, so we need to make sure that the lines begin from the beginning of the track minus some.
                if (Equals(tp, Map.TimingPoints.First()) && startTime > 0)
                {
                    while (true)
                    {
                        if (numBeatsOffsetted / BeatSnap.Value % (int)tp.Signature == 0
                            && numBeatsOffsetted % BeatSnap.Value == 0 && startTime <= -2000)
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
                if (Equals(tp, Map.TimingPoints[Map.TimingPoints.Count - 1]))
                    pointLength = Track.Length + tp.MillisecondsPerBeat * numBeatsOffsetted + 2000;

                // Create all lines.
                for (var i = 0; i < pointLength / tp.MillisecondsPerBeat * BeatSnap.Value; i++)
                {
                    var time = startTime + tp.MillisecondsPerBeat / BeatSnap.Value * i;

                    var measureBeat = i / BeatSnap.Value % (int)tp.Signature == 0 && i % BeatSnap.Value == 0;

                    if (measureBeat && time >= tp.StartTime)
                        measureCount++;

                    var height = measureBeat ? 5 : 2;

                    lines.Add(new EditorPlayfieldTimelineTick(Playfield, tp, time, i, measureCount, measureBeat && time >= tp.StartTime)
                    {
                        Image = UserInterface.BlankBox,
                        Size = new ScalableVector2(Playfield.Width - 4, 0),
                        X = Playfield.AbsolutePosition.X + 2,
                        Y = Playfield.HitPositionY - time * Playfield.TrackSpeed - height,
                        Tint = GetLineColor(i % BeatSnap.Value, i),
                        Height = height
                    });
                }
            }

            CachedLines[BeatSnap.Value] = lines;
            Lines = lines;
            InitializeLinePool();
        }

        /// <summary>
        ///     Does any initializing of the pool from the current time
        /// </summary>
        private void InitializeLinePool()
        {
            LinePool = new List<EditorPlayfieldTimelineTick>();
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

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawLines(GameTime gameTime)
        {
            const float minimumDistanceToDraw = 8;

            for (var i = 0; i < LinePool.Count; i++)
            {
                var line = LinePool[i];
                var previous = LinePool.FindLastIndex(i, i, x => x.IsMeasureLine);
                var next = LinePool.FindIndex(i + 1, LinePool.Count - i - 1, x => x.IsMeasureLine);
                line.SetPosition();
                line.Tint = GetLineColor(line.Index % BeatSnap.Value, line.Index);

                if (line.IsOnScreen() &&
                    (previous is -1 ||
                        next is -1 ||
                        LinePool[previous].Y - line.Y > minimumDistanceToDraw ||
                        line.Y - LinePool[next].Y > minimumDistanceToDraw))
                    line.Draw(gameTime);
            }
        }

        /// <summary>
        ///     Gets an individual line color for the snap line.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Color GetLineColor(int val, int i)
        {
            switch (BeatSnapColor.Value)
            {
                case EditorBeatSnapColor.Default:
                    return GetDefaultLineColor(val, i);
                case EditorBeatSnapColor.Legacy:
                    return GetLegacyLineColor(val, i);
                case EditorBeatSnapColor.White:
                    return Color.White;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Color GetDefaultLineColor(int val, int i)
        {
            switch (BeatSnap.Value)
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
                            return ColorHelper.HexToColor("#0085ff");
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
                        return ColorHelper.HexToColor("#0085ff");
                default:
                    if (val == 0)
                        return Color.White;

                    return ColorHelper.HexToColor(i % 2 == 0 ? "#af4fb8" : "#4e94b7");
            }
        }

       /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Color GetLegacyLineColor(int val, int i)
        {
            switch (BeatSnap.Value)
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
                            return ColorHelper.HexToColor("#4e94b7");
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
                            return ColorHelper.HexToColor("#af4fb8");
                        default:
                            return ColorHelper.HexToColor("#4e94b7");
                    }
                // 1/3rd, 1/6th, 1/12th,
                case 3:
                case 6:
                case 12:
                    if (val % 3 == 0)
                        return Color.White;
                    else if (val == 0)
                        return Color.White;
                    else
                        return ColorHelper.HexToColor("#4e94b7");
                // 1/8th, 1//16th
                case 8:
                case 16:
                    if (val == 0)
                        return Color.White;
                    else if (( i - 1 ) % 2 == 0)
                        return ColorHelper.HexToColor("#af4fb8");
                    else if (i % 4 == 0)
                        return ColorHelper.HexToColor("#4e94b7");
                    else
                        return Colors.MainAccent;
                default:
                    if (val == 0)
                        return Color.White;

                    return ColorHelper.HexToColor(i % 2 == 0 ? "#af4fb8" : "#4e94b7");
            }
        }

       /// <summary>
       ///     Completely reinitializes the lines
       /// </summary>
       private void ReInitialize()
       {
           foreach (var lines in CachedLines)
               lines.Value.ForEach(x => x.Destroy());

           Lines.Clear();
           CachedLines.Clear();

           InitializeLines();
       }

       /// <summary>
       /// </summary>
       private void RefreshLines()
       {
           foreach (var line in Lines)
               line.SetPosition();

           InitializeLinePool();
       }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeatSnapChanged(object sender, BindableValueChangedEventArgs<int> e) => InitializeLines();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => InitializeLinePool();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackRateChanged(object sender, TrackRateChangedEventArgs e) => InitializeLinePool();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e)
            => RefreshLines();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScaleScrollSpeedWithRateChanged(object sender, BindableValueChangedEventArgs<bool> e)
            => RefreshLines();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointAdded(object sender, EditorTimingPointAddedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointRemoved(object sender, EditorTimingPointAddedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBatchRemoved(object sender, EditorTimingPointBatchRemovedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBatchAdded(object sender, EditorTimingPointBatchAddedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointOffsetChanged(object sender, EditorTimingPointOffsetChangedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointOffsetBatchChanged(object sender, EditorChangedTimingPointOffsetBatchEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBpmChanged(object sender, EditorTimingPointBpmChangedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointBpmBatchChanged(object sender, EditorChangedTimingPointBpmBatchEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointSignatureChanged(object sender, EditorTimingPointSignatureChangedEventArgs e) => ReInitialize();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimingPointSignatureBatchChanged(object sender, EditorChangedTimingPointSignatureBatchEventArgs e) => ReInitialize();
    }
}
