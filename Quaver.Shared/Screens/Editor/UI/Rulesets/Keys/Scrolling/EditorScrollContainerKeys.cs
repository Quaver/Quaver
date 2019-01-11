using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling
{
    public class EditorScrollContainerKeys : Sprite
    {
        /// <summary>
        /// </summary>
        public EditorRulesetKeys Ruleset { get; }

        /// <summary>
        ///     The *actual* speed at which the container scrolls at.
        /// </summary>
        public float TrackSpeed => ConfigManager.EditorScrollSpeedKeys.Value / (20 * AudioEngine.Track.Rate);

        /// <summary>
        ///     The y positon of the track.
        /// </summary>
        public float TrackPositionY => (float) AudioEngine.Track.Time * TrackSpeed;

        /// <summary>
        ///     The size of each lane in the container.
        /// </summary>
        public int LaneSize
        {
            get
            {
                switch (Ruleset.WorkingMap.Mode)
                {
                    case GameMode.Keys4:
                        return 55;
                    case GameMode.Keys7:
                        return 45;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     The width of the divider lines of the stage.
        /// </summary>
        public int DividerLineWidth { get; } = 2;

        /// <summary>
        ///     The y position of the hit position.
        /// </summary>
        public int HitPositionY { get; } = 580;

        /// <summary>
        ///     The audio playback rate in the last frame. Used to know
        ///     when we should be updating object positions.
        /// </summary>
        private float PreviousAudioRate { get; set; } = 1.0f;

        /// <summary>
        ///     The line that defines where the hit position is.
        /// </summary>
        private Sprite HitPositionLine { get; set; }

        /// <summary>
        ///     All of the available HitObject sprites in the map.
        /// </summary>
        protected List<DrawableEditorHitObject> HitObjects { get; private set; }

        /// <summary>
        /// </summary>
        private EditorTimelineKeys Timeline { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        public EditorScrollContainerKeys(EditorRulesetKeys ruleset)
        {
            Ruleset = ruleset;
            Alignment = Alignment.MidCenter;
            Size = new ScalableVector2(LaneSize * ruleset.WorkingMap.GetKeyCount(), WindowManager.Height);
            Tint = Color.Black;
            Alpha = 1f;

            CreateBorderLines();
            CreateHitPositionLine();
            GenerateNotes();
            Timeline = new EditorTimelineKeys(Ruleset, this);
            RunObjectScreenCheckThread();

            ConfigManager.EditorScrollSpeedKeys.ValueChanged += OnScrollSpeedChanged;
        }

        public override void Update(GameTime gameTime)
        {
            if (PreviousAudioRate != AudioEngine.Track.Rate)
                ResetObjectPositions();

            PreviousAudioRate = AudioEngine.Track.Rate;
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

            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null,
                Matrix.CreateTranslation(0, TrackPositionY, 0) * WindowManager.Scale);

            foreach (var line in Timeline.Lines)
            {
                if (line.IsInView)
                    line.Draw(gameTime);
            }

            foreach (var obj in HitObjects)
            {
                if (obj.IsInView)
                    obj.Draw(gameTime);
            }

            GameBase.Game.SpriteBatch.End();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.EditorScrollSpeedKeys.ValueChanged -= OnScrollSpeedChanged;

            HitObjects.ForEach(x => x.Destroy());
            base.Destroy();
        }

        /// <summary>
        ///     Creates the lines that act as a border for the stage.
        /// </summary>
        private void CreateBorderLines()
        {
            // Left Line
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(DividerLineWidth, Height)
            };

            // Right Line
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(DividerLineWidth, Height)
            };

            for (var i = 0; i < Ruleset.WorkingMap.GetKeyCount() - 1; i++)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(DividerLineWidth, Height),
                    X = LaneSize * (i + 1)
                };
            }
        }

        /// <summary>
        ///     Creates the sprite where the hit position line is.
        /// </summary>
        private void CreateHitPositionLine() => HitPositionLine = new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Size = new ScalableVector2(Width, 6),
            Y = HitPositionY,
            Tint = Colors.SecondaryAccent
        };

        /// <summary>
        ///     Creates all of the HitObject sprites and sets their initial positions.
        /// </summary>
        private void GenerateNotes()
        {
            HitObjects = new List<DrawableEditorHitObject>();

            foreach (var h in Ruleset.WorkingMap.HitObjects)
            {
                DrawableEditorHitObject hitObject;

                if (h.IsLongNote)
                    hitObject = new DrawableEditorHitObjectLong(this, h, SkinManager.Skin.Keys[Ruleset.WorkingMap.Mode].NoteHoldHitObjects[h.Lane - 1].First(),
                        SkinManager.Skin.Keys[Ruleset.WorkingMap.Mode].NoteHoldBodies[h.Lane - 1].First(),
                        SkinManager.Skin.Keys[Ruleset.WorkingMap.Mode].NoteHoldEnds[h.Lane - 1]);
                else
                    hitObject = new DrawableEditorHitObject(this, h, SkinManager.Skin.Keys[Ruleset.WorkingMap.Mode].NoteHitObjects[h.Lane - 1].First());

                hitObject.Alignment = Alignment.TopLeft;

                hitObject.X = ScreenRectangle.X + LaneSize * (h.Lane - 1) + DividerLineWidth * 2;
                hitObject.Width = LaneSize - DividerLineWidth;

                // Make sure the width of the long note is updated if this object is indeed an LN.
                if (hitObject is DrawableEditorHitObjectLong longNote)
                {
                    longNote.Body.Width = hitObject.Width;
                    longNote.Tail.Width = hitObject.Width;
                }

                HitObjects.Add(hitObject);
            }
        }

        /// <summary>
        ///     Ran on a separate thread to
        /// </summary>
        private void RunObjectScreenCheckThread() => ThreadScheduler.Run(() =>
        {
            while (!Ruleset.Screen.Exiting)
            {
                CheckIfObjectsOnScreen();
                Thread.Sleep(30);
            }
        });

        /// <summary>
        ///     - Makes sure only notes that are on-screen are marked as in view.
        ///       These objects are the ones that actually get drawn on-screen.
        /// </summary>
        public void CheckIfObjectsOnScreen()
        {
            foreach (var obj in HitObjects)
                obj.IsInView = obj.CheckIfOnScreen();

            foreach (var line in Timeline.Lines)
                line.IsInView = line.CheckIfOnScreen();
        }

        /// <summary>
        /// </summary>
        public void ResetObjectPositions() => HitObjects.ForEach(x => x.SetPositionY());

        /// <summary>
        ///     Called when the user changes the scroll speed of the map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            NotificationManager.Show(NotificationLevel.Info, $"Scroll Speed Changed to: {e.Value}");
            HitObjects.ForEach(x => x.SetPositionY());
        }
    }
}