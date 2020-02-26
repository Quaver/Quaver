using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
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
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private List<EditorHitObjectKeys> HitObjects { get; set; }

        /// <summary>
        ///     The objects that are currently visible and ready to be drawn to the screen
        /// </summary>
        private List<EditorHitObjectKeys> HitObjectPool { get; set; }

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
        /// <param name="longNoteOpacity"></param>
        /// <param name="isUneditable"></param>
        public EditorPlayfield(Qua map, EditorActionManager manager, Bindable<SkinStore> skin, IAudioTrack track, BindableInt beatSnap,
            BindableInt scrollSpeed, Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<bool> scaleScrollSpeedWithRate,
            Bindable<EditorBeatSnapColor> beatSnapColor, Bindable<bool> viewLayers, Bindable<EditorCompositionTool> tool,
            BindableInt longNoteOpacity, bool isUneditable = false)
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
                LongNoteOpacity);

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
                Alpha = 0
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
            var totalX = Button.AbsolutePosition.X + Button.Width;
            var lane = Map.GetKeyCount() - (int) Math.Round(totalX / x * 10 % 10, MidpointRounding.AwayFromZero);

            return MathHelper.Clamp(lane, 0, Map.GetKeyCount());
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
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0 || IsUneditable)
                return;

            if (!Button.IsHeld)
            {
                if (LongNoteInDrag != null && LongNoteResizeOriginalEndTime != LongNoteInDrag.Info.EndTime)
                    ActionManager.ResizeLongNote(LongNoteInDrag.Info, LongNoteResizeOriginalEndTime, LongNoteInDrag.Info.EndTime);

                LongNoteInDrag = null;
                LongNoteResizeOriginalEndTime = -1;
            }


            if (Button.IsHovered)
            {
                HandleLeftMouseClick();
                HandleRightClick();
            }

            HandleLongNoteDragging();
        }

        /// <summary>
        /// </summary>
        private void HandleLeftMouseClick()
        {
            if (!MouseManager.IsUniquePress(MouseButton.Left))
                return;

            if (Tool.Value == EditorCompositionTool.Select)
            {
                HandleHitObjectSelectTool();
                return;
            }

            HandleHitObjectPlacement();
        }

        /// <summary>
        /// </summary>
        private void HandleHitObjectSelectTool()
        {
            var hitObject = GetHoveredHitObject();

            if (hitObject != null && hitObject.Info.IsLongNote && hitObject.IsTailHovered(GetRelativeMousePosition()))
            {
                LongNoteInDrag = hitObject;
                LongNoteResizeOriginalEndTime = hitObject.Info.EndTime;
                return;
            }
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

            switch (Tool.Value)
            {
                case EditorCompositionTool.Note:
                    ActionManager.PlaceHitObject(x, time);
                    break;
                case EditorCompositionTool.LongNote:
                    hitObject = ActionManager.PlaceHitObject(x, time, 0);

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
        /// <returns></returns>
        private Vector2 GetRelativeMousePosition()
        {
            var relativeY = HitPositionY - (int) GetTimeFromY(MouseManager.CurrentState.Y);

            return new Vector2(MouseManager.CurrentState.X, relativeY);
        }
    }
}