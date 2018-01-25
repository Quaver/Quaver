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
        private Sprite[] Receptors { get; set; }

        /// <summary>
        ///     The first layer of the playfield. Used to render receptors/FX
        /// </summary>
        private Boundary Boundary { get; set; }

        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public void Initialize(IGameState state)
        {
            PlayScreenState playScreen = (PlayScreenState)state;
            int laneSize = 0;
            //PlayScreen = playScreen;

            // Set default reference variables
            int playfieldPadding = 0;
            int receptorPadding = 0;
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    GameplayReferences.ReceptorXPosition = new float[4];
                    laneSize = (int)(GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale);
                    playfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding4K * GameBase.WindowUIScale);
                    receptorPadding = (int)(GameBase.LoadedSkin.NotePadding4K * GameBase.WindowUIScale);
                    GameplayReferences.ReceptorYOffset = Config.Configuration.DownScroll4k //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorYOffset4K * GameBase.WindowUIScale + (laneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width))
                        : GameBase.LoadedSkin.ReceptorYOffset4K * GameBase.WindowUIScale;
                    break;
                case GameModes.Keys7:
                    GameplayReferences.ReceptorXPosition = new float[7];
                    laneSize = (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale);
                    playfieldPadding = (int)(GameBase.LoadedSkin.BgMaskPadding7K * GameBase.WindowUIScale);
                    receptorPadding = (int)(GameBase.LoadedSkin.NotePadding7K * GameBase.WindowUIScale);
                    GameplayReferences.ReceptorYOffset = Config.Configuration.DownScroll7k //todo: use list for scaling
                        ? GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorYOffset7K * GameBase.WindowUIScale + (laneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width))
                        : GameBase.LoadedSkin.ReceptorYOffset7K * GameBase.WindowUIScale;
                    break;
            }

            // Calculate Config stuff
            var playfieldSize = ((laneSize + receptorPadding) * GameplayReferences.ReceptorXPosition.Length) + (playfieldPadding * 2) - receptorPadding;
            GameplayReferences.PlayfieldSize = playfieldSize;

            // Create playfield boundary
            Boundary = new Boundary()
            {
                Size = new UDim2(playfieldSize, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };

            // Create BG Mask
            var bgMask = new Sprite()
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
                GameplayReferences.ReceptorXPosition[i] = ((laneSize + receptorPadding) * i) + playfieldPadding;

                // Create new Receptor Sprite
                Receptors[i] = new Sprite
                {
                    Size = new UDim2(laneSize, 0),
                    Position = new UDim2(GameplayReferences.ReceptorXPosition[i], GameplayReferences.ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = Boundary
                };

                // Set current receptor's image based on the current key count.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        Receptors[i].Image = GameBase.LoadedSkin.NoteReceptorsUp4K[i];
                        Receptors[i].SizeY = laneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[i].Width;
                        break;
                    case GameModes.Keys7:
                        Receptors[i].Image = GameBase.LoadedSkin.NoteReceptorsUp7K[i];
                        Receptors[i].SizeY = laneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[i].Width;
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
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptorsDown7K[curReceptor];
                        break;
                }
            }
            else
            {
                // Set current receptor's image based on the current key count.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptorsUp4K[curReceptor];
                        break;
                    case GameModes.Keys7:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptorsUp7K[curReceptor];
                        break;
                }
            }

            return true;
        }
    }
}
