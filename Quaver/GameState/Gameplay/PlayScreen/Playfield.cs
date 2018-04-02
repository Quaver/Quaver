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
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Skins;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    internal class Playfield : IHelper
    {
        /// <summary>
        ///     The receptor sprites.
        /// </summary>
        internal QuaverSprite[] ReceptorObjects { get; set; }

        /// <summary>
        ///     The Hit Lighting Sprites. Will be visible when designated key is held down.
        /// </summary>
        internal QuaverSprite[] ColumnLightingObjects { get; set; }

        /// <summary>
        ///     The first layer of the playfield. Used to render playfield mask + ColumnLighting (+ receptors if set in skin.ini)
        /// </summary>
        private Boundary ForegroundBoundary { get; set; }

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
        internal float ReceptorYPosition { get; set; }

        /// <summary>
        ///     Determines Position of Column Lighting Objects
        /// </summary>
        internal float ColumnLightingPosition { get; set; }

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
            ForegroundBoundary = new Boundary()
            {
                Size = new UDim2(PlayfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };

            BackgroundBoundary = new Boundary()
            {
                Size = new UDim2(PlayfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };

            // Create Stage Left
            var borderSize = GameBase.LoadedSkin.StageLeftBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageLeftBorder.Height;
            var stage = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageLeftBorder,
                Size = new UDim2(borderSize, GameBase.WindowRectangle.Height),
                Position = new UDim2(-borderSize + 1, 0),
                Alignment = Alignment.TopLeft,
                Parent = BackgroundBoundary
            };

            // Create Stage Right
            borderSize = GameBase.LoadedSkin.StageRightBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageRightBorder.Height;
            stage = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageRightBorder,
                Size = new UDim2(borderSize, GameBase.WindowRectangle.Height),
                Position = new UDim2(borderSize - 1, 0),
                Alignment = Alignment.TopRight,
                Parent = BackgroundBoundary
            };

            // todo: code cleanup
            // Create Receptors + Hit Lighting + Bg Mask
            double imageRatio;
            double columnRatio;
            float bgMaskSize;
            float overlaySize;
            float posOffset;
            QuaverSprite bgMask;
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    // Create BG Mask
                    imageRatio = (double)GameBase.LoadedSkin.StageBgMask4K.Width / GameBase.LoadedSkin.StageBgMask4K.Height;
                    columnRatio = PlayfieldSize / GameBase.WindowRectangle.Height;
                    bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);

                    bgMask = new QuaverSprite()
                    {
                        Image = GameBase.LoadedSkin.StageBgMask4K,
                        Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                        Size = new UDim2(PlayfieldSize, bgMaskSize),
                        Alignment = Alignment.MidCenter,
                        Parent = BackgroundBoundary
                    };

                    // Create Receptors + Hit Lighting
                    ReceptorObjects = new QuaverSprite[4];
                    ColumnLightingObjects = new QuaverSprite[4];
                    ColumnLightingActive = new bool[4];
                    ColumnLightingAnimation = new float[4];
                    for (var i = 0; i < ReceptorObjects.Length; i++)
                    {
                        // Set ReceptorXPos 
                        GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                        // Create receptor QuaverSprite
                        ReceptorObjects[i] = new QuaverSprite
                        {
                            Size = new UDim2(LaneSize, LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[i].Width),
                            Position = new UDim2(GameplayReferences.ReceptorXPosition[i], ReceptorYPosition),
                            Alignment = Alignment.TopLeft,
                            Image = GameBase.LoadedSkin.NoteReceptorsUp4K[i],
                            SpriteEffect = !Config.ConfigManager.DownScroll4k && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                            Parent = ForegroundBoundary
                        };

                        // Create hit lighting sprite
                        var columnLightingSize = GameBase.LoadedSkin.ColumnLightingScale * LaneSize * ((float)GameBase.LoadedSkin.ColumnLighting4K.Height / GameBase.LoadedSkin.ColumnLighting4K.Width);
                        ColumnLightingObjects[i] = new QuaverSprite
                        {
                            Image = GameBase.LoadedSkin.ColumnLighting4K,
                            Size = new UDim2(LaneSize, columnLightingSize),
                            Tint = GameBase.LoadedSkin.ColumnColors4K[i],
                            PosX = GameplayReferences.ReceptorXPosition[i],
                            PosY = Config.ConfigManager.DownScroll4k ? ColumnLightingPosition - columnLightingSize : ColumnLightingPosition,
                            SpriteEffect = !Config.ConfigManager.DownScroll4k && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                            Alignment = Alignment.TopLeft,
                            Parent = BackgroundBoundary
                        };
                    }

                    // Create Stage Distant
                    overlaySize = GameBase.LoadedSkin.StageDistantOverlay.Height * PlayfieldSize / GameBase.LoadedSkin.StageDistantOverlay.Width;
                    stage = new QuaverSprite()
                    {
                        Image = GameBase.LoadedSkin.StageDistantOverlay,
                        Size = new UDim2(PlayfieldSize, overlaySize),
                        PosY = Config.ConfigManager.DownScroll4k ? -1 : 1,
                        Alignment = Config.ConfigManager.DownScroll4k ? Alignment.TopRight : Alignment.BotRight,
                        Parent = ForegroundBoundary
                    };

                    // Create Stage HitPosition Overlay
                    overlaySize = GameBase.LoadedSkin.StageHitPositionOverlay.Height * PlayfieldSize / GameBase.LoadedSkin.StageHitPositionOverlay.Width;
                    posOffset = LaneSize * ((float)GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width);
                    stage = new QuaverSprite()
                    {
                        Image = GameBase.LoadedSkin.StageHitPositionOverlay,
                        Size = new UDim2(PlayfieldSize, overlaySize),
                        PosY = Config.ConfigManager.DownScroll4k ? ReceptorYPosition : ReceptorYPosition + posOffset + overlaySize,
                        Parent = ForegroundBoundary
                    };
                    break;
                case GameModes.Keys7:
                    // Create BG Mask
                    imageRatio = (double)GameBase.LoadedSkin.StageBgMask7K.Width / GameBase.LoadedSkin.StageBgMask7K.Height;
                    columnRatio = PlayfieldSize / GameBase.WindowRectangle.Height;
                    bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);

                    bgMask = new QuaverSprite()
                    {
                        Image = GameBase.LoadedSkin.StageBgMask7K,
                        Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                        Size = new UDim2(PlayfieldSize, bgMaskSize),
                        Alignment = Alignment.MidCenter,
                        Parent = BackgroundBoundary
                    };

                    // Create Receptors + HitLighting
                    ReceptorObjects = new QuaverSprite[7];
                    ColumnLightingObjects = new QuaverSprite[7];
                    ColumnLightingActive = new bool[7];
                    ColumnLightingAnimation = new float[7];
                    for (var i = 0; i < ReceptorObjects.Length; i++)
                    {
                        // Set ReceptorXPos 
                        GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                        // Create receptor QuaverSprite
                        ReceptorObjects[i] = new QuaverSprite
                        {
                            Size = new UDim2(LaneSize, LaneSize * ((float)GameBase.LoadedSkin.NoteReceptorsUp7K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[i].Width)),
                            Position = new UDim2(GameplayReferences.ReceptorXPosition[i], ReceptorYPosition),
                            Alignment = Alignment.TopLeft,
                            Image = GameBase.LoadedSkin.NoteReceptorsUp7K[i],
                            SpriteEffect = !Config.ConfigManager.DownScroll7k && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll7K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                            Parent = ForegroundBoundary
                        };

                        // Create hit lighting sprite
                        var columnLightingSize = LaneSize * GameBase.LoadedSkin.ColumnLightingScale * GameBase.LoadedSkin.ColumnLighting7K.Height / GameBase.LoadedSkin.ColumnLighting7K.Width;
                        ColumnLightingObjects[i] = new QuaverSprite
                        {
                            Image = GameBase.LoadedSkin.ColumnLighting7K,
                            Size = new UDim2(LaneSize, columnLightingSize),
                            Tint = GameBase.LoadedSkin.ColumnColors7K[i],
                            PosX = GameplayReferences.ReceptorXPosition[i],
                            PosY = Config.ConfigManager.DownScroll7k ? ColumnLightingPosition - columnLightingSize : ColumnLightingPosition,
                            SpriteEffect = !Config.ConfigManager.DownScroll7k && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll7K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                            Alignment = Alignment.TopLeft,
                            Parent = BackgroundBoundary
                        };
                    }

                    // Create StageDistant
                    overlaySize = GameBase.LoadedSkin.StageDistantOverlay.Height * PlayfieldSize / GameBase.LoadedSkin.StageDistantOverlay.Width;
                    stage = new QuaverSprite()
                    {
                        Image = GameBase.LoadedSkin.StageDistantOverlay,
                        Size = new UDim2(PlayfieldSize, overlaySize),
                        PosY = Config.ConfigManager.DownScroll7k ? -1 : 1,
                        Alignment = Config.ConfigManager.DownScroll7k ? Alignment.TopRight : Alignment.BotRight,
                        Parent = ForegroundBoundary
                    };

                    // Create Stage HitPosition Overlay
                    overlaySize = GameBase.LoadedSkin.StageHitPositionOverlay.Height * PlayfieldSize / GameBase.LoadedSkin.StageHitPositionOverlay.Width;
                    posOffset = LaneSize * ((float)GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width);
                    stage = new QuaverSprite()
                    {
                        Image = GameBase.LoadedSkin.StageHitPositionOverlay,
                        Size = new UDim2(PlayfieldSize, overlaySize),
                        PosY = Config.ConfigManager.DownScroll7k ? ReceptorYPosition : ReceptorYPosition + posOffset + overlaySize,
                        Parent = ForegroundBoundary
                    };
                    break;
            }
        }

        public void DrawBgMask()
        {
            BackgroundBoundary.Draw();
        }

        public void Draw()
        {
            ForegroundBoundary.Draw();
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
                    ColumnLightingAnimation[i] = GraphicsHelper.Tween(1, ColumnLightingAnimation[i], Math.Min(dt / 2, 1));
                else
                    ColumnLightingAnimation[i] = GraphicsHelper.Tween(0, ColumnLightingAnimation[i], Math.Min(dt / 60, 1));

                // Update Hit Lighting Object
                ColumnLightingObjects[i].Alpha = ColumnLightingAnimation[i];
            }

            ForegroundBoundary.Update(dt);
            BackgroundBoundary.Update(dt);
        }

        /// <summary>
        ///     Unloads content to free memory
        /// </summary>
        public  void UnloadContent()
        {
            ForegroundBoundary.Destroy();
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
