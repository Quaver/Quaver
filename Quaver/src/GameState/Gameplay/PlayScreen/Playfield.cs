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
        internal Sprite[] ReceptorObjects { get; set; }

        /// <summary>
        ///     The Hit Lighting Sprites. Will be visible when designated key is held down.
        /// </summary>
        internal Sprite[] ColumnLightingObjects { get; set; }

        /// <summary>
        ///     The first layer of the playfield. Used to render playfield mask + ColumnLighting (+ receptors if set in skin.ini)
        /// </summary>
        private Boundary ReceptorBoundary { get; set; }

        /// <summary>
        ///     The second layer of the playfield. Used to render receptors
        /// </summary>
        private Boundary BackgroundBoundary { get; set; }

        /// <summary>
        ///     Size of the playfield
        /// </summary>
        internal float PlayfieldSize { get; set; }

        /// <summary>
        ///     Size of each lane
        /// </summary>
        internal float LaneSize { get; set; }

        /// <summary>
        ///     Gap size between each note
        /// </summary>
        internal float ReceptorPadding { get; set; }

        /// <summary>
        ///     Gap size between note and edge of playfield
        /// </summary>
        internal float PlayfieldPadding { get; set; }

        /// <summary>
        ///     Receptor Y-Offset from screen
        /// </summary>
        internal float ReceptorYOffset { get; set; }

        /// <summary>
        ///     Is determined if hit lighting object should be active
        /// </summary>
        private bool[] ColumnLightingActive { get; set; }

        /// <summary>
        ///     Determines hit lighting object animation from scale of 0 -> 1.
        /// </summary>
        private float[] ColumnLightingAnimation { get; set; }


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
                    ReceptorObjects = new Sprite[4];
                    ColumnLightingObjects = new Sprite[4];
                    ColumnLightingActive = new bool[4];
                    ColumnLightingAnimation = new float[4];
                    for (var i = 0; i < ReceptorObjects.Length; i++)
                    {
                        // Set ReceptorXPos 
                        GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                        // Create receptor Sprite
                        ReceptorObjects[i] = new Sprite
                        {
                            Size = new UDim2(LaneSize, LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[i].Width),
                            Position = new UDim2(GameplayReferences.ReceptorXPosition[i], ReceptorYOffset),
                            Alignment = Alignment.TopLeft,
                            Image = GameBase.LoadedSkin.NoteReceptorsUp4K[i],
                            Parent = ReceptorBoundary
                        };

                        // Create hit lighting sprite
                        var columnLightingSize = LaneSize * GameBase.LoadedSkin.ColumnLighting4K.Height / GameBase.LoadedSkin.ColumnLighting4K.Width;
                        ColumnLightingObjects[i] = new Sprite
                        {
                            Image = GameBase.LoadedSkin.ColumnLighting4K,
                            Size = new UDim2(LaneSize, columnLightingSize * GameBase.LoadedSkin.ColumnLightingScale),
                            Tint = GameBase.LoadedSkin.ColumnColors4K[i],
                            Alpha = 0.25f,
                            PosX = GameplayReferences.ReceptorXPosition[i],
                            PosY = Config.Configuration.DownScroll4k ? ReceptorYOffset - columnLightingSize : ReceptorYOffset + columnLightingSize,
                            SpriteEffect = Config.Configuration.DownScroll4k ? SpriteEffects.None : SpriteEffects.FlipVertically,
                            Alignment = Config.Configuration.DownScroll4k ? Alignment.TopLeft : Alignment.BotLeft,
                            Parent = BackgroundBoundary
                        };
                    }
                    break;
                case GameModes.Keys7:
                    ReceptorObjects = new Sprite[7];
                    ColumnLightingObjects = new Sprite[7];
                    ColumnLightingActive = new bool[7];
                    ColumnLightingAnimation = new float[7];
                    for (var i = 0; i < ReceptorObjects.Length; i++)
                    {
                        // Set ReceptorXPos 
                        GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                        // Create receptor Sprite
                        ReceptorObjects[i] = new Sprite
                        {
                            Size = new UDim2(LaneSize, LaneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[i].Width),
                            Position = new UDim2(GameplayReferences.ReceptorXPosition[i], ReceptorYOffset),
                            Alignment = Alignment.TopLeft,
                            Image = GameBase.LoadedSkin.NoteReceptorsUp7K[i],
                            Parent = ReceptorBoundary
                        };

                        // Create hit lighting sprite
                        var columnLightingSize = LaneSize * GameBase.LoadedSkin.ColumnLighting7K.Height / GameBase.LoadedSkin.ColumnLighting7K.Width;
                        ColumnLightingObjects[i] = new Sprite
                        {
                            Image = GameBase.LoadedSkin.ColumnLighting7K,
                            Size = new UDim2(LaneSize, columnLightingSize * GameBase.LoadedSkin.ColumnLightingScale),
                            Tint = GameBase.LoadedSkin.ColumnColors7K[i],
                            Alpha = 0.25f,
                            PosX = GameplayReferences.ReceptorXPosition[i],
                            PosY = Config.Configuration.DownScroll7k ? ReceptorYOffset - columnLightingSize : ReceptorYOffset + columnLightingSize,
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
            for (var i = 0; i < ColumnLightingActive.Length; i++)
            {
                // Update ColumnLighting Animation
                if (ColumnLightingActive[i])
                    ColumnLightingAnimation[i] = Util.Tween(1, ColumnLightingAnimation[i], Math.Min(dt / 2, 1));
                else
                    ColumnLightingAnimation[i] = Util.Tween(0, ColumnLightingAnimation[i], Math.Min(dt / 60, 1));

                // Update Hit Lighting Object
                ColumnLightingObjects[i].Alpha = ColumnLightingAnimation[i];
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
                        ReceptorObjects[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsDown4K[keyIndex];
                        ColumnLightingActive[keyIndex] = true;
                        ColumnLightingAnimation[keyIndex] = 1;
                    }
                    else
                    {
                        ReceptorObjects[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsUp4K[keyIndex];
                        ColumnLightingActive[keyIndex] = false;
                    }
                    break;
                case GameModes.Keys7:
                    if (keyDown)
                    {
                        ReceptorObjects[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsDown7K[keyIndex];
                        ColumnLightingActive[keyIndex] = true;
                        ColumnLightingAnimation[keyIndex] = 1;
                    }
                    else
                    {
                        ReceptorObjects[keyIndex].Image = GameBase.LoadedSkin.NoteReceptorsUp7K[keyIndex];
                        ColumnLightingActive[keyIndex] = false;
                    }
                    break;
            }
        }
    }
}
