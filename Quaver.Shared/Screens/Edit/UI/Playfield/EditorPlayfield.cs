using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.UI.Playfield.Timeline;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
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
                        return 80;
                    case GameMode.Keys7:
                        return 70;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// </summary>
        public int HitPositionY { get; } = 860;

        /// <summary>
        ///     The speed at which the container scrolls at.
        /// </summary>
        public float TrackSpeed => ScrollSpeed.Value / (20 * Track.Rate);

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
        private List<EditorHitObject> HitObjects { get; set; }

        /// <summary>
        ///     The objects that are currently visible and ready to be drawn to the screen
        /// </summary>
        private List<EditorHitObject> HitObjectPool { get; set; }

        /// <summary>
        ///     The index of the last object that was added to the pool
        /// </summary>
        private int LastPooledHitObjectIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        private EditorPlayfieldTimeline Timeline { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="beatSnap"></param>
        /// <param name="scrollSpeed"></param>
        /// <param name="isUneditable"></param>
        public EditorPlayfield(Qua map, Bindable<SkinStore> skin, IAudioTrack track, BindableInt beatSnap, BindableInt scrollSpeed,
            Bindable<bool> anchorHitObjectsAtMidpoint, bool isUneditable = false)
        {
            Map = map;
            Skin = skin;
            Track = track;
            BeatSnap = beatSnap;
            IsUneditable = isUneditable;
            ScrollSpeed = scrollSpeed;
            AnchorHitObjectsAtMidpoint = anchorHitObjectsAtMidpoint;

            Alignment = Alignment.TopCenter;
            Tint = ColorHelper.HexToColor("#181818");
            Size = new ScalableVector2(ColumnSize * Map.GetKeyCount(), WindowManager.Height);

            CreateBorders();
            CreateDividerLines();
            CreateHitPositionLine();
            CreateTimeline();
            CreateHitObjects();
            CreateButton();

            InitializeHitObjectPool();
            Track.Seeked += OnTrackSeeked;
            ScrollSpeed.ValueChanged += OnScrollSpeedChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alignment = Alignment;
            Button.Position = new ScalableVector2(X + BorderLeft.Width / 2f, Y);
            UpdateHitObjectPool();
            Timeline.Update(gameTime);

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
            Track.Seeked -= OnTrackSeeked;

            // ReSharper disable once DelegateSubtraction
            ScrollSpeed.ValueChanged -= OnScrollSpeedChanged;

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
        private void CreateTimeline() => Timeline = new EditorPlayfieldTimeline(Map, this, Track, BeatSnap, ScrollSpeed);

        /// <summary>
        /// </summary>
        private void CreateHitObjects()
        {
            HitObjects = new List<EditorHitObject>();
            Map.HitObjects.ForEach(CreateHitObject);
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        private void CreateHitObject(HitObjectInfo info)
        {
            var ho = info.IsLongNote ? new EditorHitObjectLong(Map, this, info, Skin, Track, AnchorHitObjectsAtMidpoint)
                                      : new EditorHitObject(Map, this, info, Skin, Track, AnchorHitObjectsAtMidpoint);

            ho.SetSize();
            ho.SetPosition();

            if (ho is EditorHitObjectLong longNote)
                longNote.ResizeLongNote();

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
        ///     Does any initializing of the pool from the current time
        /// </summary>
        private void InitializeHitObjectPool()
        {
            HitObjectPool = new List<EditorHitObject>();
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
                HitObjectPool[i].SetPosition();
                HitObjectPool[i].Draw(gameTime);
            }
        }

        /// <summary>
        /// </summary>
        public void ResetObjectPositions() => HitObjects.ForEach(x => x.SetPosition());

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => InitializeHitObjectPool();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            ScheduleUpdate(() =>
            {
                foreach (var ho in HitObjects)
                {
                    ho.SetPosition();

                    if (ho is EditorHitObjectLong ln)
                        ln.ResizeLongNote();
                }

                InitializeHitObjectPool();
            });
        }
    }
}