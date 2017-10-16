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

        //Sprites
        private Sprite[] _Receptors;

        //Boundaries
        private Boundary _PlayField;

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
            for (int i=0; i<4; i++)
            {
                _Receptors[i] = new Sprite
                {
                    Image = GameBase.LoadedSkin.NoteReceptor,
                    Size = Vector2.One * _PlayFieldObjectSize,
                    Position = new Vector2(_PlayFieldPadSize + (_PlayFieldObjectSize * i), _ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = _PlayField
                };
                _Receptors[i].UpdateRect();
                Console.WriteLine(_Receptors[i].GlobalRect);
            }
        }
    }
}
