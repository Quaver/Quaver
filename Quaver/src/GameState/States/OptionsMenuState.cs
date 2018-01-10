using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.States
{
    class OptionsMenuState : IGameState
    {
        public State CurrentState { get; set; }
        public bool UpdateReady { get; set; }

        private Boundary Boundary { get; set; }
        private TextButton BackButton { get; set; }


        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Boundary.Draw();
            GameBase.SpriteBatch.End();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
            Logger.Log("Options Menu Button Clicked", LogColors.GameImportant, 5);
            Boundary = new Boundary();
            CreateSkinSelectButtons();
            CreateBackButton();

            UpdateReady = true;
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
            BackButton.Clicked -= BackButtonClick;
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }

        private void CreateBackButton()
        {
            //Todo: Remove. TEST.
            BackButton = new TextButton(new Vector2(200, 50), "BACK")
            {
                Alignment = Alignment.BotCenter,
                Parent = Boundary
            };
            BackButton.Clicked += BackButtonClick;
        }

        private void CreateSkinSelectButtons()
        {

        }
    }
}
