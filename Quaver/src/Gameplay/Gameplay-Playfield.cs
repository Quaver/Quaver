using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen : GameStateBase
    {
        //Skinning Properties/Fields
        private int _PlayFieldObjectSize = 75; //The size of each hit object.
        private int _PlayFieldPadSize = 5; //The size of the play field padding
        private int _ReceptorYOffset = 20;

        //Reference Variables
        private int _PlayFieldSize; //playfield is 400 pixels wide. Load from skin later.

        //Receptors
        private Sprite[] _Receptors;
        private float[] _ReceptorTargetSize = new float[4] { 1, 1, 1, 1 };
        private float[] _ReceptorCurrentSize = new float[4] { 1, 1, 1, 1 };
        private float[] _ReceptorXPosition;

        //Boundaries/Sprites
        private Boundary _PlayField;

        /// <summary>
        /// This method is called when the Playfield has to be initialized.
        /// </summary>
        internal void InitializePlayField()
        {
            //Calculate skin reference variables
            _PlayFieldSize = (_PlayFieldObjectSize * 4) + (_PlayFieldPadSize * 2);

            //Create PlayField Boundary
            _PlayField = new Boundary();
            _PlayField.Size = new Vector2(_PlayFieldSize, GameBase.WindowSize.Y);
            _PlayField.Alignment = Alignment.TopCenter;
            _PlayField.UpdateRect();

            //Create Receptors
            _Receptors = new Sprite[4];
            _ReceptorXPosition = new float[4];
            for (int i=0; i<4; i++)
            {
                _ReceptorXPosition[i] = _PlayFieldPadSize + (_PlayFieldObjectSize * i);
                _Receptors[i] = new Sprite
                {
                    Image = GameBase.LoadedSkin.NoteReceptor,
                    Size = Vector2.One * _PlayFieldObjectSize,
                    Position = new Vector2(_ReceptorXPosition[i], _ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = _PlayField
                };
                _Receptors[i].UpdateRect();
                Console.WriteLine(_Receptors[i].GlobalRect);
            }
        }

        /// <summary>
        /// The method is called when the Playfield is going to be updated.
        /// </summary>
        internal void UpdatePlayField(double dt)
        {
            dt = Math.Min(dt * 20, 1);

            //Update Receptors
            for (int i=0; i<4; i++)
            {
                float receptorSizeOffset = ((_ReceptorCurrentSize[i] - 1) * _PlayFieldObjectSize / 2f);
                _ReceptorCurrentSize[i] = Util.Tween(_ReceptorTargetSize[i], _ReceptorCurrentSize[i], dt);
                _Receptors[i].Size = Vector2.One * _ReceptorCurrentSize[i] * _PlayFieldObjectSize;
                _Receptors[i].Position = new Vector2(_ReceptorXPosition[i] - receptorSizeOffset, _ReceptorYOffset - receptorSizeOffset);
                _Receptors[i].UpdateRect();
            }
        }

        /// <summary>
        /// Gets called whenever a key gets pressed. This method updates the receptor state.
        /// </summary>
        /// <param name="curReceptor"></param>
        internal void UpdateReceptor(int curReceptor, bool keyDown)
        {
            if (keyDown)
            {
                //CHANGE TO RECEPTOR DOWN SKIN LATER
                _Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteHitObject1;
                _ReceptorTargetSize[curReceptor] = 1.1f;
            }
            else
            {
                _Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptor;
                _ReceptorTargetSize[curReceptor] = 1;
            }
        }
    }
}
