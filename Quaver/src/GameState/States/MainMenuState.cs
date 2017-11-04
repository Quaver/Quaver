using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.GameState
{
    internal class MainMenuState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;

        //TEST
        public Sprite testButton;

        public void Initialize()
        {
            testButton = new Sprite()
            {
                Size = new Vector2(200, 40),
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.MidCenter,
                Position = Vector2.Zero
            };
            testButton.UpdateRect();
            SpriteManager.AddToDrawList(testButton);
            Console.WriteLine(testButton.GlobalRect);
        }

        public void LoadContent()
        {
            
        }

        public void UnloadContent()
        {
            
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw()
        {
            
        }
    }
}
