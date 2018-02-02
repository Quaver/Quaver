using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;

using Quaver.Skins;
using Quaver.Utility;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    internal class Playfield : IHelper
    {
        /// <summary>
        ///     The receptor sprites.
        /// </summary>
        internal Sprite[] Receptors { get; set; }

        /// <summary>
        ///     The Hit Lighting Sprites. Will be visible when designated key is held down.
        /// </summary>
        internal Sprite[] HitLightingObjects { get; set; }

        /// <summary>
        ///     The first layer of the playfield. Used to render receptors/FX
        /// </summary>
        private Boundary ReceptorBoundary { get; set; }

        private Boundary BackgroundBoundary { get; set; }

        internal float PlayfieldSize { get; set; }

        internal float LaneSize { get; set; }

        internal float ReceptorPadding { get; set; }

        internal float PlayfieldPadding { get; set; }

        internal float ReceptorYOffset { get; set; }

        private bool[] HitLightingActive { get; set; }

        private float[] HitLightingAnimation { get; set; }


        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public void Initialize(IGameState state)
        {
            //PlayScreen = playScreen;

            // Create playfield boundary
            ReceptorBoundary = new Boundary()
            {
                Size = new UDim2(PlayfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };

            BackgroundBoundary = new Boundary()
            {
                Size = new UDim2(PlayfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };

            // Create BG Mask
            var bgMask = new Sprite()
            {
                //Image = GameBase.LoadedSkin.ColumnBgMask,
                Tint = Color.Black, //todo: remove
                Alpha = 0.8f, //todo: remove
                Size = new UDim2(PlayfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter,
                Parent = BackgroundBoundary
            };

            // Create Stage Left
            var borderSize = GameBase.LoadedSkin.StageLeftBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageLeftBorder.Height;
            var stage = new Sprite()
            {
                Image = GameBase.LoadedSkin.StageLeftBorder,
                Size = new UDim2(borderSize, GameBase.WindowRectangle.Height),
                Position = new UDim2(-borderSize + 1, 0),
                Alignment = Alignment.TopLeft,
                Parent = BackgroundBoundary
            };

            // Create Stage Right
            borderSize = GameBase.LoadedSkin.StageRightBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageRightBorder.Height;
            stage = new Sprite()
            {
                Image = GameBase.LoadedSkin.StageRightBorder,
                Size = new UDim2(borderSize, GameBase.WindowRectangle.Height),
                Position = new UDim2(borderSize - 1, 0),
                Alignment = Alignment.TopRight,
                Parent = BackgroundBoundary
            };

            // Create Receptors
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    Receptors = new Sprite[4];
                    HitLightingObjects = new Sprite[4];
                    HitLightingActive = new bool[4];
                    HitLightingAnimation = new float[4];
                    for (var i = 0; i < Receptors.Length; i++)
                    {
                        // Set ReceptorXPos 
                        GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                        // Create receptor Sprite
                        Receptors[i] = new Sprite
                        {
                            Size = new UDim2(LaneSize, LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[i].Width),
                            Position = new UDim2(GameplayReferences.ReceptorXPosition[i], ReceptorYOffset),
                            Alignment = Alignment.TopLeft,
                            Image = GameBase.LoadedSkin.NoteReceptorsUp4K[i],
                            Parent = ReceptorBoundary
                        };

                        // Create hit lighting sprite
                        var hitLightingSize = LaneSize * GameBase.LoadedSkin.ColumnHitLighting4K[i].Height / GameBase.LoadedSkin.ColumnHitLighting4K[i].Width;
                        HitLightingObjects[i] = new Sprite
                        {
                            Image = GameBase.LoadedSkin.ColumnHitLighting4K[i],
                            Size = new UDim2(LaneSize, hitLightingSize * GameBase.LoadedSkin.HitLightingScale),
                            Alpha = 0.25f,
                            PosX = GameplayReferences.ReceptorXPosition[i],
                            PosY = Config.Configuration.DownScroll4k ? ReceptorYOffset - hitLightingSize : ReceptorYOffset + hitLightingSize,
                            SpriteEffect = Config.Configuration.DownScroll4k ? SpriteEffects.None : SpriteEffects.FlipVertically,
                            Alignment = Config.Configuration.DownScroll4k ? Alignment.TopLeft : Alignment.BotLeft,
                            Parent = BackgroundBoundary
                        };
                    }
                    break;
                case GameModes.Keys7:
                    Receptors = new Sprite[7];
                    HitLightingObjects = new Sprite[7];
                    HitLightingActive = new bool[7];
                    HitLightingAnimation = new float[7];
                    for (var i = 0; i < Receptors.Length; i++)
                    {
                        // Set ReceptorXPos 
                        GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                        // Create receptor Sprite
                        Receptors[i] = new Sprite
                        {
                            Size = new UDim2(LaneSize, LaneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[i].Width),
                            Position = new UDim2(GameplayReferences.ReceptorXPosition[i], ReceptorYOffset),
                            Alignment = Alignment.TopLeft,
                            Image = GameBase.LoadedSkin.NoteReceptorsUp7K[i],
                            Parent = ReceptorBoundary
                        };

                        // Create hit lighting sprite
                        var hitLightingSize = LaneSize * GameBase.LoadedSkin.ColumnHitLighting7K[i].Height / GameBase.LoadedSkin.ColumnHitLighting7K[i].Width;
                        HitLightingObjects[i] = new Sprite
                        {
                            Image = GameBase.LoadedSkin.ColumnHitLighting7K[i],
                            Size = new UDim2(LaneSize, hitLightingSize * GameBase.LoadedSkin.HitLightingScale),
                            Alpha = 0.25f,
                            PosX = GameplayReferences.ReceptorXPosition[i],
                            PosY = Config.Configuration.DownScroll7k ? ReceptorYOffset - hitLightingSize : ReceptorYOffset + hitLightingSize,
                            SpriteEffect = Config.Configuration.DownScroll7k ? SpriteEffects.None : SpriteEffects.FlipVertically,
                            Alignment = Config.Configuration.DownScroll7k ? Alignment.TopLeft : Alignment.BotLeft,
                            Parent = BackgroundBoundary
                        };
                    }
                    break;
            }
        }

        public void DrawBgMask()
        {
            BackgroundBoundary.Draw();
        }

        public void Draw()
        {
            ReceptorBoundary.Draw();
        }

        /// <summary>
        ///     Updates the current playfield.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            for (var i = 0; i < HitLightingActive.Length; i++)
            {
                // Update HitLighting Animation
                if (HitLightingActive[i])
                    HitLightingAnimation[i] = Util.Tween(1, HitLightingAnimation[i], Math.Min(dt / 2, 1));
                else
                    HitLightingAnimation[i] = Util.Tween(0, HitLightingAnimation[i], Math.Min(dt / 60, 1));

                // Update Hit Lighting Object
                HitLightingObjects[i].Alpha = HitLightingAnimation[i];
            }

            ReceptorBoundary.Update(dt);
            BackgroundBoundary.Update(dt);
        }

        /// <summary>
        ///     Unloads content to free memory
        /// </summary>
        public  void UnloadContent()
        {
            ReceptorBoundary.Destroy();
            BackgroundBoundary.Destroy();
        }

        /// <summary>
        /// Gets called whenever a key gets pressed. This method updates the receptor state.
        /// </summary>
        /// <param name="keyIndex"></param>
        public void UpdateReceptor(int keyIndex, bool keyDown)
        {
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    if (keyDown)
                    {
                        Receptors[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsDown4K[keyIndex];
                        HitLightingActive[keyIndex] = true;
                        HitLightingAnimation[keyIndex] = 1;
                    }
                    else
                    {
                        Receptors[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsUp4K[keyIndex];
                        HitLightingActive[keyIndex] = false;
                    }
                    break;
                case GameModes.Keys7:
                    if (keyDown)
                    {
                        Receptors[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsDown7K[keyIndex];
                        HitLightingActive[keyIndex] = true;
                        HitLightingAnimation[keyIndex] = 1;
                    }
                    else
                    {
                        Receptors[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsUp7K[keyIndex];
                        HitLightingActive[keyIndex] = false;
                    }
                    break;
            }
        }
    }
}
