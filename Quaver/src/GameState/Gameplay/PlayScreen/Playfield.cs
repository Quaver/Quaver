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
        ///     The size of the playfield padding.
        /// </summary>
        private int PlayfieldPadding { get; set; }

        /// <summary>
        ///     The padding of the receptors.
        /// </summary>
        private int ReceptorPadding { get; set; }

        private int LaneSize { get; set; }

        /// <summary>
        ///     The receptor sprites.
        /// </summary>
        private Sprite[] Receptors { get; set; }

        /// <summary>
        ///     The first layer of the playfield. Used to render receptors/FX
        /// </summary>
        private Boundary Boundary { get; set; }

        /// <summary>
        ///     The background mask of the playfield.
        /// </summary>
        private Sprite BgMask { get; set; }

        /// <summary>
        ///     The size of the playfield
        /// </summary>
        private float PlayfieldSize { get; set; }

        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public void Initialize(IGameState state)
        {
            PlayScreenState playScreen = (PlayScreenState)state;
            //PlayScreen = playScreen;

            // Set default reference variables
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    GameplayReferences.ReceptorXPosition = new float[4];
                    LaneSize = (int)(GameBase.LoadedSkin.ColumnSize * GameBase.WindowUIScale);
                    GameplayReferences.ReceptorYOffset = Config.Configuration.DownScroll //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorYOffset * GameBase.WindowUIScale + (LaneSize * GameBase.LoadedSkin.NoteReceptors4K[0].Height / GameBase.LoadedSkin.NoteReceptors4K[0].Width))
                        : GameBase.LoadedSkin.ReceptorYOffset * GameBase.WindowUIScale;
                    break;
                case GameModes.Keys7:
                    GameplayReferences.ReceptorXPosition = new float[7];
                    LaneSize = (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale);
                    GameplayReferences.ReceptorYOffset = Config.Configuration.DownScroll //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorYOffset * GameBase.WindowUIScale + (LaneSize * GameBase.LoadedSkin.NoteReceptors7K[0].Height / GameBase.LoadedSkin.NoteReceptors7K[0].Width))
                        : GameBase.LoadedSkin.ReceptorYOffset * GameBase.WindowUIScale;
                    break;
            }

            // Calculate Config stuff
            PlayfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding * GameBase.WindowUIScale);
            ReceptorPadding = (int)(GameBase.LoadedSkin.NotePadding * GameBase.WindowUIScale);
            PlayfieldSize = ((LaneSize + ReceptorPadding) * GameplayReferences.ReceptorXPosition.Length) + (PlayfieldPadding * 2) - ReceptorPadding;
            GameplayReferences.PlayfieldSize = PlayfieldSize;

            // Create playfield boundary
            Boundary = new Boundary()
            {
                Size = new UDim2(PlayfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };

            // Create BG Mask
            BgMask = new Sprite()
            {
                //Image = GameBase.LoadedSkin.ColumnBgMask,
                Tint = Color.Black, //todo: remove
                Alpha = 0.8f, //todo: remove
                Parent = Boundary,
                Size = new UDim2(0, 0, 1, 1)
            };

            // Create Receptors
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    Receptors = new Sprite[4];
                    break;
                case GameModes.Keys7:
                    Receptors = new Sprite[7];
                    break;
            }

            for (var i = 0; i < Receptors.Length; i++)
            {
                // Set ReceptorXPos 
                GameplayReferences.ReceptorXPosition[i] = ((LaneSize + ReceptorPadding) * i) + PlayfieldPadding;

                // Create new Receptor Sprite
                Receptors[i] = new Sprite
                {
                    Size = new UDim2(LaneSize, 0),
                    Position = new UDim2(GameplayReferences.ReceptorXPosition[i], GameplayReferences.ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = Boundary
                };

                // Set current receptor's image based on the current key count.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        Receptors[i].Image = GameBase.LoadedSkin.NoteReceptors4K[i];
                        Receptors[i].SizeY = LaneSize * GameBase.LoadedSkin.NoteReceptors4K[i].Height / GameBase.LoadedSkin.NoteReceptors4K[i].Width;
                        break;
                    case GameModes.Keys7:
                        Receptors[i].Image = GameBase.LoadedSkin.NoteReceptors7K[i];
                        Receptors[i].SizeY = LaneSize * GameBase.LoadedSkin.NoteReceptors7K[i].Height / GameBase.LoadedSkin.NoteReceptors7K[i].Width;
                        break;
                }
            }
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        /// <summary>
        ///     Updates the current playfield.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        /// <summary>
        ///     Unloads content to free memory
        /// </summary>
        public  void UnloadContent()
        {
            Boundary.Destroy();
        }

        /// <summary>
        /// Gets called whenever a key gets pressed. This method updates the receptor state.
        /// </summary>
        /// <param name="curReceptor"></param>
        public bool UpdateReceptor(int curReceptor, bool keyDown)
        {
            if (keyDown)
            {
                //TODO: CHANGE TO RECEPTOR_DOWN SKIN LATER WHEN RECEPTOR IS PRESSED
                //Receptors[curReceptor].Image = GameBase.LoadedSkin.ColumnHitLighting;
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptorsDown4K[curReceptor];
                        break;
                    case GameModes.Keys7:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.ColumnHitLighting;
                        //Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptorsDown7K[curReceptor];
                        break;
                }
            }
            else
            {
                // Set current receptor's image based on the current key count.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptors4K[curReceptor];
                        break;
                    case GameModes.Keys7:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptors7K[curReceptor];
                        break;
                }
            }

            return true;
        }
    }
}
