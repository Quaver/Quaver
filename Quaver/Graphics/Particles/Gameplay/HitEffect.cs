using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics.Base;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.Particles.Gameplay
{
    internal class HitEffect : Particle
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
        private float MaxScale { get; } = 0.2f;

        /// <summary>
        ///     The Hit Burst QuaverSprite. Will be animated.
        /// </summary>
        private Sprites.QuaverSprite HitBurstQuaverSprite { get; set; }

        /// <summary>
        ///     Hit Burst QuaverSprite's Parent. Used for object alignment.
        /// </summary>
        private QuaverContainer QuaverContainer { get; set; }

        /// <summary>
        ///     Create a new hit burst. Used after a note has been hit.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="parent"></param>
        /// <param name="keyLane"></param>
        public HitEffect(DrawRectangle rect, Drawable parent, int keyLane)
        {
            // Create QuaverContainer and Particle
            QuaverContainer = new QuaverContainer(rect.X, rect.Y, rect.Width, rect.Height)
            {
                Parent = parent
            };

            HitBurstQuaverSprite = new Sprites.QuaverSprite()
            {
                Alignment = Alignment.MidCenter,
                Size = new UDim2D(0, 0, 1, 1),
                Parent = QuaverContainer
            };

            // Choose the correct image based on the specific key lane.
            switch (GameBase.SelectedMap.Qua.Mode)
            {
                case GameModes.Keys4:
                    HitBurstQuaverSprite.Image = GameBase.LoadedSkin.NoteHitEffects4K[keyLane];
                    HitBurstQuaverSprite.SpriteEffect = !Config.ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    break;
                case GameModes.Keys7:
                    HitBurstQuaverSprite.Image = GameBase.LoadedSkin.NoteHitEffects7K[keyLane];
                    HitBurstQuaverSprite.SpriteEffect = !Config.ConfigManager.DownScroll7K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll7K ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    break;
            }
        }

        /// <summary>
        ///     Destroys this object.
        /// </summary>
        public override void Destroy()
        {
            QuaverContainer.Destroy();
        }

        /// <summary>
        ///     Updates Hit Burst QuaverSprite.
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(double dt)
        {
            // Update Time Elapsed + Hit Burst QuaverSprite
            TimeElapsed += dt;
            var timeRatio = (float)(TimeElapsed / DisplayTime);

            // Destroy itself if time elapsed over DISPLAY_TIME duration.
            if (TimeElapsed > DisplayTime)
            {
                DestroyReady = true;
                return;
            }

            // Update Objects
            HitBurstQuaverSprite.ScaleX = (float)(1 + (Math.Pow(timeRatio, 0.5) * MaxScale));
            HitBurstQuaverSprite.ScaleY = HitBurstQuaverSprite.ScaleX;
            HitBurstQuaverSprite.Alpha = 1 - timeRatio;

            // Update QuaverContainer
            QuaverContainer.Update(dt);
        }
    }
}
