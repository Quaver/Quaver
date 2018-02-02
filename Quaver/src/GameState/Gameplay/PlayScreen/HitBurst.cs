using Quaver.API.Enums;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    class HitBurst : Boundary
    {
        /// <summary>
        ///     Determines how long the sprite will be visible for in miliseconds
        /// </summary>
        private const double DISPLAY_TIME = 500;

        private const float MAX_SCALE = 0.1f;

        /// <summary>
        ///     The Hit Burst Sprite. Will be animated.
        /// </summary>
        private Sprite HitBurstSprite { get; set; }

        /// <summary>
        ///     Total time elapsed since this object has been created.
        /// </summary>
        private double timeElapsed { get; set; }

        /// <summary>
        ///     Create a new hit burst. Used after a note has been hit.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="parent"></param>
        /// <param name="keyLane"></param>
        public HitBurst(DrawRectangle rect, Drawable parent, int keyLane)
        {
            // Update Size + Position
            Position = new UDim2(rect.X, rect.Y);
            Size = new UDim2(rect.Width, rect.Height);
            Parent = parent;

            HitBurstSprite = new Sprite()
            {
                Alignment = Alignment.MidCenter,
                Size = new UDim2(0, 0, 1, 1),
                Parent = this
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
        ///     Updates Hit Burst Sprite.
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Update Time Elapsed + Hit Burst Sprite
            timeElapsed += dt;
            var timeRatio = (float)(timeElapsed / DISPLAY_TIME);

            // Destroy itself if time elapsed over DISPLAY_TIME duration.
            if (timeElapsed > DISPLAY_TIME)
            {
                HitBurstSprite.Destroy();
                return;
            }

            // Update Objects
            HitBurstSprite.ScaleX = 1 + (timeRatio * MAX_SCALE);
            HitBurstSprite.ScaleY = HitBurstSprite.ScaleX;
            HitBurstSprite.Alpha = 1 - timeRatio;

            // Update Base
            base.Update(dt);
        }
    }
}
