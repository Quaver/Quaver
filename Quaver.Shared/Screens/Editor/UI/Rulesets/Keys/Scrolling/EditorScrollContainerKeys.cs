/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Editor.UI.Layering;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Components;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Logging;
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
                        return 60;
                    case GameMode.Keys7:
                        return 50;
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
        public int HitPositionY { get; } = 575;

        /// <summary>
        ///     The audio playback rate in the last frame. Used to know
        ///     when we should be updating object positions.
        /// </summary>
        private float PreviousAudioRate { get; set; } = 1.0f;

        /// <summary>
        ///     The line that defines where the hit position is.
        /// </summary>
        public Sprite HitPositionLine { get; private set; }

        /// <summary>
        ///     All of the available HitObject sprites in the map.
        /// </summary>
        public List<DrawableEditorHitObject> HitObjects { get; private set; }

        /// <summary>
        /// </summary>
        public EditorTimelineKeys Timeline { get; }

        /// <summary>
        /// </summary>
        private TimelineZoomer Zoomer { get; set; }

        /// <summary>
        ///     The lines that divide the lanes
        /// </summary>
        private List<Sprite> LaneDividerLines { get; set; }

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
            Alpha = 0.85f;

            CreateBorderLines();
            CreateHitPositionLine();
            GenerateNotes();

            Timeline = new EditorTimelineKeys(Ruleset, this);

            Zoomer = new TimelineZoomer
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(Width, 90),
            };

            RunObjectScreenCheckThread();

            ConfigManager.EditorScrollSpeedKeys.ValueChanged += OnScrollSpeedChanged;
            ConfigManager.EditorShowLaneDividerLines.ValueChanged += OnShowDividerLinesChanged;
            ConfigManager.EditorHitObjectsMidpointAnchored.ValueChanged += OnHitObjectMidpointAnchoredChanged;
            ConfigManager.EditorViewLayers.ValueChanged += OnEditorUseLayerHitObjects;

            var view = (EditorScreenView) Ruleset.Screen.View;
            view.LayerCompositor.LayerUpdated += OnEditorLayerUpdated;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
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

            var transformMatrix = Matrix.CreateTranslation(0, TrackPositionY, 0) * WindowManager.Scale;

            // Handle upscroll by flipping/rotating the matrix.
            /*if (GameplayRulesetKeys.ScrollDirection == ScrollDirection.Up)
             {
                 transformMatrix = transformMatrix * Matrix.CreateTranslation(-ConfigManager.WindowWidth.Value / 2f, -ConfigManager.WindowHeight.Value / 2f, 0f)
                                                   * Matrix.CreateRotationZ(MathHelper.ToRadians(180)) *
                                                   Matrix.CreateTranslation(ConfigManager.WindowWidth.Value / 2f, ConfigManager.WindowHeight.Value / 2f, 0f);
            }*/

            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, transformMatrix);

            Timeline.Draw(gameTime);
            DrawHitObjects(gameTime);

            GameBase.Game.SpriteBatch.End();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            ConfigManager.EditorScrollSpeedKeys.ValueChanged -= OnScrollSpeedChanged;
            ConfigManager.EditorShowLaneDividerLines.ValueChanged -= OnShowDividerLinesChanged;
            ConfigManager.EditorHitObjectsMidpointAnchored.ValueChanged -= OnHitObjectMidpointAnchoredChanged;
            ConfigManager.EditorViewLayers.ValueChanged -= OnEditorUseLayerHitObjects;

            var view = (EditorScreenView) Ruleset.Screen.View;
            view.LayerCompositor.LayerUpdated -= OnEditorLayerUpdated;

            HitObjects.ForEach(x => x.Destroy());
            Timeline.Destroy();
            base.Destroy();
        }

        /// <summary>
        ///     Creates the lines that act as a border for the stage.
        /// </summary>
        private void CreateBorderLines()
        {
            LaneDividerLines = new List<Sprite>
            {
                new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(DividerLineWidth, Height),
                    Visible = ConfigManager.EditorShowLaneDividerLines.Value,
                    Alpha = 0.45f
                },
                new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopRight,
                    Size = new ScalableVector2(DividerLineWidth, Height),
                    Visible = ConfigManager.EditorShowLaneDividerLines.Value,
                    Alpha = 0.45f
                }
            };

            for (var i = 0; i < Ruleset.WorkingMap.GetKeyCount() - 1; i++)
            {
                LaneDividerLines.Add(new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(DividerLineWidth, Height),
                    X = LaneSize * (i + 1),
                    Visible = ConfigManager.EditorShowLaneDividerLines.Value,
                    Alpha = 0.45f
                });
            }
        }

        /// <summary>
        ///     Creates the sprite where the hit position line is.
        /// </summary>
        private void CreateHitPositionLine()
        {
            var y = HitPositionY;

            /*if (GameplayRulesetKeys.ScrollDirection == ScrollDirection.Up)
                y = (int) WindowManager.Height - HitPositionY;*/

            HitPositionLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(Width, 6),
                Y = y,
                Tint = Colors.SecondaryAccent
            };
        }

        /// <summary>
        ///     Creates all of the HitObject sprites and sets their initial positions.
        /// </summary>
        private void GenerateNotes()
        {
            HitObjects = new List<DrawableEditorHitObject>();

            foreach (var h in Ruleset.WorkingMap.HitObjects)
                AddHitObjectSprite(h);
        }

        /// <summary>
        ///     Draws all of the currently available hitobjects.
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawHitObjects(GameTime gameTime)
        {
            foreach (var obj in new List<DrawableEditorHitObject>(HitObjects))
            {
                if (obj.IsInView)
                    obj.Draw(gameTime);
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
            lock (HitObjects)
            {
                foreach (var obj in new List<DrawableEditorHitObject>(HitObjects))
                {
                    if (obj != null)
                        obj.IsInView = obj.CheckIfOnScreen();
                }
            }

            foreach (var line in new List<TimelineTickLine>(Timeline.Lines))
                line.IsInView = line.CheckIfOnScreen();
        }

        /// <summary>
        /// </summary>
        public void ResetObjectPositions(bool positionSnapLines = true)
        {
            HitObjects.ForEach(x =>
            {
                x.SetPositionY();

                if (x is DrawableEditorHitObjectLong obj)
                    obj.ResizeLongNote();
            });

            if (positionSnapLines)
                Timeline.RepositionLines();
        }

        /// <summary>
        ///     Adds a HitObject sprite to the container.
        /// </summary>
        public void AddHitObjectSprite(HitObjectInfo h)
        {
            DrawableEditorHitObject hitObject;

            var skin = SkinManager.Skin.Keys[Ruleset.WorkingMap.Mode];
            var index = skin.ColorObjectsBySnapDistance ? HitObjectManager.GetBeatSnap(h, h.GetTimingPoint(Ruleset.WorkingMap.TimingPoints)) : 0;

            if (h.IsLongNote)
            {
                if (ConfigManager.EditorViewLayers.Value)
                {
                    hitObject = new DrawableEditorHitObjectLong(this, h,
                        skin.EditorLayerNoteHitObjects[h.Lane - 1],
                        skin.EditorLayerNoteHoldBodies[h.Lane - 1],
                        skin.EditorLayerNoteHoldEnds[h.Lane - 1]);
                }
                else
                {
                    hitObject = new DrawableEditorHitObjectLong(this, h,
                        skin.NoteHoldHitObjects[h.Lane - 1][index],
                        skin.NoteHoldBodies[h.Lane - 1].First(),
                        skin.NoteHoldEnds[h.Lane - 1]);
                }
            }
            else
            {
                if (ConfigManager.EditorViewLayers.Value)
                    hitObject = new DrawableEditorHitObject(this, h, skin.EditorLayerNoteHitObjects[h.Lane - 1]);
                else
                    hitObject = new DrawableEditorHitObject(this, h, skin.NoteHitObjects[h.Lane - 1][index]);
            }

            hitObject.Alignment = Alignment.TopLeft;
            hitObject.X = ScreenRectangle.X + LaneSize * (h.Lane - 1) + DividerLineWidth;
            hitObject.Width = LaneSize - DividerLineWidth;

            // Make sure the width of the long note is updated if this object is indeed an LN.
            if (hitObject is DrawableEditorHitObjectLong longNote)
            {
                longNote.Body.Width = hitObject.Width;
                longNote.Tail.Width = hitObject.Width;
            }

            hitObject.AppearAsActive();

            // Check if the layer is hidden that the user is adding the object to and display the object
            // as that appropriate colour.
            if (((EditorScreenView)Ruleset.Screen.View).LayerCompositor.ScrollContainer.AvailableItems[hitObject.Info.EditorLayer].Hidden)
                hitObject.AppearAsHiddenInLayer();

            lock (HitObjects)
                HitObjects.Add(hitObject);
        }

        /// <summary>
        ///     Removes a HitObject sprite at a given index.
        /// </summary>
        public void RemoveHitObjectSprite(HitObjectInfo h)
        {
            lock (HitObjects)
            {
                var ho = HitObjects?.Find(x => x.Info == h);

                HitObjects?.Remove(ho);

                if (ho != null)
                {
                    ho.IsInView = false;
                    ho?.Destroy();
                }

                HitObjects = HitObjects.OrderBy(x => x.Info.StartTime).ToList();
            }
        }

        /// <summary>
        ///     Resizes a long note with that given hitobject info.
        /// </summary>
        /// <param name="h"></param>
        public DrawableEditorHitObjectLong ResizeLongNote(HitObjectInfo h)
        {
            var note = HitObjects.Find(x => x.Info == h);

            if (note is DrawableEditorHitObjectLong n)
                n.ResizeLongNote();

            return note as DrawableEditorHitObjectLong;
        }

        /// <summary>
        ///     Gets the audio time from a y position.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public double GetTimeFromY(float y) => TrackPositionY + (HitPositionY - y);

        /// <summary>
        ///     Gets the lane the mouse is in based on the mouse's x position.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int GetLaneFromX(float x)
        {
            var lane = (int) (( x / (ScreenRectangle.X - Width + LaneSize * Ruleset.WorkingMap.GetKeyCount()) - 1) * 10 + 1);

            if (lane <= 0 || lane > Ruleset.WorkingMap.GetKeyCount())
                lane = -1;

            return lane;
        }

        /// <summary>
        ///     Gets an object that is currently hovered
        /// </summary>
        /// <returns></returns>
        public DrawableEditorHitObject GetHoveredHitObject()
        {
            var relativeY = HitPositionY - (int) GetTimeFromY(MouseManager.CurrentState.Y);
            var relativeMousePos = new Vector2(MouseManager.CurrentState.X, relativeY);

            foreach (var h in HitObjects)
            {
                if (h.IsHovered(relativeMousePos))
                    return h;
            }

            return null;
        }

        /// <summary>
        ///     Called when the user changes the scroll speed of the map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e) => ResetObjectPositions();

        /// <summary>
        ///     Called when the user changes if they want to show lane divider lines.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowDividerLinesChanged(object sender, BindableValueChangedEventArgs<bool> e) => LaneDividerLines.ForEach(x => x.Visible = e.Value);

        /// <summary>
        ///     Called when the user wants to anchor the objects y position to their midpoint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHitObjectMidpointAnchoredChanged(object sender, BindableValueChangedEventArgs<bool> e) => ResetObjectPositions(false);

        /// <summary>
        ///     Called whenever an editor layer has been updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEditorLayerUpdated(object sender, EditorLayerUpdatedEventArgs e)
        {
            var hitObjects = HitObjects.FindAll(x => x.Info.EditorLayer == e.Index);

            if (!e.Type.HasFlag(EditorLayerUpdateType.Visibility) && !e.Type.HasFlag(EditorLayerUpdateType.Color))
                return;

            if (e.Layer.Hidden)
                hitObjects.ForEach(x =>
                {
                    if (Ruleset.SelectedHitObjects.Contains(x))
                        x.AppearAsSelected();
                    else if (Ruleset.PendingLongNoteReleases.Contains(x.Info))
                    {
                        var h = x as DrawableEditorHitObjectLong;
                        h?.AppearAsInactive();
                    }
                    else
                    {
                        x.AppearAsHiddenInLayer();
                    }
                });
            else
            {
                hitObjects.ForEach(x =>
                {
                    if (Ruleset.SelectedHitObjects.Contains(x))
                        x.AppearAsSelected();
                    else if (Ruleset.PendingLongNoteReleases.Contains(x.Info))
                    {
                        var h = x as DrawableEditorHitObjectLong;
                        h?.AppearAsInactive();
                    }
                    else
                        x.AppearAsActive();
                });
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEditorUseLayerHitObjects(object sender, BindableValueChangedEventArgs<bool> e)
        {
            lock (HitObjects)
            {
                var skin = SkinManager.Skin.Keys[Ruleset.Screen.WorkingMap.Mode];

                HitObjects.ForEach(h =>
                {
                    var index = skin.ColorObjectsBySnapDistance ?
                        HitObjectManager.GetBeatSnap(h.Info, h.Info.GetTimingPoint(Ruleset.Screen.WorkingMap.TimingPoints)) : 0;

                    if (h.Info.IsLongNote)
                    {
                        var ln = (DrawableEditorHitObjectLong) h;

                        if (ConfigManager.EditorViewLayers.Value)
                        {
                            h.Image = skin.EditorLayerNoteHitObjects[h.Info.Lane - 1];
                            ln.ChangeTextures(skin.EditorLayerNoteHoldBodies[h.Info.Lane - 1], skin.EditorLayerNoteHoldEnds[h.Info.Lane - 1]);
                        }
                        else
                        {
                            h.Image = skin.NoteHoldHitObjects[h.Info.Lane - 1][index];
                            ln.ChangeTextures(skin.NoteHoldBodies[h.Info.Lane - 1].First(), skin.NoteHoldEnds[h.Info.Lane - 1]);
                        }
                    }
                    else
                    {
                        if (ConfigManager.EditorViewLayers.Value)
                            h.Image = skin.EditorLayerNoteHitObjects[h.Info.Lane - 1];
                        else
                            h.Image = skin.NoteHitObjects[h.Info.Lane - 1][index];
                    }

                    // Resize and reposition the object to support skins that are of different sizes.
                    h.SetHeight();
                    h.SetPositionY();
                    h.Resize();

                    // Reset back to an active state temporarily because any objects that don't
                    // fit the following cases are considered active still.
                    h.AppearAsActive();

                    // Get the layer the object is in
                    var view = (EditorScreenView) Ruleset.Screen.View;
                    var layer = view.LayerCompositor.ScrollContainer.AvailableItems[h.Info.EditorLayer];

                    if (Ruleset.SelectedHitObjects.Contains(h))
                        h.AppearAsSelected();
                    else if (Ruleset.PendingLongNoteReleases.Contains(h.Info))
                    {
                        var ln = h as DrawableEditorHitObjectLong;
                        ln?.AppearAsInactive();
                    }
                    else if (layer.Hidden)
                        h.AppearAsHiddenInLayer();
                });
            }
        }
    }
}
