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
        public static int ReceptorYOffset { get; set; } = 20;

        /// <summary>
        ///     TODO: The Playfield size. Load from skin -- About 400px wide.
        /// </summary>
        private static int PlayfieldSize { get; set; }

        /// <summary>
        ///     The receptor sprites.
        /// </summary>
        private static Sprite[] Receptors { get; set; }

        /// <summary>
        ///     The target size for each receptors.
        /// </summary>
        private static float[] ReceptorTargetSize { get; set; } = { 1.0f, 1.0f, 1.0f, 1.0f };

        /// <summary>
        ///     The current size for each receptors. Used for animation.
        /// </summary>
        private static float[] ReceptorCurrentSize { get; set; } = { 1.0f, 1.0f, 1.0f, 1.0f };

        /// <summary>
        ///     The X-position of each receptor.
        /// </summary>
        public static float[] ReceptorXPosition { get; set; } = new float[4];

        /// <summary>
        ///     The playfield Boundary
        /// </summary>
        public static Boundary PlayfieldBoundary { get; set; } = new Boundary();

        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public static void InitializePlayfield()
        {
            // Calculate skin reference variables.
            PlayfieldSize = PlayfieldObjectSize * 4 + PlayfieldPadSize * 2;

            // Create playfield boundary & Update Rect.
            PlayfieldBoundary = new Boundary()
            {
                Size = new Vector2(PlayfieldSize, GameBase.Window.Height),
                Alignment = Alignment.TopCenter
            };

            PlayfieldBoundary.UpdateRect();

            // Create Receptors
            Receptors = new Sprite[4];

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
                    Parent = PlayfieldBoundary
                };

                Receptors[i].UpdateRect();
            }
            SpriteManager.AddToDrawList(PlayfieldBoundary);
        }

        /// <summary>
        ///     Updates the current playfield.
        /// </summary>
        /// <param name="dt"></param>
        public static void UpdatePlayfield(double dt)
        {
            // The delta time tweening variable for animation.
            dt = Math.Min(dt / 30, 1);

            // Update receptors
            for (var i = 0; i < 4; i++)
            {
                var receptorSizeOffset = (ReceptorCurrentSize[i] - 1) * PlayfieldObjectSize / 2f;

                // Update receptor Size/Position
                ReceptorCurrentSize[i] = Util.Tween(ReceptorTargetSize[i], ReceptorCurrentSize[i], dt);
                Receptors[i].Size = Vector2.One * ReceptorCurrentSize[i] * PlayfieldObjectSize;
                Receptors[i].Position = new Vector2(ReceptorXPosition[i] - receptorSizeOffset, ReceptorYOffset - receptorSizeOffset);
                Receptors[i].UpdateRect();
            }
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
