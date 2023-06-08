using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Skinning;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.UI.Playfield
{
    public class EditorHitObject : Sprite
    {
        /// <summary>
        /// </summary>
        protected Qua Map { get; }

        /// <summary>
        /// </summary>
        protected EditorPlayfield Playfield { get; }

        /// <summary>
        /// </summary>
        public HitObjectInfo Info { get; }

        /// <summary>
        /// </summary>
        protected Bindable<SkinStore> Skin { get; }

        /// <summary>
        /// </summary>
        protected IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        protected Bindable<bool> AnchorHitObjectsAtMidpoint { get; }

        /// <summary>
        /// </summary>
        protected Bindable<bool> ViewLayers { get; }

        /// <summary>
        /// </summary>
        protected BindableInt LongNoteOpacity { get; }

        /// <summary>
        /// </summary>
        protected BindableList<HitObjectInfo> SelectedHitObjects { get; }

        /// <summary>
        /// </summary>
        protected SkinKeys SkinMode => Skin.Value.Keys[Map.Mode];

        protected EditorLayerInfo DefaultLayer { get; }

        private Color HiddenLayerColor { get; } = new Color(40, 40, 40);

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="playfield"></param>
        /// <param name="info"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        /// <param name="viewLayers"></param>
        /// <param name="longNoteOpacity"></param>
        /// <param name="selectedHitObjects"></param>
        /// <param name="defaultLayer"></param>
        public EditorHitObject(Qua map, EditorPlayfield playfield, HitObjectInfo info, Bindable<SkinStore> skin, IAudioTrack track,
            Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<bool> viewLayers, BindableInt longNoteOpacity,
            BindableList<HitObjectInfo> selectedHitObjects, EditorLayerInfo defaultLayer)
        {
            Map = map;
            Playfield = playfield;
            Info = info;
            Skin = skin;
            Track = track;
            AnchorHitObjectsAtMidpoint = anchorHitObjectsAtMidpoint;
            ViewLayers = viewLayers;
            LongNoteOpacity = longNoteOpacity;
            SelectedHitObjects = selectedHitObjects;
            DefaultLayer = defaultLayer;

            Image = GetHitObjectTexture();
            SetPosition();
            Tint = GetNoteTint();
        }

        /// <inheritdoc />
        /// <summary>
        ///     When drawing the object, we only want to just draw it to SpriteBatch.
        ///     We don't want to handle anything else since its all manual in this purpose
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime) => DrawToSpriteBatch();


        /// <summary>
        ///     Sets the size of the object
        /// </summary>
        public virtual void SetSize()
        {
            Width = Playfield.ColumnSize - Playfield.BorderLeft.Width * 2;
            Height = (Playfield.ColumnSize - Playfield.BorderLeft.Width * 2) * Image.Height / Image.Width;
        }

        /// <summary>
        ///     Sets the position of the object
        /// </summary>
        public void SetPosition()
        {
            var lane = Info.Lane - 1;

            // Place the scratch key on the left instead of right if the user has it enabled in gameplay.
            if (Map.HasScratchKey && ConfigManager.ScratchLaneLeft7K != null && ConfigManager.ScratchLaneLeft7K.Value)
            {
                if (Info.Lane == 8)
                    lane = 0;
                else
                    lane++;
            }

            var x = Playfield.ScreenRectangle.X + Playfield.ColumnSize * lane + Playfield.BorderLeft.Width;
            var y = Playfield.HitPositionY - Info.StartTime * Playfield.TrackSpeed - Height;

            if (AnchorHitObjectsAtMidpoint.Value)
                y += Height / 2f;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != x || Y != y)
                Position = new ScalableVector2(x, y);
        }

        /// <summary>
        ///    Returns the texture for the hitobject
        /// </summary>
        public Texture2D GetHitObjectTexture()
        {
            var index = SkinMode.ColorObjectsBySnapDistance && Map.TimingPoints.Count > 0 ? HitObjectManager.GetBeatSnap(Info, Info.GetTimingPoint(Map.TimingPoints)) : 0;

            if (ViewLayers.Value)
                return SkinMode.EditorLayerNoteHitObjects[Info.Lane - 1];

            if (Info.IsLongNote)
                return SkinMode.NoteHoldHitObjects[Info.Lane - 1][index];

            return SkinMode.NoteHitObjects[Info.Lane - 1][index];
        }

        /// <summary>
        ///     Checks if the object is visible and on the screen
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOnScreen() => Info.StartTime * Playfield.TrackSpeed >= Playfield.TrackPositionY - Playfield.Height &&
                                                 Info.StartTime * Playfield.TrackSpeed <= Playfield.TrackPositionY + Playfield.Height;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected Color GetNoteTint()
        {
            if (Info.EditorLayer == 0)
                return DefaultLayer.Hidden & !Playfield.IsUneditable ? HiddenLayerColor : Color.White;

            try
            {
                var layer = Map.EditorLayers[Info.EditorLayer - 1];

                if (layer.Hidden && !Playfield.IsUneditable)
                    return HiddenLayerColor;

                if (!ViewLayers.Value)
                    return Color.White;

                return ColorHelper.ToXnaColor(layer.GetColor());
            }
            catch (Exception)
            {
                return Color.White;
            }
        }

        /// <summary>
        ///     Determines if the HitObject is hovered.
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public virtual bool IsHovered(Vector2 mousePos) => ScreenRectangle.Contains(mousePos);
    }
}