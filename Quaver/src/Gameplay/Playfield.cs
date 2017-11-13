using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
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
        public static int PlayfieldObjectSize { get; set; }

        /// <summary>
        ///     The size of the playfield padding.
        /// </summary>
        private static int PlayfieldPadding { get; set; }

        private static int ReceptorPadding { get; set; }

        /// <summary>
        ///     TODO: CHANGE. Use Config Variable instead.
        /// </summary>
        public static int ReceptorYOffset { get; set; }

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
        ///     The first layer of the playfield. Used to render receptors/FX
        /// </summary>
        public static Boundary BoundaryUnder { get; set; }

        /// <summary>
        ///     The second layer of the playfield. Used to render judge/HitBurst
        /// </summary>
        public static Boundary BoundaryOver { get; set; }

        /// <summary>
        ///     The background mask of the playfield.
        /// </summary>
        public static Sprite BgMask { get; set; }

        /// <summary>
        ///     This displays the judging (MARV/PERF/GREAT/ect)
        /// </summary>
        public static Sprite JudgeSprite { get; set; }

        public static Texture2D[] JudgeImages { get; set; }

        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public static void Initialize()
        {
            // Calculate skin reference variables.
            PlayfieldObjectSize = (int)(GameBase.LoadedSkin.ColumnSize * GameBase.WindowYRatio);
            PlayfieldPadding = (int) (GameBase.LoadedSkin.BgMaskPadding * GameBase.WindowYRatio);
            ReceptorPadding = (int)(GameBase.LoadedSkin.NotePadding * GameBase.WindowYRatio);
            PlayfieldSize = (PlayfieldObjectSize + ReceptorPadding) * GameBase.SelectedBeatmap.Qua.KeyCount + PlayfieldPadding * 2 - ReceptorPadding;

            // Calculate Config stuff
            ReceptorYOffset = Config.Configuration.DownScroll ? GameBase.Window.Bottom - GameBase.LoadedSkin.ReceptorYOffset - PlayfieldObjectSize : GameBase.LoadedSkin.ReceptorYOffset;

            // Create playfield boundary & Update Rect.
            BoundaryUnder = new Boundary()
            {
                Size = new Vector2(PlayfieldSize, GameBase.Window.Height),
                Alignment = Alignment.TopCenter
            };

            BoundaryOver = new Boundary()
            {
                Size = new Vector2(PlayfieldSize, GameBase.Window.Height),
                Alignment = Alignment.TopCenter
            };

            // Create BG Mask
            BgMask = new Sprite()
            {
                Image = GameBase.LoadedSkin.ColumnBgMask,
                Parent = BoundaryUnder,
                Scale = Vector2.One
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
                ReceptorXPosition[i] = PlayfieldPadding + (PlayfieldObjectSize + ReceptorPadding) * i;

                // Create new Receptor Sprite
                Receptors[i] = new Sprite
                {
                    Image = GameBase.LoadedSkin.NoteReceptor,
                    Size = Vector2.One * PlayfieldObjectSize,
                    Position = new Vector2(ReceptorXPosition[i], ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = BoundaryUnder
                };
            }

            // Create Judge Sprite/References
            JudgeImages = new Texture2D[6]
            {
                GameBase.LoadedSkin.JudgeMarv,
                GameBase.LoadedSkin.JudgePerfect,
                GameBase.LoadedSkin.JudgeGreat,
                GameBase.LoadedSkin.JudgeGood,
                GameBase.LoadedSkin.JudgeBad,
                GameBase.LoadedSkin.JudgeMiss
            };

            //TODO: add judge scale
            //var size = new Vector2(GameBase.LoadedSkin.JudgeMiss.Width, GameBase.LoadedSkin.JudgeMiss.Height)*(float)GameBase.WindowYRatio;
            var size = new Vector2(JudgeImages[0].Width, JudgeImages[0].Height) * (float)GameBase.WindowYRatio * 0.5f;
            JudgeSprite = new Sprite()
            {
                Size = size,
                Alignment = Alignment.MidCenter,
                Image = JudgeImages[0],
                Parent = BoundaryOver
            };
        }

        /// <summary>
        ///     Draws the first layer of the Playfield (Renders before Notes)
        /// </summary>
        public static void DrawUnder()
        {
            BoundaryUnder.Draw();
        }

        /// <summary>
        ///     Draws the second layer of the Playfield (Renders after Notes)
        /// </summary>
        public static void DrawOver()
        {
            BoundaryOver.Draw();
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
                Receptors[i].PositionX = ReceptorXPosition[i] - receptorSizeOffset;
                Receptors[i].PositionY = ReceptorYOffset - receptorSizeOffset;
            }

            //Update Playfield + Children
            BoundaryUnder.Update(dt);
            BoundaryOver.Update(dt);
        }

        /// <summary>
        ///     Unloads content to free memory
        /// </summary>
        public static void UnloadContent()
        {
            BoundaryUnder.Destroy();
        }

        public static void UpdateJudge(int index)
        {
            //TODO: add judge scale
            var size = new Vector2(JudgeImages[index].Width, JudgeImages[index].Height) * (float)GameBase.WindowYRatio * 0.5f;
            JudgeSprite.Size = size;
            JudgeSprite.Image = JudgeImages[index];
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
