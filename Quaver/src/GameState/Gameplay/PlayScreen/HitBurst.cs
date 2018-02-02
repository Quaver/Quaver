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
    class HitBurst
    {
        /// <summary>
        ///     Determines how long the sprite will be visible for in miliseconds
        /// </summary>
        private const double DISPLAY_TIME = 500;

        /// <summary>
        ///     Hit Burst Sprite's parent. Used for alignment
        /// </summary>
        private Boundary Boundary { get; set; }

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
            // Create boundary and hit burst sprite
            Boundary = new Boundary()
            {
                Size = new UDim2(rect.Width, rect.Height),
                Position = new UDim2(rect.X, rect.Y),
                Parent = parent
            };

            HitBurstSprite = new Sprite()
            {
                Alignment = Alignment.MidCenter,
                Size = new UDim2(rect.Width, rect.Height, 0, 0),
                Parent = parent
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
        ///     Destroys this class
        /// </summary>
        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        /// <summary>
        ///     Updates Hit Burst Sprite.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Update Time Elapsed + Hit Burst Sprite
            timeElapsed += dt;
            HitBurstSprite.Update(dt);

            // Destroy itself if time elapsed over DISPLAY_TIME duration.
            if (timeElapsed > DISPLAY_TIME) UnloadContent();
        }
    }
}
