using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState.Gameplay;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Peppy;
using Quaver.QuaFile;

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
        ///     Button to import .qp
        /// </summary>
        public Button ImportQpButton { get; set; }

        /// <summary>
        ///     Button to export .qp
        /// </summary>
        public Button ExportQpButton { get; set; }

        /// <summary>
        ///     Button to import .osz
        /// </summary>
        public Button ImportPeppyButton { get; set; }

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            // Remove speed mods upon going to the main menu so songs can be played at normal speed.
            ModManager.RemoveSpeedMods();

            // Initialize the main menu's audio player.
            MenuAudioPlayer.Initialize();

            //Initialize Menu Screen
            Boundary = new Boundary();

            // Initialize the UI buttons
            CreateOszImportButton();
            CreateQpImportButton();
            CreateSongSelectButton();
            CreateQpExportButton();

            UpdateReady = true;
        }

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
        public void Update(double dt)
        {
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
            ImportQpButton.Draw();
            ExportQpButton.Draw();
        }

        /// <summary>
        ///     Responsible for creating the button to move to the song select screen state
        /// </summary>
        private void CreateSongSelectButton()
        {
            // Switch Song Select Button
            SwitchSongSelectButton = new TextButton(new Vector2(200, 40), "Song Select")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.MidCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };

            SwitchSongSelectButton.Clicked += OnSongSelectButtonClick;
        }

        /// <summary>
        ///     The event handler that switches to the song select screen
        /// </summary>
        public void OnSongSelectButtonClick(object sender, EventArgs e)
        {
            //Change to SongSelectState
            GameBase.LoadedSkin.Click.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Responsible for creating the import .osz button
        /// </summary>
        private void CreateOszImportButton()
        {
            // Import .osz Button
            ImportPeppyButton = new TextButton(new Vector2(200, 40), "Import .osz")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.TopCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };

            ImportPeppyButton.Clicked += Osz.OnImportButtonClick;
        }


        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpImportButton()
        {
            // Import .osz Button
            ImportQpButton = new TextButton(new Vector2(200, 40), "Import Quaver Mapset")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.BotCenter,
                Position = Vector2.Zero,
                Parent = Boundary
            };

            ImportQpButton.Clicked += Qp.OnImportButtonClick;
        }

        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpExportButton()
        {
            // Import .osz Button
            ExportQpButton = new TextButton(new Vector2(200, 40), "Export Current Mapset")
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.BotRight,
                Position = Vector2.Zero,
                Parent = Boundary
            };

            ExportQpButton.Clicked += Qp.OnExportButtonClick;
        }
    }
}
