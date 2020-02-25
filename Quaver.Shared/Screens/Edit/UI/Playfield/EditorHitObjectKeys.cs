using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI.Playfield
{
    public class EditorHitObjectKeys : EditorHitObject
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
        public Texture2D TextureBody { get; private set; }

        /// <summary>
        ///     The texture for the long note's end.
        /// </summary>
        public Texture2D TextureTail { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="playfield"></param>
        /// <param name="info"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        /// <param name="viewLayers"></param>
        public EditorHitObjectKeys(Qua map, EditorPlayfield playfield, HitObjectInfo info, Bindable<SkinStore> skin, IAudioTrack track,
            Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<bool> viewLayers)
            : base(map, playfield, info, skin, track, anchorHitObjectsAtMidpoint, viewLayers)
        {
            TextureBody = GetBodyTexture();
            TextureTail = GetTailTexture();

            CreateLongNoteSprite();
            ResizeLongNote();

            ViewLayers.ValueChanged += OnViewLayersChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime) => DrawToSpriteBatch();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            Body?.Destroy();
            Tail?.Destroy();

            // ReSharper disable once DelegateSubtraction
            ViewLayers.ValueChanged -= OnViewLayersChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            // Draw the body first, then the note. That'll make it so we can get that effect
            // where if the player is using an arrow skin, part of the body will be under the note.
            if (Info.IsLongNote)
            {
                Body.DrawToSpriteBatch();
                Tail.DrawToSpriteBatch();
            }

            base.DrawToSpriteBatch();
        }

        /// <summary>
        ///     Handles the creation of the long note sprite.
        /// </summary>
        private void CreateLongNoteSprite()
        {
            // Make sure the
            base.SetSize();

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
                Size = new ScalableVector2(Width, GetTailHeight()),
                Y = -Body.Height,
            };
        }

        /// <summary>
        ///     Resizes the long note to the correct height.
        ///     Usually used for when the zoom/playback rate has changed.
        /// </summary>
        public void ResizeLongNote()
        {
            if (!Info.IsLongNote)
                return;

            Body.Height = GetLongNoteHeight();
            Body.Y = -Body.Height + Height / 2f;
            Tail.Y = -Body.Height;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private float GetLongNoteHeight()
        {
            if (!Info.IsLongNote)
                return 0;

            var height = Math.Abs(Playfield.HitPositionY - Info.EndTime * Playfield.TrackSpeed -
                                  (float) Playfield.ColumnSize * TextureTail.Height / TextureTail.Width / 2 - Height / 2f - Y);

            if (AnchorHitObjectsAtMidpoint.Value)
                height -= Height / 2f;

            return height;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void SetSize()
        {
            base.SetSize();

            if (!Info.IsLongNote)
                return;

            Tail.Height = GetTailHeight();
            ResizeLongNote();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private float GetTailHeight() => (float) Playfield.ColumnSize * TextureTail.Height / TextureTail.Width;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Texture2D GetBodyTexture()
        {
            return ViewLayers.Value ? SkinMode.EditorLayerNoteHoldBodies[Info.Lane - 1] : SkinMode.NoteHoldBodies[Info.Lane - 1].First();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Texture2D GetTailTexture()
        {
            return ViewLayers.Value ? SkinMode.EditorLayerNoteHoldEnds[Info.Lane - 1] : SkinMode.NoteHoldEnds[Info.Lane - 1];
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override bool IsOnScreen() => base.IsOnScreen() || Track.Time >= Info.StartTime
                                                  && Track.Time <= Info.EndTime + 1000;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public override bool IsHovered(Vector2 mousePos)
        {
            var headHovered = ScreenRectangle.Contains(mousePos);

            if (!Info.IsLongNote)
                return headHovered;

            return headHovered || Body.ScreenRectangle.Contains(mousePos) || Tail.ScreenRectangle.Contains(mousePos);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewLayersChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            TextureBody = GetBodyTexture();
            TextureTail = GetTailTexture();

            Body.Image = TextureBody;
            Tail.Image = TextureTail;

            Body.Tint = GetNoteTint();
            Tail.Tint = GetNoteTint();

            ResizeLongNote();
        }
    }
}