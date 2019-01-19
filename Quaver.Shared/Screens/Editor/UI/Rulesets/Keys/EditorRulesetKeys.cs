/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Editor.Actions;
using Quaver.Shared.Screens.Editor.Actions.Rulesets;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Input;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys
{
    public class EditorRulesetKeys : EditorRuleset
    {
        /// <summary>
        ///     Used for scrolling hitobjects & timing lines.
        /// </summary>
        public EditorScrollContainerKeys ScrollContainer { get; private set; }

        /// <summary>
        ///     The selected tool the user has when compositing the map
        /// </summary>
        public Bindable<EditorCompositionTool> CompositionTool { get; }

        /// <summary>
        ///     Keeps track if we're currently pending a long note release specification for that
        ///     given lane.
        /// </summary>
        public List<HitObjectInfo> PendingLongNoteReleases { get; } = new List<HitObjectInfo>(new HitObjectInfo[7]);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorRulesetKeys(EditorScreen screen) : base(screen)
        {
            CompositionTool = new Bindable<EditorCompositionTool>(EditorCompositionTool.LongNote)
            {
                Value = EditorCompositionTool.LongNote
            };

            CreateScrollContainer();
            ActionManager = CreateActionManager();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.PageUp))
                ConfigManager.EditorScrollSpeedKeys.Value++;

            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.PageDown))
                ConfigManager.EditorScrollSpeedKeys.Value--;

            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D1))
                PlaceObject(1);

            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D2))
                PlaceObject(2);

            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D3))
                PlaceObject(3);

            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D4))
                PlaceObject(4);

            if (Screen.WorkingMap.Mode == GameMode.Keys7)
            {
                if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D5))
                    PlaceObject(5);

                if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D6))
                    PlaceObject(6);

                if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.D7))
                    PlaceObject(7);
            }

            // Change between composition tools (only when shift isn't held down).
            // if shift is held down, then it'll change the beat snap.
            if (KeyboardManager.CurrentState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.LeftShift)
                || KeyboardManager.CurrentState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.RightShift))
            {
                if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.Up))
                    CompositionTool.Value = EditorCompositionTool.Note;

                if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.Down))
                    CompositionTool.Value = EditorCompositionTool.LongNote;
            }

            HandleHitObjectMouseInput();
        }

        /// <summary>
        ///     Toggles the scroll direction for the editor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ToggleScrollDirection()
        {
            switch (Screen.Ruleset.WorkingMap.Mode)
            {
                case GameMode.Keys4:
                    ConfigManager.ScrollDirection4K.Value = ConfigManager.ScrollDirection4K.Value != ScrollDirection.Down
                        ? ScrollDirection.Down : ScrollDirection.Up;
                    break;
                case GameMode.Keys7:
                    ConfigManager.ScrollDirection7K.Value = ConfigManager.ScrollDirection7K.Value != ScrollDirection.Down
                        ? ScrollDirection.Down : ScrollDirection.Up;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Change hit pos line Y
            switch (GameplayRulesetKeys.ScrollDirection)
            {
                case ScrollDirection.Split:
                case ScrollDirection.Down:
                    ScrollContainer.HitPositionLine.Y = ScrollContainer.HitPositionY;
                    break;
                case ScrollDirection.Up:
                    ScrollContainer.HitPositionLine.Y = (int) WindowManager.Height - ScrollContainer.HitPositionY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Handles performing actions w/ the mouse
        /// </summary>
        private void HandleHitObjectMouseInput()
        {
            // Left click/place object
            if (MouseManager.IsUniqueClick(MouseButton.Left))
            {
            }

            // Right click/delete object.
            if (MouseManager.IsUniqueClick(MouseButton.Right))
            {
            }
        }

        /// <summary>
        ///     Places a HitObject at a given lane.
        /// </summary>
        /// <param name="lane"></param>
        private void PlaceObject(int lane)
        {
            var am = ActionManager as EditorActionManagerKeys;

            if (HandlePendingLongNoteReleases(lane))
                return;

            // Find an existing object in the current lane at the same time, so we can determine if
            // the object should be placed or deleted accordingly.
            var existingObject = WorkingMap.HitObjects.Find(x => x.StartTime == (int) AudioEngine.Track.Time && x.Lane == lane);

            // There's no object currently at this position, so add it.
            if (existingObject == null)
            {
                switch (CompositionTool.Value)
                {
                    case EditorCompositionTool.Note:
                        am?.PlaceHitObject(lane);
                        break;
                    case EditorCompositionTool.LongNote:
                        am?.PlaceLongNote(lane);

                        // Makes sure the long note is marked as pending, so any future objects placed in this lane
                        // will be awarded to this LN's end.
                        var workingObject = WorkingMap.HitObjects.Find(x => x.StartTime == (int) AudioEngine.Track.Time && x.Lane == lane);
                        PendingLongNoteReleases[lane - 1] = workingObject;

                        // Make the long note appear as inactive/dead. Gives a visual effect to the user that
                        // they need to do something with the note.
                        var drawable = (DrawableEditorHitObjectLong) ScrollContainer.HitObjects.Find(x => x.Info == workingObject);
                        drawable.AppearAsInactive();

                        NotificationManager.Show(NotificationLevel.Info, "Scroll through the timeline and place the end of the long note.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            // An object exists, so delete it.
            else
                am?.DeleteHitObject(existingObject);
        }

        /// <summary>
        ///     Places an object at a specific time.
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="time"></param>
        private void PlaceObject(int lane, double time)
        {
            var am = ActionManager as EditorActionManagerKeys;

            var existingObject = WorkingMap.HitObjects.Find(x => x.StartTime == (int) time && x.Lane == lane);

            // There's no object currently at this position, so add it.
            if (existingObject == null)
                am?.PlaceHitObject(lane, time);
        }

        /// <summary>
        ///     Handles any long note releases that are currently pending.
        ///     If returned false, nothing has been handled/needs to be handled.
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        private bool HandlePendingLongNoteReleases(int lane)
        {
            // User has a pending long note release for this lane, so that needs to be taken care of.
            if (PendingLongNoteReleases[lane - 1] == null)
                return false;

            var pendingObject = PendingLongNoteReleases[lane - 1];

            if ((int) AudioEngine.Track.Time < pendingObject.StartTime)
            {
                NotificationManager.Show(NotificationLevel.Error, "You need to select a position later than the start time");
                return true;
            }

            // Returning false here because the user should be able to delete the note if they're
            // still on the starting area.
            if ((int) AudioEngine.Track.Time == pendingObject.StartTime)
            {
                PendingLongNoteReleases[lane - 1] = null;
                return false;
            }

            // Long note is no longer pending given that the user has entered a correct position.
            PendingLongNoteReleases[lane - 1] = null;

            // Resize the long note and then reset the color of it.
            pendingObject.EndTime = (int) AudioEngine.Track.Time;
            ScrollContainer.ResizeLongNote(pendingObject).AppearAsActive();
            return true;

        }

        /// <summary>
        /// </summary>
        private void CreateScrollContainer() => ScrollContainer = new EditorScrollContainerKeys(this) { Parent = Container };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override EditorActionManager CreateActionManager() => new EditorActionManagerKeys(this);
    }
}