using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.Skins;
using Quaver.Utility;

namespace Quaver.Gameplay
{
    internal class Playfield
    {
        /// <summary>
        ///     The size of each HitObject.
        /// </summary>
        public static int PlayfieldObjectSize { get; } = 75;

        /// <summary>
        ///     The size of the playfield padding.
        /// </summary>
        private static int PlayfieldPadSize { get; set; } = 5;

        /// <summary>
        ///     TODO: CHANGE. Use Config Variable instead.
        /// </summary>
        public static int ReceptorYOffset { get; set; } = 40;

        /// <summary>
        ///     TODO: The Playfield size. Load from skin -- About 400px wide.
        /// </summary>
        public static int PlayfieldSize { get; set; }

        /// <summary>
        ///     The receptor sprites.
        /// </summary>
        private static Sprite[] Receptors { get; set; }

        /// <summary>
        ///     The target size for each receptors.
        /// </summary>
        private static float[] ReceptorTargetSize { get; set; }

        /// <summary>
        ///     The current size for each receptors. Used for animation.
        /// </summary>
        private static float[] ReceptorCurrentSize { get; set; }

        /// <summary>
        ///     The X-position of each receptor.
        /// </summary>
        public static float[] ReceptorXPosition { get; set; } = new float[GameBase.SelectedBeatmap.Qua.KeyCount];

        /// <summary>
        ///     The playfield Boundary
        /// </summary>
        public static Boundary Boundary { get; set; }

        public static Sprite BgMask { get; set; }

        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public static void Initialize()
        {
            // Calculate skin reference variables.
            PlayfieldSize = PlayfieldObjectSize * GameBase.SelectedBeatmap.Qua.KeyCount + PlayfieldPadSize * 2;

            // Create playfield boundary & Update Rect.
            Boundary = new Boundary()
            {
                Size = new Vector2(PlayfieldSize, GameBase.Window.Height),
                Alignment = Alignment.TopCenter
            };

            // Create BG Mask
            BgMask = new Sprite()
            {
                Image = GameBase.LoadedSkin.ColumnBgMask,
                Parent = Boundary,
                Size = Boundary.Size,
            };

            // Create Receptors
            Receptors = new Sprite[GameBase.SelectedBeatmap.Qua.KeyCount];
            ReceptorCurrentSize = new float[GameBase.SelectedBeatmap.Qua.KeyCount];
            ReceptorTargetSize = new float[GameBase.SelectedBeatmap.Qua.KeyCount];

            for (var i = 0; i < GameBase.SelectedBeatmap.Qua.KeyCount; i++)
            {
                ReceptorCurrentSize[i] = 1;
                ReceptorTargetSize[i] = 1;
            }

            for (var i = 0; i < Receptors.Length; i++)
            {
                // Set ReceptorXPos 
                ReceptorXPosition[i] = PlayfieldPadSize + PlayfieldObjectSize * i;

                // Create new Receptor Sprite
                Receptors[i] = new Sprite
                {
                    Image = GameBase.LoadedSkin.NoteReceptor,
                    Size = Vector2.One * PlayfieldObjectSize,
                    Position = new Vector2(ReceptorXPosition[i], ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = Boundary
                };
            }
        }

        /// <summary>
        ///     Updates the current playfield.
        /// </summary>
        /// <param name="dt"></param>
        public static void Update(double dt)
        {
            // Update the delta time tweening variable for animation.
            dt = Math.Min(dt / 30, 1);

            // Update receptors
            for (var i = 0; i < GameBase.SelectedBeatmap.Qua.KeyCount; i++)
            {
                var receptorSizeOffset = (ReceptorCurrentSize[i] - 1) * PlayfieldObjectSize / 2f;

                // Update receptor Size/Position
                ReceptorCurrentSize[i] = Util.Tween(ReceptorTargetSize[i], ReceptorCurrentSize[i], dt);
                Receptors[i].Size = Vector2.One * ReceptorCurrentSize[i] * PlayfieldObjectSize;
                Receptors[i].Position = new Vector2(ReceptorXPosition[i] - receptorSizeOffset, ReceptorYOffset - receptorSizeOffset);
            }

            //Update Playfield + Children
            Boundary.Update(dt);
        }

        /// <summary>
        ///     Unloads content to free memory
        /// </summary>
        public static void UnloadContent()
        {
            Boundary.Destroy();
        }

        /// <summary>
        /// Gets called whenever a key gets pressed. This method updates the receptor state.
        /// </summary>
        /// <param name="curReceptor"></param>
        public static bool UpdateReceptor(int curReceptor, bool keyDown)
        {
            if (keyDown)
            {
                //TODO: CHANGE TO RECEPTOR_DOWN SKIN LATER WHEN RECEPTOR IS PRESSED
                Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteHitObject1;
                ReceptorTargetSize[curReceptor] = 1.1f;
            }
            else
            {
                Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptor;
                ReceptorTargetSize[curReceptor] = 1;
            }

            return true;
        }
    }
}
