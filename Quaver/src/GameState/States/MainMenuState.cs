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
        /// <summary>
        ///     State
        /// </summary>
        public State CurrentState { get; set; } = State.MainMenu;

        /// <summary>
        ///     Update Ready
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Boundary
        /// </summary>
        public Boundary Boundary { get; set; }

        /// <summary>
        ///     Button to switch to the song select state
        /// </summary>
        public Button SwitchSongSelectButton { get; set; }

        /// <summary>
        ///     Button to import .osz
        /// </summary>
        public Button ImportPeppyButton { get; set; }

        public void Initialize()
        {
            // Initialize the main menu's audio player.
            MenuAudioPlayer.Initialize();

            //Initialize Menu Screen
            Boundary = new Boundary();

            // Create Buttons
            CreateButtons();

            UpdateReady = true;
        }

        /// <summary>
        ///     Load
        /// </summary>
        public void LoadContent() { }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;
            Boundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Play Random Maps during the main menu
            MenuAudioPlayer.PlayRandomBeatmaps();

            //Update Menu Screen Boundary
            Boundary.Update(dt);
        }
        
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            SwitchSongSelectButton.Draw();
            ImportPeppyButton.Draw();
        }

        //TODO: Remove. Test function.
        public void ButtonClick(object sender, EventArgs e)
        {
            // Stop the selected song since it's only played during the main menu.
            //GameBase.SelectedBeatmap.Song.Stop();

            //Change to SongSelectState
            GameStateManager.Instance.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Responsible for creating the buttons to be displayed on the screen.
        /// </summary>
        private void CreateButtons()
        {
            // Switch Song Select Button
            SwitchSongSelectButton = new TextButton(new Vector2(200, 40), "Next State")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.MidCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };

            SwitchSongSelectButton.UpdateRect();
            SwitchSongSelectButton.Clicked += ButtonClick;

            // Import .osz Button
            ImportPeppyButton = new TextButton(new Vector2(200, 40), "Import .osz")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.TopCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };

            ImportPeppyButton.UpdateRect();
            ImportPeppyButton.Clicked += Osz.OnImportButtonClick;
        }
    }
}
