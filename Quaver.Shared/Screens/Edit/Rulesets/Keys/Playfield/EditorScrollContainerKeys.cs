using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Rulesets.Keys.Playfield
{
    public class EditorScrollContainerKeys : Container
    {
        /// <summary>
        ///     The playfield
        /// </summary>
        public EditorPlayfieldKeys Playfield { get; }

        /// <summary>
        ///     All of the HitObjects in the map.
        /// </summary>
        public List<EditorHitObjectKeys> HitObjects { get; private set; }

        /// <summary>
        ///     The amount of objects that were off-screen in the previous frame
        ///     So we can do some cleanup of the positions on those objects.
        /// </summary>
        private int ObjectsOffScreenInLastFrame { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="playfield"></param>
        public EditorScrollContainerKeys(EditorPlayfieldKeys playfield)
        {
            Playfield = playfield;

            Parent = Playfield.Container;
            Size = new ScalableVector2(Playfield.Width, Playfield.BackgroundContainer.Height);
            Alignment = Alignment.TopCenter;

            InitializeHitObjects();
        }

        /// <summary>
        ///     Initializes all of the HitObjects for the map.
        /// </summary>
        private void InitializeHitObjects()
        {
            HitObjects = new List<EditorHitObjectKeys>();

            for (var i = 0; i < Playfield.Screen.Map.HitObjects.Count; i++)
            {
                var hitObject = new EditorHitObjectKeys(this, Playfield.Screen.Map.HitObjects[i]);

                if (i > byte.MaxValue)
                    hitObject.MakeInvisible();

                HitObjects.Add(hitObject);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update all of the HitObject's positions based on the song time.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // The amount of objects that'll be updated each frame.
            const int poolSize = byte.MaxValue;

            // The amount of time it takes for an object to be seen as "off-screen"
            // TODO: This needs to be adjusted based on scroll speed, and it may not be accurate.
            // TODO: This is merely just an arbitrary value based on "15" scroll speed at the time of
            // TODO: Writing.
            const int clearTime = 1000;

            // Get the amount of objects that have already passed off-screen based on the audio's
            // current time. (1.5 second time, increase as necessary).
            var objectsOffScreen = HitObjects.Count(x =>
            {
                var timeDifference = AudioEngine.Track.Time;
                timeDifference = x.Info.IsLongNote ? timeDifference - x.Info.EndTime : timeDifference - x.Info.StartTime;

                return timeDifference >= clearTime;
            });

            // If a different amount of objects were off-screen in the previous lane, then we need to
            // update those ones accordingly.
            if (objectsOffScreen != ObjectsOffScreenInLastFrame)
            {
                for (var i = ObjectsOffScreenInLastFrame; i < HitObjects.Count && i < poolSize + ObjectsOffScreenInLastFrame; i++)
                {
                    var hitObject = HitObjects[i];
                    hitObject.MakeInvisible();

                    // Set new HitObject positions.
                    hitObject.PositionY = GetPosFromOffset(Playfield.HitPositionY, Playfield.ScrollSpeed, hitObject.OffsetYFromReceptor, hitObject.HitObjectSprite.Height);
                    hitObject.UpdateSpritePositions();
                    hitObject.MakeVisible();
                }
            }

            // Update all of the objects in the current pool.
            for (var i = objectsOffScreen; i < HitObjects.Count && i < poolSize + objectsOffScreen; i++)
            {
                var hitObject = HitObjects[i];

                // Set new HitObject positions.
                hitObject.PositionY = GetPosFromOffset(Playfield.HitPositionY, Playfield.ScrollSpeed, hitObject.OffsetYFromReceptor, hitObject.HitObjectSprite.Height);
                hitObject.UpdateSpritePositions();
                hitObject.PlayHitsoundsIfNecessary();
            }

            ObjectsOffScreenInLastFrame = objectsOffScreen;
            base.Update(gameTime);
        }

        /// <summary>
        ///     Calculates the position from the offset.
        /// </summary>
        /// <returns></returns>
        public static float GetPosFromOffset(float hitPosY, float speed, float offset, float sizeY)
        {
            return (float)(hitPosY - (offset - AudioEngine.Track.Time) * speed) - sizeY;
        }
    }
}
