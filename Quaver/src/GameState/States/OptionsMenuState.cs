using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
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
        private TextButton[] ManiaKeys4K { get; set; }
        private TextButton[] ManiaKeys7K { get; set; }
        private List<TextButton> SkinSelectButton { get; set; }


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
            CreateManiaKeyButtons();
            CreateSkinSelectButtons();
            CreateBackButton();
            CreateGraphicsOptionButtons();

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

        private void CreateManiaKeyButtons()
        {
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Key Bindings",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 80,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Mania 4K",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 150,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Mania 7K",
                Parent = Boundary
            };

            ManiaKeys4K = new TextButton[4];
            var keys = new Keys[4] { Configuration.KeyMania1, Configuration.KeyMania2, Configuration.KeyMania3, Configuration.KeyMania4 };
            for (var i=0; i<4; i++)
            {
                //todo: hook this to an event/method or something
                ManiaKeys4K[i] = new TextButton(new Vector2(100, 30), keys[i].ToString())
                {
                    PosY = 110,
                    PosX = (i - 1.5f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
            }

            ManiaKeys7K = new TextButton[7];
            keys = new Keys[7] { Configuration.KeyMania7k1, Configuration.KeyMania7k2, Configuration.KeyMania7k3, Configuration.KeyMania7k4, Configuration.KeyMania7k5, Configuration.KeyMania7k6, Configuration.KeyMania7k7 };
            for (var i = 0; i < 7; i++)
            {
                //todo: hook this to an event/method or something
                ManiaKeys7K[i] = new TextButton(new Vector2(100, 30), keys[i].ToString())
                {
                    PosY = 180,
                    PosX = (i - 3f) * 110f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
            }
        }

        private void CreateSkinSelectButtons()
        {
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                PosY = 220,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Select Skin",
                Parent = Boundary
            };

            SkinSelectButton = new List<TextButton>();
            var skins = new String[4] { "skin 1", "skin 2", "skin 3", "skin 4" }; //todo: this is placeholder until i get back
            for (var i = 0; i < 4; i++)
            {
                //todo: hook this to an event/method or something
                var skin = new TextButton(new Vector2(150, 30), skins[i])
                {
                    PosY = 300,
                    PosX = (i - 1.5f) * 160f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
                SkinSelectButton.Add(skin);
            }
        }

        private void CreateGraphicsOptionButtons()
        {
            TextButton button;
            var ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 70,
                PosY = 340,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Font = Fonts.Medium24,
                Text = "Graphics",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 420,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Resolution",
                Parent = Boundary
            };

            ob = new TextBoxSprite()
            {
                SizeX = 400,
                SizeY = 20,
                PosY = 490,
                TextAlignment = Alignment.BotCenter,
                Alignment = Alignment.TopCenter,
                Text = "Other",
                Parent = Boundary
            };

            for (var i = 0; i < 4; i++)
            {
                //todo: hook this to an event/method or something
                button = new TextButton(new Vector2(150, 30), "1920x1080")
                {
                    PosY = 450,
                    PosX = (i - 1.5f) * 160f,
                    Alignment = Alignment.TopCenter,
                    Parent = Boundary
                };
                //todo: add to a button list
                //SkinSelectButton.Add(button);
            }

            //Toggle Buttons
            button = new TextButton(new Vector2(200, 30), "Letterboxing: On")
            {
                PosY = 520,
                PosX = (-1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };

            button = new TextButton(new Vector2(200, 30), "FullScreen: Off")
            {
                PosY = 520,
                //PosX = (0.5f) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };

            button = new TextButton(new Vector2(200, 30), "BG Brightness: 100%")
            {
                PosY = 520,
                PosX = (1) * 210f,
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };
        }
    }
}
