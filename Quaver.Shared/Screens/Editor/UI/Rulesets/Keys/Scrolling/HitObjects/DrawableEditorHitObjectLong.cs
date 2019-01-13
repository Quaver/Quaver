using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
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
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private float GetLongNoteHeight()
            => Math.Abs(Container.HitPositionY - Info.EndTime * Container.TrackSpeed -
                        (float) Container.LaneSize * TextureTail.Height / TextureTail.Width / 2f - Height / 2f - Y);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override bool CheckIfOnScreen() => base.CheckIfOnScreen() ||
                                                  AudioEngine.Track.Time >= Info.StartTime && AudioEngine.Track.Time <= Info.EndTime + 1000;
    }
}