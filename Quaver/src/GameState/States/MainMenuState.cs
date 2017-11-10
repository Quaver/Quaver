using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Discord;
using Quaver.Gameplay;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Peppy;

namespace Quaver.GameState.States
{
    internal class MainMenuState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;
        public bool UpdateReady { get; set; }

        public Boundary Boundary;

        //TEST
        public Button testButton;
        public Button importPeppyButton;

        public void Initialize()
        {
            // Initialize the main menu's audio player.
            MenuAudioPlayer.Initialize();

            //Initialize Menu Screen
            Boundary = new Boundary();

            //Initialize Test Buttons TODO: Remove later
            testButton = new TextButton(new Vector2(200, 40), "Next State")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.MidCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };
            testButton.UpdateRect();
            testButton.Clicked += ButtonClick;

            importPeppyButton = new TextButton(new Vector2(200, 40), "Import .osz")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.TopCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };
            importPeppyButton.UpdateRect();
            importPeppyButton.Clicked += Osz.OnImportButtonClick;
            UpdateReady = true;
        }

        public void LoadContent()
        {
            
        }

        public void UnloadContent()
        {
            UpdateReady = false;
            testButton.Clicked -= ButtonClick;
            Boundary.Destroy();
        }

        public void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Select and play random maps.
            //MenuAudioPlayer.PlayRandomBeatmaps();

            //Update Menu Screen Boundary
            Boundary.Update(dt);
        }

        public void Draw()
        {
            testButton.Draw();
            importPeppyButton.Draw();
        }

        //TODO: Remove. Test function.
        public void ButtonClick(object sender, EventArgs e)
        {
            // Stop the selected song since it's only played during the main menu.
            //GameBase.SelectedBeatmap.Song.Stop();

            //Change to SongSelectState
            GameStateManager.Instance.AddState(new SongSelectState());
        }
    }
}
