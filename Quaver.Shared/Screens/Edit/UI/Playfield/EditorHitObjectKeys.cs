using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
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

        /// <summary>
        ///     Displays when the object is selected
        /// </summary>
        private Sprite SelectionSprite { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="playfield"></param>
        /// <param name="info"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        /// <param name="coloring"></param>
        /// <param name="longNoteOpacity"></param>
        /// <param name="selectedHitObjects"></param>
        /// <param name="defaultLayer"></param>
        public EditorHitObjectKeys(Qua map, EditorPlayfield playfield, HitObjectInfo info, Bindable<SkinStore> skin, IAudioTrack track,
            Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<HitObjectColoring> coloring, BindableInt longNoteOpacity,
            BindableList<HitObjectInfo> selectedHitObjects, EditorLayerInfo defaultLayer) : base(map, playfield, info,
            skin, track, anchorHitObjectsAtMidpoint, coloring, longNoteOpacity, selectedHitObjects, defaultLayer)
        {
            TextureBody = GetBodyTexture();
            TextureTail = GetTailTexture();

            CreateLongNoteSprite();
            UpdateLongNoteSizeAndAlpha();
            CreateSelectionSprite();

            Refresh();

            Coloring.ValueChanged += OnViewLayersChanged;
            SelectedHitObjects.ItemAdded += OnSelectedHitObject;
            SelectedHitObjects.ItemRemoved += OnDeselectedHitObject;
            SelectedHitObjects.ListCleared += OnAllObjectsDeselected;
            SelectedHitObjects.MultipleItemsAdded += OnMultipleItemsAdded;
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

            // ReSharper disable twice DelegateSubtraction
            Coloring.ValueChanged -= OnViewLayersChanged;

            SelectedHitObjects.ItemAdded -= OnSelectedHitObject;
            SelectedHitObjects.ItemRemoved -= OnDeselectedHitObject;
            SelectedHitObjects.ListCleared -= OnAllObjectsDeselected;
            SelectedHitObjects.MultipleItemsAdded -= OnMultipleItemsAdded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            Tint = GetNoteTint();
            Body.Tint = Tint;
            Tail.Tint = Tint;

            // Draw the body first, then the note. That'll make it so we can get that effect
            // where if the player is using an arrow skin, part of the body will be under the note.
            if (Info.IsLongNote)
            {
                Body.DrawToSpriteBatch();
                Tail.DrawToSpriteBatch();
            }

            base.DrawToSpriteBatch();

            if (SelectionSprite.Visible)
            {
                if (Info.IsLongNote)
                    SelectionSprite.Height = GetLongNoteHeight() + Height / 2f + Tail.Height / 2f + 20;
                else
                    SelectionSprite.Height = Height / 2f + Tail.Height / 2f + 20;

                SelectionSprite.Draw(new GameTime());
            }
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
        ///     Creates <see cref="SelectionSprite"/>
        /// </summary>
        private void CreateSelectionSprite()
        {
            SelectionSprite = new Sprite()
            {
                Parent = Body,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(Width, 0),
                Image = UserInterface.BlankBox,
                Tint = Color.White,
                Alpha = 0.50f,
                Visible = false
            };
        }

        /// <summary>
        ///     Resizes the long note to the correct height and opacity
        /// </summary>
        public void UpdateLongNoteSizeAndAlpha()
        {
            if (!Info.IsLongNote)
                return;

            Body.Height = GetLongNoteHeight();
            Body.Y = -Body.Height + Height / 2f;
            Tail.Y = -Body.Height;

            Tail.Alpha = LongNoteOpacity.Value / 100f;
            Body.Alpha = LongNoteOpacity.Value / 100f;
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
            UpdateLongNoteSizeAndAlpha();
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
            return Coloring.Value != HitObjectColoring.None ? SkinMode.EditorLayerNoteHoldBodies[Info.Lane - 1] : SkinMode.NoteHoldBodies[Info.Lane - 1].First();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Texture2D GetTailTexture()
        {
            return Coloring.Value != HitObjectColoring.None ? SkinMode.EditorLayerNoteHoldEnds[Info.Lane - 1] : SkinMode.NoteHoldEnds[Info.Lane - 1];
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
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public bool IsTailHovered(Vector2 mousePos)
        {
            if (!Info.IsLongNote)
                return false;

            return Tail.ScreenRectangle.Contains(mousePos);
        }

        /// <summary>
        ///     Updates all textures for the object
        /// </summary>
        public void UpdateTextures()
        {
            Image = GetHitObjectTexture();
            TextureBody = GetBodyTexture();
            TextureTail = GetTailTexture();

            Body.Image = TextureBody;
            Tail.Image = TextureTail;

            if (SkinMode.RotateHitObjectsByColumn && Coloring.Value == HitObjectColoring.None)
                Rotation = GameplayHitObjectKeys.GetObjectRotation(Map.Mode, Info.Lane - 1);
            else
                Rotation = 0;
        }

        /// <summary>
        ///     Refreshes the object to make it appear as it should be (positions/sizes/textures)
        /// </summary>
        public void Refresh()
        {
            UpdateTextures();
            SetPosition();
            SetSize();
            UpdateLongNoteSizeAndAlpha();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewLayersChanged(object sender, BindableValueChangedEventArgs<HitObjectColoring> e)
        {
            UpdateTextures();
            SetSize();

            Body.Tint = GetNoteTint();
            Tail.Tint = GetNoteTint();

            UpdateLongNoteSizeAndAlpha();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedHitObject(object sender, BindableListItemAddedEventArgs<HitObjectInfo> e)
        {
            if (e.Item != Info)
                return;

            SelectionSprite.Visible = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeselectedHitObject(object sender, BindableListItemRemovedEventArgs<HitObjectInfo> e)
        {
            if (e.Item != Info)
                return;

            SelectionSprite.Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllObjectsDeselected(object sender, BindableListClearedEventArgs e)
        {
            SelectionSprite.Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultipleItemsAdded(object sender, BindableListMultipleItemsAddedEventArgs<HitObjectInfo> e)
        {
            if (!e.Items.Contains(Info))
                return;

            SelectionSprite.Visible = true;
        }
    }
}