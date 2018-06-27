using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Modes.Keys.Playfield
{
    internal class EditorScrollContainerKeys : Container
    {
        /// <summary>
        ///     The playfield
        /// </summary>
        internal EditorPlayfieldKeys Playfield { get; }

        /// <summary>
        ///     All of the HitObjects in the map.
        /// </summary>
        private List<EditorHitObjectKeys> HitObjects { get; set; }

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
        internal EditorScrollContainerKeys(EditorPlayfieldKeys playfield)
        {
            Playfield = playfield;

            Parent = Playfield.Container;
            Size = new UDim2D(Playfield.Width, Playfield.BackgroundContainer.SizeY);
            Alignment = Alignment.TopCenter;
            
            InitializeHitObjects();
        }

        /// <summary>
        ///     Initializes all of the HitObjects for the map.
        /// </summary>
        private void InitializeHitObjects()
        {
            HitObjects = new List<EditorHitObjectKeys>();          
            Playfield.Screen.Map.HitObjects.ForEach(x => HitObjects.Add(new EditorHitObjectKeys(this, x)));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update all of the HitObject's positions based on the song time.
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
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
                var timeDifference = GameBase.AudioEngine.Time;
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
                
                    // Set new HitObject positions.
                    hitObject.PositionY = hitObject.GetPosFromOffset(hitObject.OffsetYFromReceptor);
                    hitObject.UpdateSpritePositions();
                }
            }
         
            // Update all of the objects in the current pool.
            for (var i = objectsOffScreen; i < HitObjects.Count && i < poolSize + objectsOffScreen; i++)
            {
                var hitObject = HitObjects[i];
                
                // Set new HitObject positions.
                hitObject.PositionY = hitObject.GetPosFromOffset(hitObject.OffsetYFromReceptor);
                hitObject.UpdateSpritePositions();
                hitObject.PlayHitsoundsIfNecessary();
            }

            ObjectsOffScreenInLastFrame = objectsOffScreen;
            base.Update(dt);
        }
    }
}