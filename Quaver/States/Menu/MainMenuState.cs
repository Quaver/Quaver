using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Quaver.Commands;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Enums;
using Quaver.States.Options;
using Quaver.States.Select;
using Quaver.States.Tests;

namespace Quaver.States.Menu
{
    internal class MainMenuState : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        ///     State
        /// </summary>
        public State CurrentState { get; set; } = State.MainMenu;

        /// <inheritdoc />
        /// <summary>
        ///     Update Ready
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     QuaverContainer
        /// </summary>
        private QuaverContainer QuaverContainer { get; set; }

        /// <summary>
        ///     The navbar at the top of the screen.
        /// </summary>
        private Navbar Navbar { get; set; }

        /// <summary>
        ///     QuaverButton to export .qp
        /// </summary>
        private QuaverButton ExportQpQuaverButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            // Remove speed mods upon going to the main menu so songs can be played at normal speed.
            if (GameBase.CurrentGameModifiers.Count > 0)
                ModManager.RemoveSpeedMods();

            // Set Discord RP
            DiscordController.ChangeDiscordPresence("Main Menu", "In the menus");

#if DEBUG
            // Enable console commands (Only applicable if on debug release)
            CommandHandler.HandleConsoleCommand();
#endif

            //Initialize Menu Screen
            QuaverContainer = new QuaverContainer();
            CreateUI();

            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            QuaverContainer.Destroy();
            Navbar.UnloadContent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            QuaverContainer.Update(dt);
            Navbar.Update(dt);
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkSlateBlue);
            GameBase.SpriteBatch.Begin();
            
            QuaverContainer.Draw();
            Navbar.Draw();
            
            GameBase.SpriteBatch.End();
        }
        
        /// <summary>
        ///     Initializes the UI for this state
        /// </summary>
        private void CreateUI()
        {
            // Create navbar
            Navbar = new Navbar();
            Navbar.Initialize(this);
            
            // TODO: Button to export mapsets (put in navbar)
             CreateQpExportButton();   
        }
        
        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpExportButton()
        {
            // Import .osz QuaverButton
            ExportQpQuaverButton = new QuaverTextButton(new Vector2(200, 40), "Export Current Mapset")
            {
                Alignment = Alignment.BotRight,
                Parent = QuaverContainer
            };

            ExportQpQuaverButton.Clicked += OnExportButtonClick;
        }

        /// <summary>
        ///     Responsible for zipping the selected mapset
        /// </summary>
        private static void OnExportButtonClick(object sender, EventArgs e)
        {
            var zip = new ZipFile();

            // Get all the files in the current selected map's directory.
            var dirInfo = new DirectoryInfo(ConfigManager.SongDirectory + "/" + GameBase.SelectedMap.Directory + "/");
            var files = dirInfo.GetFiles();

            foreach (var file in files)
                zip.AddFile(ConfigManager.SongDirectory + "/" + GameBase.SelectedMap.Directory + "/" + file, "");

            // Create the Data/Maps directory if it doesn't exist already.
            Directory.CreateDirectory($"{ConfigManager.DataDirectory}/Maps/");

            // Save the file
            var outputPath = $"{ConfigManager.DataDirectory}/Maps/{GameBase.GameTime.ElapsedMilliseconds} {StringHelper.FileNameSafeString(GameBase.SelectedMap.Artist)} - {StringHelper.FileNameSafeString(GameBase.SelectedMap.Title)}.qp";
            zip.Save(outputPath);

            Logger.LogSuccess($"Successfully exported {outputPath}", LogType.Runtime);

            // Open the folder where the file is contained.
            if (!File.Exists(outputPath))
                return;

            // TODO: Fix for linux/mac.
            try
            {
                Console.WriteLine(outputPath);
                System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + $@"{outputPath.Replace("/", "\\")}" + "\"");
            }
            catch (Exception ex) 
            { 
                //Logger.Error(ex);
            }
        }
    }
}
