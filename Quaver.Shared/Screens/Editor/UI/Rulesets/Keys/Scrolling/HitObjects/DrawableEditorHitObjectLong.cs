/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects
{
    public class DrawableEditorHitObjectLong : DrawableEditorHitObject
    {
        /// <summary>
        ///     The long note's body sprite
        /// </summary>
        public Sprite Body { get; private set; }

        /// <summary>
        ///     The long note's tail.
        /// </summary>
        public Sprite Tail { get; private set; }

        /// <summary>
        ///     The texture for the long note's body
        /// </summary>
        private Texture2D TextureBody { get; }

        /// <summary>
        ///     The texture for the long note's end.
        /// </summary>
        private Texture2D TextureTail { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="info"></param>
        /// <param name="texHead"></param>
        /// <param name="texBody"></param>
        /// <param name="texTail"></param>
        public DrawableEditorHitObjectLong(EditorScrollContainerKeys container, HitObjectInfo info, Texture2D texHead,
            Texture2D texBody, Texture2D texTail) : base(container, info, texHead)
        {
            TextureBody = texBody;
            TextureTail = texTail;
            CreateLongNoteSprite();
            SelectionSprite.Parent = Body;
            SelectionSprite.Alignment = Alignment.MidLeft;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            // Draw the body first, then the note. That'll make it so we can get that effect
            // where if the player is using an arrow skin, part of the body will be under the note.
            Body.DrawToSpriteBatch();
            Tail.DrawToSpriteBatch();
            base.DrawToSpriteBatch();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Override and don't call base method update method
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            Body.Destroy();
            Tail.Destroy();
        }

        /// <summary>
        ///     Handles the creation of the long note sprite.
        /// </summary>
        private void CreateLongNoteSprite()
        {
            Body = new Sprite
            {
                Parent = this,
                Image = TextureBody,
                Size = new ScalableVector2(Width, GetLongNoteHeight()),
            };

            Body.Y = -Body.Height + Height / 2f;

            Tail = new Sprite
            {
                Parent = this,
                Image = TextureTail,
                Size = new ScalableVector2(Width, (float) Container.LaneSize * TextureTail.Height / TextureTail.Width),
                Y = -Body.Height,
            };
        }

        /// <summary>
        ///     Resizes the long note to the correct height.
        ///     Usually used for when the zoom/playback rate has changed.
        /// </summary>
        public void ResizeLongNote()
        {
            Body.Height = GetLongNoteHeight();
            Body.Y = -Body.Height + Height / 2f;
            Tail.Y = -Body.Height;

            SelectionSprite.Size = new ScalableVector2(Width, GetLongNoteHeight() + Height / 2f + Tail.Height / 2f + 20);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private float GetLongNoteHeight()
        {
            var height = Math.Abs(Container.HitPositionY - Info.EndTime * Container.TrackSpeed -
                            (float) Container.LaneSize * TextureTail.Height / TextureTail.Width / 2 - Height / 2f - Y);

            if (ConfigManager.EditorHitObjectsMidpointAnchored.Value)
                height -= Height / 2f;

            return height;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override bool CheckIfOnScreen() => base.CheckIfOnScreen() ||
                                                  AudioEngine.Track.Time >= Info.StartTime && AudioEngine.Track.Time <= Info.EndTime + 1000;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void AppearAsActive()
        {
            base.AppearAsActive();
            Body.Tint = Tint;
            Tail.Tint = Tint;
        }

        /// <summary>
        ///     Makes the long note appear as if it is dead/inactive.
        /// </summary>
        public void AppearAsInactive()
        {
            var col = new Color(160, 160, 160);
            Tint = col;
            Body.Tint = col;
            Tail.Tint = col;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void AppearAsSelected()
        {
            SelectionSprite.Size = new ScalableVector2(Width, GetLongNoteHeight() + Height / 2f + Tail.Height / 2f + 20);
            SelectionSprite.Visible = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void AppearAsHiddenInLayer()
        {
            base.AppearAsHiddenInLayer();
            Body.Tint = Tint;
            Tail.Tint = Tint;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public override bool IsHovered(Vector2 mousePos) => base.IsHovered(mousePos) || Body.ScreenRectangle.Contains(mousePos) ||
                                                            Tail.ScreenRectangle.Contains(mousePos);
    }
}
