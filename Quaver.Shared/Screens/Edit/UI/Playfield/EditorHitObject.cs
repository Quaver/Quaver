using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Skinning;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;

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
        protected HitObjectInfo Info { get; }

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
        protected SkinKeys SkinMode => Skin.Value.Keys[Map.Mode];

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="playfield"></param>
        /// <param name="info"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        public EditorHitObject(Qua map, EditorPlayfield playfield, HitObjectInfo info, Bindable<SkinStore> skin, IAudioTrack track,
            Bindable<bool> anchorHitObjectsAtMidpoint)
        {
            Map = map;
            Playfield = playfield;
            Info = info;
            Skin = skin;
            Track = track;
            AnchorHitObjectsAtMidpoint = anchorHitObjectsAtMidpoint;

            Image = GetHitObjectTexture();

            SetPosition();
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
            X = Playfield.ScreenRectangle.X + Playfield.ColumnSize * (Info.Lane - 1) + Playfield.BorderLeft.Width;
            Y = Playfield.HitPositionY - Info.StartTime * Playfield.TrackSpeed - Height;

            if (AnchorHitObjectsAtMidpoint.Value)
                Y += Height / 2f;
        }

        /// <summary>
        ///    Returns the texture for the hitobject
        /// </summary>
        private Texture2D GetHitObjectTexture()
        {
            var index = SkinMode.ColorObjectsBySnapDistance ? HitObjectManager.GetBeatSnap(Info, Info.GetTimingPoint(Map.TimingPoints)) : 0;
            return SkinMode.NoteHoldHitObjects[Info.Lane - 1][index];
        }

        /// <summary>
        ///     Checks if the object is visible and on the screen
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOnScreen() => Info.StartTime * Playfield.TrackSpeed >= Playfield.TrackPositionY - Playfield.Height &&
                                                 Info.StartTime * Playfield.TrackSpeed <= Playfield.TrackPositionY + Playfield.Height;
    }
}