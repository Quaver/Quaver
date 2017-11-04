using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Gameplay;
using Quaver.Graphics;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.GameState
{
    internal class MainMenuState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;

        //TEST
        public Button testButton;

        public void Initialize()
        {
            testButton = new Button(ButtonType.Image)
            {
                Size = new Vector2(200, 40),
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.MidCenter,
                Position = Vector2.Zero
            };
            testButton.UpdateRect();
            testButton.Clicked += ButtonClick;
            SpriteManager.AddToDrawPool(testButton);
            Console.WriteLine(testButton.GlobalRect);
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            LogManager.QuickLog("Clicked",Color.White,1f);
            GameStateManager.Instance.AddState(new SongLoadingState());
        }


        public void LoadContent()
        {
            
        }

        public void UnloadContent()
        {
            Console.WriteLine("UNLOADED MAIN MENU");
            Console.WriteLine("UNLOADED MAIN MENU");
            Console.WriteLine("UNLOADED MAIN MENU");
            Console.WriteLine("UNLOADED MAIN MENU");
            Console.WriteLine("UNLOADED MAIN MENU");
            testButton.Clicked -= ButtonClick;
            testButton.Destroy();
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw()
        {
            
        }
    }
}
