using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;

namespace Quaver.GameState.States
{
    class ScoreScreenState : IGameState
    {
        public State CurrentState { get; set; } = State.ScoreScreen;
        public bool UpdateReady { get; set; }

        private TextButton BackButton { get; set; }

        public void Initialize()
        {
            BackButton = new TextButton(new Vector2(300,200),"SONG SELECT" )
            {
                Alignment = Alignment.TopRight
            };
            BackButton.Clicked += OnBackButtonClick;

            UpdateReady = true;
        }

        private void OnBackButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        public void UnloadContent()
        {
            BackButton.Destroy();
        }

        public void Update(double dt)
        {
            BackButton.Update(dt);
        }

        public void Draw()
        {
            BackButton.Draw();
        }
    }
}
