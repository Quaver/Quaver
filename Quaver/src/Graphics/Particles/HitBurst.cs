using Quaver.API.Enums;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.Particles
{
    class HitBurst : Particle
    {
        /// <summary>
        ///     Determines how long the sprite will be visible for in miliseconds
        /// </summary>
        public override double DisplayTime { get; set; } = 500;

        /// <summary>
        ///     Determines if the particle is ready to be destroyed
        /// </summary>
        public override bool DestroyReady { get; set; }

        /// <summary>
        ///     Total time elapsed since this object has been created.
        /// </summary>
        public override double TimeElapsed { get; set; }

        /// <summary>
        ///     Max Scale of sprite when it is resized.
        /// </summary>
        private float MaxScale { get; } = 0.1f;

        /// <summary>
        ///     The Hit Burst Sprite. Will be animated.
        /// </summary>
        private Sprite.Sprite HitBurstSprite { get; set; }

        /// <summary>
        ///     Hit Burst Sprite's Parent. Used for object alignment.
        /// </summary>
        private Boundary Boundary { get; set; }

        /// <summary>
        ///     Create a new hit burst. Used after a note has been hit.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="parent"></param>
        /// <param name="keyLane"></param>
        public HitBurst(DrawRectangle rect, Drawable parent, int keyLane)
        {
            // Create Boundary and Particle
            Boundary = new Boundary(rect.X, rect.Y, rect.Width, rect.Height)
            {
                Parent = parent
            };

            HitBurstSprite = new Sprite.Sprite()
            {
                Alignment = Alignment.MidCenter,
                Size = new UDim2(0, 0, 1, 1),
                Parent = Boundary
            };

            // Choose the correct image based on the specific key lane.
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    HitBurstSprite.Image = GameBase.LoadedSkin.NoteHitBursts4K[keyLane];
                    break;
                case GameModes.Keys7:
                    HitBurstSprite.Image = GameBase.LoadedSkin.NoteHitBursts7K[keyLane];
                    break;
            }
        }

        /// <summary>
        ///     Destroys this object.
        /// </summary>
        public override void Destroy()
        {
            Boundary.Destroy();
        }

        /// <summary>
        ///     Updates Hit Burst Sprite.
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(double dt)
        {
            // Update Time Elapsed + Hit Burst Sprite
            TimeElapsed += dt;
            var timeRatio = (float)(TimeElapsed / DisplayTime);

            // Destroy itself if time elapsed over DISPLAY_TIME duration.
            if (TimeElapsed > DisplayTime)
            {
                DestroyReady = true;
                return;
            }

            // Update Objects
            HitBurstSprite.ScaleX = 1 + (timeRatio * MaxScale);
            HitBurstSprite.ScaleY = HitBurstSprite.ScaleX;
            HitBurstSprite.Alpha = 1 - timeRatio;

            // Update Boundary
            Boundary.Update(dt);
        }
    }
}
