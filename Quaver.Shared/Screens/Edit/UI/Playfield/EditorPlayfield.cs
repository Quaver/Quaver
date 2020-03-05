using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MoreLinq;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Graphs;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.HitObjects;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.UI.Footer;
using Quaver.Shared.Screens.Edit.UI.Playfield.Timeline;
using Quaver.Shared.Screens.Edit.UI.Playfield.Zoom;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Playfield
{
    public class EditorPlayfield : Sprite
    {
        /// <summary>
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Bindable<SkinStore> Skin { get; }

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
        private Bindable<bool> AnchorHitObjectsAtMidpoint { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> ScaleScrollSpeedWithAudioRate { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorBeatSnapColor> BeatSnapColor { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> ViewLayers { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorCompositionTool> Tool { get; }

        /// <summary>
        /// </summary>
        private BindableInt LongNoteOpacity { get; }

        /// <summary>
        ///     If true, this playfield is unable to be edited/interacted with. This is purely for viewing
        /// </summary>
        public bool IsUneditable { get; }

        /// <summary>
        ///     The size of each column in the playfield
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int ColumnSize
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return 74;
                    case GameMode.Keys7:
                        return 70;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// </summary>
        public int HitPositionY { get; } = 820;

        /// <summary>
        ///     The speed at which the container scrolls at.
        /// </summary>
        public float TrackSpeed => ScrollSpeed.Value / (ScaleScrollSpeedWithAudioRate.Value ? 20f * Track.Rate : 20f);

        /// <summary>
        ///     The current y positon of the playfield track
        /// </summary>
        public float TrackPositionY => (float) Track.Time * TrackSpeed;

        /// <summary>
        /// </summary>
        public Sprite BorderLeft { get; private set; }

        /// <summary>
        /// </summary>
        private Sprite BorderRight { get; set; }

        /// <summary>
        /// </summary>
        private List<Sprite> DividerLines { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HitPositionLine { get; set; }

        /// <summary>
        /// </summary>
        public ImageButton Button { get; private set; }

        /// <summary>
        /// </summary>
        private List<EditorHitObjectKeys> HitObjects { get; set; }

        /// <summary>
        ///     The objects that are currently visible and ready to be drawn to the screen
        /// </summary>
        private List<EditorHitObjectKeys> HitObjectPool { get; set; }

        /// <summary>
        /// </summary>
        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorLayerInfo> SelectedLayer { get; }

        /// <summary>
        /// </summary>
        private EditorLayerInfo DefaultLayer { get; }

        /// <summary>
        ///     The index of the last object that was added to the pool
        /// </summary>
        private int LastPooledHitObjectIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        private EditorPlayfieldTimeline Timeline { get; set; }

        /// <summary>
        /// </summary>
        private DifficultySeekBar SeekBar { get; set; }

        /// <summary>
        /// </summary>
        private EditorPlayfieldZoom Zoom { get; set; }

        /// <summary>
        ///     The long note that is currently being dragged
        /// </summary>
        private EditorHitObjectKeys LongNoteInDrag { get; set; }

        /// <summary>
        ///     The start time of <see cref="LongNoteInDrag"/> when
        ///     it is initially being resized. This is used to compare
        ///     the end times, so an action can be made to store the resize change
        /// </summary>
        private int LongNoteResizeOriginalEndTime { get; set; } = -1;

        /// <summary>
        ///     The initial position of the mouse when the user begins to drag notes
        /// </summary>
        private Vector2? NoteMoveInitialMousePosition { get; set; }

        /// <summary>
        ///     The hitobject that is currently being dragged to move the selected objects
        /// </summary>
        private EditorHitObjectKeys HitObjectInDrag { get; set; }

        /// <summary>
        ///     The time at which the user began dragging the notes
        /// </summary>
        private int TimeDragStart { get; set; }

        /// <summary>
        ///     The offset of the object dragging in the previous frame. Used to calculate the final
        ///     time of the notes between drags
        /// </summary>
        public int PreviousDragOffset { get; private set; }

        /// <summary>
        ///     The offset of the lane the user was dragging in the previous frame. This is used to calculate
        ///     which lanes the notes should be dragged to
        /// </summary>
        public int PreviousLaneDragOffset { get; private set; }

        /// <summary>
        ///     The offset of the columns moved in the current drag
        /// </summary>
        public int ColumnOffset { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="manager"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="beatSnap"></param>
        /// <param name="scrollSpeed"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        /// <param name="scaleScrollSpeedWithRate"></param>
        /// <param name="beatSnapColor"></param>
        /// <param name="viewLayers"></param>
        /// <param name="tool"></param>
        /// <param name="longNoteOpacity"></param>
        /// <param name="selectedHitObjects"></param>
        /// <param name="selectedLayer"></param>
        /// <param name="defaultLayer"></param>
        /// <param name="isUneditable"></param>
        public EditorPlayfield(Qua map, EditorActionManager manager, Bindable<SkinStore> skin, IAudioTrack track, BindableInt beatSnap,
            BindableInt scrollSpeed, Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<bool> scaleScrollSpeedWithRate,
            Bindable<EditorBeatSnapColor> beatSnapColor, Bindable<bool> viewLayers, Bindable<EditorCompositionTool> tool,
            BindableInt longNoteOpacity, BindableList<HitObjectInfo> selectedHitObjects, Bindable<EditorLayerInfo> selectedLayer,
            EditorLayerInfo defaultLayer, bool isUneditable = false)
        {
            Map = map;
            ActionManager = manager;
            Skin = skin;
            Track = track;
            BeatSnap = beatSnap;
            IsUneditable = isUneditable;
            ScrollSpeed = scrollSpeed;
            AnchorHitObjectsAtMidpoint = anchorHitObjectsAtMidpoint;
            ScaleScrollSpeedWithAudioRate = scaleScrollSpeedWithRate;
            BeatSnapColor = beatSnapColor;
            ViewLayers = viewLayers;
            Tool = tool;
            LongNoteOpacity = longNoteOpacity;
            SelectedHitObjects = selectedHitObjects;
            SelectedLayer = selectedLayer;
            DefaultLayer = defaultLayer;

            Alignment = Alignment.TopCenter;
            Tint = ColorHelper.HexToColor("#181818");
            Size = new ScalableVector2(ColumnSize * Map.GetKeyCount(), WindowManager.Height);

            CreateBorders();
            CreateDividerLines();
            CreateHitPositionLine();
            CreateTimeline();
            CreateHitObjects();
            CreateButton();
            CreateSeekBar();
            CreateZoom();

            InitializeHitObjectPool();
            Track.Seeked += OnTrackSeeked;
            Track.RateChanged += OnTrackRateChanged;
            ScrollSpeed.ValueChanged += OnScrollSpeedChanged;
            ScaleScrollSpeedWithAudioRate.ValueChanged += OnScaleScrollSpeedWithRateChanged;

            ActionManager.HitObjectPlaced += OnHitObjectPlaced;
            ActionManager.HitObjectRemoved += OnHitObjectRemoved;
            ActionManager.HitObjectBatchRemoved += OnHitObjectBatchRemoved;
            ActionManager.HitObjectBatchPlaced += OnHitObjectBatchPlaced;
            ActionManager.HitObjectsFlipped += OnHitObjectsFlipped;
            ActionManager.HitObjectsMoved += OnHitObjectsMoved;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alignment = Alignment;
            Button.Position = new ScalableVector2(X + BorderLeft.Width / 2f, Y);
            Button.Update(gameTime);
            UpdateHitObjectPool();
            Timeline.Update(gameTime);
            HandleInput();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored
            }

            var transformMatrix = Matrix.CreateTranslation(0, TrackPositionY, 0) * WindowManager.Scale;

            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, transformMatrix);
            Timeline.Draw(gameTime);
            DrawHitObjects(gameTime);
            GameBase.Game.SpriteBatch.End();

            // Draw the button on top of the hitobjects because it serves as a dimming
            Button.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            HitObjects.ForEach(x => x.Destroy());
            Timeline?.Destroy();
            Button.Destroy();

            Track.Seeked -= OnTrackSeeked;
            Track.RateChanged -= OnTrackRateChanged;

            // ReSharper disable twice DelegateSubtraction
            ScrollSpeed.ValueChanged -= OnScrollSpeedChanged;
            ScaleScrollSpeedWithAudioRate.ValueChanged -= OnScaleScrollSpeedWithRateChanged;
            ActionManager.HitObjectPlaced -= OnHitObjectPlaced;
            ActionManager.HitObjectRemoved -= OnHitObjectRemoved;
            ActionManager.HitObjectBatchRemoved -= OnHitObjectBatchRemoved;
            ActionManager.HitObjectBatchPlaced -= OnHitObjectBatchPlaced;
            ActionManager.HitObjectsFlipped -= OnHitObjectsFlipped;
            ActionManager.HitObjectsMoved -= OnHitObjectsMoved;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBorders()
        {
            var color = ColorHelper.HexToColor("#808080");
            const int width = 2;

            BorderLeft = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(width, Height),
                Tint = color
            };

            BorderRight = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(width, Height),
                Tint = color
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDividerLines()
        {
            DividerLines = new List<Sprite>();

            for (var i = 0; i < Map.GetKeyCount() - 1; i++)
            {
                DividerLines.Add(new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(2, Height),
                    X = ColumnSize * (i + 1),
                    Alpha = 0.35f
                });
            }
        }

        /// <summary>
        /// </summary>
        private void CreateHitPositionLine() => HitPositionLine = new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Y = HitPositionY,
            Size = new ScalableVector2(Width - BorderLeft.Width * 2, 6),
            Tint = Colors.MainBlue
        };

        /// <summary>
        /// </summary>
        private void CreateTimeline() => Timeline = new EditorPlayfieldTimeline(Map, this, Track, BeatSnap, ScrollSpeed,
            ScaleScrollSpeedWithAudioRate, BeatSnapColor);

        /// <summary>
        /// </summary>
        private void CreateHitObjects()
        {
            HitObjects = new List<EditorHitObjectKeys>();
            Map.HitObjects.ForEach(x => CreateHitObject(x));
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="insertAtIndex"></param>
        private void CreateHitObject(HitObjectInfo info, bool insertAtIndex = false)
        {
            var ho = new EditorHitObjectKeys(Map, this, info, Skin, Track, AnchorHitObjectsAtMidpoint, ViewLayers,
                LongNoteOpacity, SelectedHitObjects, DefaultLayer);

            ho.SetSize();
            ho.SetPosition();

            if (ho.Info.IsLongNote)
                ho.UpdateLongNoteSizeAndAlpha();

            if (insertAtIndex)
            {
                HitObjects.Add(ho);
                HitObjects = HitObjects.OrderBy(x => x.Info.StartTime).ToList();
            }
            else
                HitObjects.Add(ho);
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Size = new ScalableVector2(Width - BorderLeft.Width * 4, Height),
                Tint = Color.Black,
                Alpha = 0,
                Depth = 2
            };

            if (IsUneditable)
                Button.Alpha = 0.45f;
        }

        /// <summary>
        /// </summary>
        private void CreateSeekBar()
        {
            if (IsUneditable)
                return;

            SeekBar = new DifficultySeekBar(Map, ModIdentifier.None,
                new ScalableVector2(54, WindowManager.Height - EditorFooter.HEIGHT - 4), 85, 5, Track,
                true, 0.85f)
            {
                Parent = this,
                Tint = ColorHelper.HexToColor("#181818"),
            };

            SeekBar.X -= SeekBar.Width + 26;

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = SeekBar,
                Size = new ScalableVector2(2, SeekBar.Height),
                Tint = ColorHelper.HexToColor("#808080")
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = SeekBar,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(2, SeekBar.Height),
                Tint = ColorHelper.HexToColor("#808080")
            };
        }

        /// <summary>
        ///     Creates <see cref="Zoom"/>. Not create if playfield is <see cref="IsUneditable"/>
        /// </summary>
        private void CreateZoom()
        {
            if (IsUneditable)
                return;

            Zoom = new EditorPlayfieldZoom(ScrollSpeed)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = 100
            };

            Zoom.X += Zoom.Width + 10;
        }

        /// <summary>
        ///     Does any initializing of the pool from the current time
        /// </summary>
        private void InitializeHitObjectPool()
        {
            HitObjectPool = new List<EditorHitObjectKeys>();
            LastPooledHitObjectIndex = -1;

            for (var i = 0; i < HitObjects.Count; i++)
            {
                var hitObject = HitObjects[i];

                if (!hitObject.IsOnScreen())
                    continue;

                HitObjectPool.Add(hitObject);
                LastPooledHitObjectIndex = i;
            }
        }

        /// <summary>
        ///     Updates the object pool to get rid of old/out of view objects
        /// </summary>
        private void UpdateHitObjectPool()
        {
            // Check the objects that are in the pool currently to see if they're still in view.
            // if they're not, remove them.
            for (var i = HitObjectPool.Count - 1; i >= 0; i--)
            {
                var hitObject = HitObjectPool[i];

                if (!hitObject.IsOnScreen())
                    HitObjectPool.Remove(hitObject);
            }

            // Add any objects that are now on-screen
            for (var i = LastPooledHitObjectIndex + 1; i < HitObjects.Count; i++)
            {
                var hitObject = HitObjects[i];

                if (!hitObject.IsOnScreen())
                    break;

                HitObjectPool.Add(hitObject);
                LastPooledHitObjectIndex = i;
            }
        }

        /// <summary>
        ///     Draws all of the currently available hitobjects.
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawHitObjects(GameTime gameTime)
        {
            for (var i = 0; i < HitObjectPool.Count; i++)
            {
                var hitObject = HitObjectPool[i];

                hitObject.SetPosition();
                hitObject.UpdateLongNoteSizeAndAlpha();
                hitObject.Draw(gameTime);
            }
        }

        /// <summary>
        /// </summary>
        public void ResetObjectPositions() => HitObjects.ForEach(x =>
        {
            x.SetPosition();
            x.UpdateLongNoteSizeAndAlpha();
        });

        /// <summary>
        ///     Gets the audio time from a y position.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public double GetTimeFromY(float y) => TrackPositionY + (HitPositionY - y);

        /// <summary>
        ///     Returns the nearest tick time from a time and beat snap
        /// </summary>
        /// <param name="time"></param>
        /// <param name="beatSnap"></param>
        /// <returns></returns>
        public int GetNearestTickFromTime(int time, int beatSnap)
        {
            var timingPoint = Map.GetTimingPointAt(time);

            if (timingPoint == null)
                return time;

            var timeFwd = (int) AudioEngine.GetNearestSnapTimeFromTime(Map, Direction.Forward, beatSnap, time);
            var timeBwd = (int) AudioEngine.GetNearestSnapTimeFromTime(Map, Direction.Backward, beatSnap, time);

            var fwdDiff = Math.Abs(time - timeFwd);
            var bwdDiff = Math.Abs(time - timeBwd);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bwdDiff < fwdDiff)
                time = timeBwd;
            else if (fwdDiff < bwdDiff)
                time = timeFwd;

            return time;
        }

        /// <summary>
        ///     Gets the lane the mouse is in based on the mouse's x position.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetLaneFromX(float x)
        {
            var percentage = (x - AbsolutePosition.X) / AbsoluteSize.X;
            var lane = Map.GetKeyCount() * percentage + 1;

            return (int) MathHelper.Clamp(lane, 1, Map.GetKeyCount());
        }

        /// <summary>
        ///     Gets an object that is currently hovered
        /// </summary>
        /// <returns></returns>
        public EditorHitObjectKeys GetHoveredHitObject()
        {
            foreach (var h in HitObjects)
            {
                if (h.IsHovered(GetRelativeMousePosition()))
                    return h;
            }

            return null;
        }

        /// <summary>
        ///     Returns a hitobject at a given time and lane
        /// </summary>
        /// <param name="time"></param>
        /// <param name="lane"></param>
        /// <returns></returns>
        public HitObjectInfo GetHitObjectAtTimeAndLane(int time, int lane)
        {
            for (var i = 0; i < Map.HitObjects.Count; i++)
            {
                var ho = Map.HitObjects[i];

                if (ho.Lane != lane)
                    continue;

                if (!ho.IsLongNote && ho.StartTime == time)
                    return ho;

                if (ho.IsLongNote && time >= ho.StartTime && time <= ho.EndTime)
                    return ho;
            }

            return null;
        }

        /// <summary>
        /// </summary>
        private void RefreshHitObjects() => ScheduleUpdate(() =>
        {
            ResetObjectPositions();
            InitializeHitObjectPool();
        });

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => InitializeHitObjectPool();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e) => RefreshHitObjects();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScaleScrollSpeedWithRateChanged(object sender, BindableValueChangedEventArgs<bool> e) => RefreshHitObjects();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackRateChanged(object sender, TrackRateChangedEventArgs e) => RefreshHitObjects();

        /// <summary>
        ///     Called when the user placed down a HitObject
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHitObjectPlaced(object sender, EditorHitObjectPlacedEventArgs e)
        {
            if (IsUneditable)
                return;

            CreateHitObject(e.HitObject, true);
            InitializeHitObjectPool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnHitObjectRemoved(object sender, EditorHitObjectRemovedEventArgs e)
        {
            if (IsUneditable)
                return;

            var ho = HitObjects.Find(x => x.Info == e.HitObject);

            if (ho == null)
                return;

            ho.Destroy();
            HitObjects.Remove(ho);

            InitializeHitObjectPool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHitObjectBatchRemoved(object sender, EditorHitObjectBatchRemovedEventArgs e)
        {
            if (IsUneditable)
                return;

            foreach (var obj in e.HitObjects)
            {
                var drawable = HitObjects.Find(x => x.Info == obj);

                if (drawable == null)
                    continue;

                drawable.Destroy();
                HitObjects.Remove(drawable);
            }

            InitializeHitObjectPool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnHitObjectBatchPlaced(object sender, EditorHitObjectBatchPlacedEventArgs e)
        {
            if (IsUneditable)
                return;

            foreach (var obj in e.HitObjects)
                CreateHitObject(obj);

            // Make sure to sort, because we're adding multiple at one time, and we don't want to sort
            // on every iteration of CreateHitObject.
            HitObjects = HitObjects.OrderBy(x => x.Info.StartTime).ToList();

            InitializeHitObjectPool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHitObjectsFlipped(object sender, EditorHitObjectsFlippedEventArgs e)
        {
            if (IsUneditable)
                return;

            RefreshHitObjectBatch(e.HitObjects);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHitObjectsMoved(object sender, EditorHitObjectsMovedEventArgs e)
        {
            if (IsUneditable)
                return;

            RefreshHitObjectBatch(e.HitObjects);
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0 || IsUneditable)
                return;

            if (!Button.IsHeld)
            {
                // Create an action for the long note resizing when the user lets go
                if (LongNoteInDrag != null && LongNoteResizeOriginalEndTime != LongNoteInDrag.Info.EndTime && LongNoteResizeOriginalEndTime != -1)
                    ActionManager.ResizeLongNote(LongNoteInDrag.Info, LongNoteResizeOriginalEndTime, LongNoteInDrag.Info.EndTime);

                LongNoteInDrag = null;
                LongNoteResizeOriginalEndTime = -1;

                // Create an action for the the user dragging/moving the notes
                if ((NoteMoveInitialMousePosition != null && PreviousLaneDragOffset != 0)
                    || (HitObjectInDrag != null && PreviousDragOffset != 0))
                {
                    var action = new EditorActionMoveHitObjects(ActionManager, Map, new List<HitObjectInfo>(SelectedHitObjects.Value),
                        ColumnOffset, PreviousDragOffset, false);

                    ActionManager.Perform(action);
                }

                NoteMoveInitialMousePosition = null;
                HitObjectInDrag = null;
                TimeDragStart = 0;
                PreviousDragOffset = 0;
                PreviousLaneDragOffset = 0;
                ColumnOffset = 0;
            }

            if (Button.IsHovered)
            {
                HandleLeftMouseClick();
                HandleRightClick();
            }

            HandleLongNoteDragging();
            HandleMovingObjects();
        }

        /// <summary>
        /// </summary>
        private void HandleLeftMouseClick()
        {
            if (!MouseManager.IsUniquePress(MouseButton.Left))
                return;

            var hitObject = GetHoveredHitObject();

            if (hitObject == null)
                SelectedHitObjects.Clear();

            if (Tool.Value == EditorCompositionTool.Select)
            {
                HandleHitObjectSelectTool(hitObject);
                return;
            }

            HandleHitObjectPlacement();
        }

        /// <summary>
        /// </summary>
        private void HandleHitObjectSelectTool(EditorHitObjectKeys hoveredObject)
        {
            if (hoveredObject == null)
                return;

            // Begin dragging the long note
            if (hoveredObject.Info.IsLongNote && hoveredObject.IsTailHovered(GetRelativeMousePosition()))
            {
                LongNoteInDrag = hoveredObject;
                LongNoteResizeOriginalEndTime = hoveredObject.Info.EndTime;
                return;
            }

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
            {
                if (SelectedHitObjects.Value.Contains(hoveredObject.Info))
                    SelectedHitObjects.Remove(hoveredObject.Info);
                else
                    SelectedHitObjects.Add(hoveredObject.Info);

                return;
            }

            if (!SelectedHitObjects.Value.Contains(hoveredObject.Info))
                SelectedHitObjects.Clear();

            if (!SelectedHitObjects.Value.Contains(hoveredObject.Info))
                SelectedHitObjects.Add(hoveredObject.Info);
        }

        /// <summary>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleHitObjectPlacement()
        {
            var time = (int) Math.Round(GetTimeFromY(MouseManager.CurrentState.Y) / TrackSpeed, MidpointRounding.AwayFromZero);
            time = GetNearestTickFromTime(time, BeatSnap.Value);

            var x = GetLaneFromX(MouseManager.CurrentState.X);

            if (GetHitObjectAtTimeAndLane(time, x) != null)
                return;

            HitObjectInfo hitObject;

            var layer = 0;

            if (SelectedLayer.Value != null)
                layer = Map.EditorLayers.IndexOf(SelectedLayer.Value) + 1;

            switch (Tool.Value)
            {
                case EditorCompositionTool.Note:
                    ActionManager.PlaceHitObject(x, time, 0, layer);
                    break;
                case EditorCompositionTool.LongNote:
                    hitObject = ActionManager.PlaceHitObject(x, time, 0, layer);

                    var ln = HitObjects.Find(y => y.Info == hitObject);
                    LongNoteInDrag = ln;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        private void HandleRightClick()
        {
            if (!MouseManager.IsUniquePress(MouseButton.Right))
                return;

            var ho = GetHoveredHitObject();

            if (ho == null)
                return;

            ActionManager.RemoveHitObject(ho.Info);
            SelectedHitObjects.Remove(ho.Info);
        }

        /// <summary>
        /// </summary>
        private void HandleLongNoteDragging()
        {
            if (LongNoteInDrag == null || !Button.IsHeld)
                return;

            var time = (int) Math.Round(GetTimeFromY(MouseManager.CurrentState.Y) / TrackSpeed, MidpointRounding.AwayFromZero);
            time = GetNearestTickFromTime(time, BeatSnap.Value);

            // Change the object back to a normal note
            if (time <= LongNoteInDrag.Info.StartTime)
            {
                LongNoteInDrag.Info.EndTime = 0;
                return;
            }

            LongNoteInDrag.Info.EndTime = time;
        }

        /// <summary>
        /// </summary>
        private void HandleMovingObjects()
        {
            if (SelectedHitObjects.Value.Count == 0 || !Button.IsHeld)
                return;

            // First time clicking to drag, so initialize the properties to handle the drag
            if (NoteMoveInitialMousePosition == null && HitObjectInDrag == null)
            {
                var hoveredObject = GetHoveredHitObject();

                if (hoveredObject == null)
                    return;

                NoteMoveInitialMousePosition = MouseManager.CurrentState.Position;
                HitObjectInDrag = hoveredObject;
                TimeDragStart = hoveredObject.Info.StartTime;
                PreviousDragOffset = 0;
                PreviousLaneDragOffset = 0;
                ColumnOffset = 0;
            }

            // Prevent snapping the notes if the user hasn't moved the mouse yet
            if (NoteMoveInitialMousePosition - MouseManager.CurrentState.Position == Vector2.Zero)
                return;

            // Start dragging all objects to the given y position
            var time = GetNearestTickFromTime((int) Math.Round(GetTimeFromY(MouseManager.CurrentState.Y) / TrackSpeed,
                MidpointRounding.AwayFromZero), BeatSnap.Value);

            var offset = (int) Math.Round((float) (time - TimeDragStart), MidpointRounding.AwayFromZero);

            // ReSharper disable once PossibleInvalidOperationException
            var laneOffset = GetLaneFromX(MouseManager.CurrentState.X) - GetLaneFromX(NoteMoveInitialMousePosition.Value.X);

            // Prevent dragging if there is no need to
            if (LongNoteInDrag != null || time < 0)
                return;

            var dragXAllowed = true;

            // Check to see if the user is allowed to drag the notes between lanes.
            // If the user has 2+ objects selected with objects in the side lanes (1, 4/7), disallow the drag,
            // so the pattern does not change
            if (SelectedHitObjects.Value.Count > 1 && PreviousLaneDragOffset != laneOffset)
            {
                var columnOffset = laneOffset - PreviousLaneDragOffset;

                var leftColumn = int.MaxValue;
                var rightColumn = 0;

                foreach (var obj in SelectedHitObjects.Value)
                {
                    leftColumn = Math.Min(obj.Lane, leftColumn);
                    rightColumn = Math.Max(obj.Lane, rightColumn);
                }

                if (laneOffset > PreviousLaneDragOffset)
                    dragXAllowed = rightColumn + columnOffset <= Map.GetKeyCount();
                else if (laneOffset < PreviousLaneDragOffset)
                    dragXAllowed = leftColumn + columnOffset >= 1;
            }

            // Handle drag
            for (var i = 0; i < SelectedHitObjects.Value.Count; i++)
            {
                var ho = SelectedHitObjects.Value[i];

                if (PreviousDragOffset != offset)
                {
                    var startTime = ho.StartTime + (offset - PreviousDragOffset);
                    ho.StartTime = MathHelper.Clamp(startTime, 0, (int) Track.Length);

                    // Only change the end time of long notes if the user drags it 0 or above.
                    // Notes should never begin before the maps actually start. This handles the case
                    // of the end time being automatically clamped to zero if the user tries to drag
                    // before the start of the map
                    if (ho.IsLongNote && startTime >= 0)
                        ho.EndTime = MathHelper.Clamp(ho.EndTime + (offset - PreviousDragOffset), 0, (int) Track.Length);
                }

                // Move the x position of the note
                if (PreviousLaneDragOffset == laneOffset || !dragXAllowed)
                    continue;

                var column = ho.Lane;
                ho.Lane = MathHelper.Clamp(ho.Lane + (laneOffset - PreviousLaneDragOffset), 1, Map.GetKeyCount());

                // Only update the column offset on the first iteration
                if (i == 0)
                    ColumnOffset += ho.Lane - column;
            }

            PreviousDragOffset = offset;
            PreviousLaneDragOffset = laneOffset;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Vector2 GetRelativeMousePosition()
        {
            var relativeY = HitPositionY - (int) GetTimeFromY(MouseManager.CurrentState.Y);
            return new Vector2(MouseManager.CurrentState.X, relativeY);
        }

        /// <summary>
        /// </summary>
        /// <param name="hitobjects"></param>
        private void RefreshHitObjectBatch(List<HitObjectInfo> hitobjects)
        {
            foreach (var obj in hitobjects)
            {
                var drawable = HitObjects.Find(x => x.Info == obj);

                if (drawable == null)
                    continue;

                drawable.Refresh();
            }
        }
    }
}