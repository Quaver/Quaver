using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
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
        private Sprites.Sprite HitBurstSprite { get; set; }

        /// <summary>
        ///     Hit Burst QuaverSprite's Parent. Used for object alignment.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     Create a new hit burst. Used after a note has been hit.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="parent"></param>
        /// <param name="keyLane"></param>
        public HitEffect(DrawRectangle rect, Drawable parent, int keyLane)
        {
            // Create QuaverContainer and Particle
            Container = new Container(rect.X, rect.Y, rect.Width, rect.Height)
            {
                Parent = parent
            };

            HitBurstSprite = new Sprites.Sprite()
            {
                Alignment = Alignment.MidCenter,
                Size = new UDim2D(0, 0, 1, 1),
                Parent = Container
            };

            // Choose the correct image based on the specific key lane.
            switch (GameBase.SelectedMap.Qua.Mode)
            {
                case GameMode.Keys4:
                    HitBurstSprite.Image = GameBase.LoadedSkin.NoteHitEffects4K[keyLane];
                    HitBurstSprite.SpriteEffect = !Config.ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    break;
                case GameMode.Keys7:
                    HitBurstSprite.Image = GameBase.LoadedSkin.NoteHitEffects7K[keyLane];
                    HitBurstSprite.SpriteEffect = !Config.ConfigManager.DownScroll7K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll7K ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    break;
            }
        }

        /// <summary>
        ///     Destroys this object.
        /// </summary>
        public override void Destroy()
        {
            Container.Destroy();
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
            HitBurstSprite.ScaleX = (float)(1 + (Math.Pow(timeRatio, 0.5) * MaxScale));
            HitBurstSprite.ScaleY = HitBurstSprite.ScaleX;
            HitBurstSprite.Alpha = 1 - timeRatio;

            // Update QuaverContainer
            Container.Update(dt);
        }
    }
}
