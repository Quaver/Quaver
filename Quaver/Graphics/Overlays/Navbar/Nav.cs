using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Replays;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Overlays.Options;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Peppy;
using Quaver.States;
using Quaver.States.Menu;
using Quaver.States.Options;
using Quaver.States.Results;
using Quaver.States.Select;
using Quaver.States.Tests;
using Quaver.StepMania;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Graphics.Overlays.Navbar
{
    /// <summary>
    ///     A navbar overlay
    /// </summary>
    internal class Nav : IGameStateComponent
    {
        /// <summary>
        ///     The actual navbar sprite
        /// </summary>
        internal Sprite NavbarSprite { get; set; }
            
        /// <summary>
        ///     The tooltip box that appears when hovering over a button.
        /// </summary>
        internal TooltipBox TooltipBox { get; set; }

        /// <summary>
        ///     The navbar buttons that are currently implemented on this navbar with their assigned
        ///     alignments to the navbar.
        /// </summary>
        internal Dictionary<NavbarAlignment, List<NavbarButton>> Buttons { get; private set; }

        /// <summary>
        ///     The height of the navbar.
        /// </summary>
        internal static int Height { get; } = 40;

        /// <summary>
        ///     The container for the navbar
        /// </summary>
        private Container Container { get; set; }
                
        /// <summary>
        ///     If the navbar is shown
        /// </summary>
        private bool IsShown { get; set; }

        /// <summary>
        ///     If the navbar is currently in an animation
        /// </summary>
        private bool InAnimation { get; set; }

        /// <summary>
        ///     Class reference to all our of navbar buttons.
        /// </summary>
        private NavbarButton Settings { get; set; }
        private NavbarButton Home { get; set; }
        private NavbarButton Play { get; set; }
        private NavbarButton Import { get; set; }
        private NavbarButton Notifications { get; set; }
        private NavbarButton Discord { get; set; }
        private NavbarButton Github { get; set; }
        private NavbarButton Export { get; set; }
        private NavbarButton Replay { get; set; }

        /// <summary>
        ///     The options menu attached to this navbar.
        /// </summary>
        internal OptionsOverlay OptionsMenu { get; set; }

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new Container();
            
            // Create the options menu.
            OptionsMenu = new OptionsOverlay();
            
            // Setup the dictionary of navbar buttons.
            Buttons = new Dictionary<NavbarAlignment, List<NavbarButton>>()
            {
                { NavbarAlignment.Left, new List<NavbarButton>() },
                { NavbarAlignment.Right, new List<NavbarButton>() }
            };
            
            // Create navbar
            NavbarSprite = new Sprite()
            {
                Size = new UDim2D(0, Height, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0f, 0f, 0f, 0.40f),
                Parent = Container
            };

            // Create the tooltip box.
            TooltipBox = new TooltipBox(Container, NavbarSprite);

#region nav_buttons 
            // Create the actual navbar buttons.
            // Note: The order in which you create the buttons is important.
            // When aligning left, the buttons will be ordered from left to right in the order they 
            // were created, and vice versa.
            // --------
            
            // Left Side 
            Settings = CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Cog, "Settings", "Configure Quaver.", OnSettingsButtonClicked);
            Home = CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Home, "Home", "Go to the main menu.", OnHomeButtonClicked);         
            Play = CreateNavbarButton(NavbarAlignment.Left, FontAwesome.GamePad, "Play", "Smash some keys!", OnPlayButtonClicked);
            Import = CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Copy, "Import Mapsets","Add new songs to play!", OnImportButtonClicked);
            Export = CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Archive, "Export Mapset", "Zip your current mapset to a file.", OnExportButtonClicked);
            Replay = CreateNavbarButton(NavbarAlignment.Left, FontAwesome.VideoPlay, "Watch Replay", "Load up a replay to watch.", OnReplayButtonClicked);
            
            // Right Side
            Notifications = CreateNavbarButton(NavbarAlignment.Right, FontAwesome.Exclamation, "Notifications", "Filler chicken", (sender, args) => { GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick); Logger.LogImportant("This button does nothing. Don't click it.", LogType.Runtime);});
            Discord = CreateNavbarButton(NavbarAlignment.Right, FontAwesome.Discord, "Discord", "https://discord.gg/nJa8VFr", OnDiscordButtonClicked);
            Github = CreateNavbarButton(NavbarAlignment.Right, FontAwesome.Github, "GitHub", "Contribute to the project!", OnGithubButtonClicked);
            
            // Test States.
            CreateNavbarButton(NavbarAlignment.Right, FontAwesome.Coffee, "Test State", "Go to testing", (sender, args) => GameBase.GameStateManager.ChangeState(new SemiTransparentTestScreen()));
#endregion
        }

         /// <summary>
        ///     Unload
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void UnloadContent()
        {
            OptionsMenu.Destroy();
            Container.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            if (GameBase.KeyboardState.IsKeyDown(Keys.Z))
                PerformHideAnimation(dt);
            
            if (GameBase.KeyboardState.IsKeyDown(Keys.X))
                PerformShowAnimation(dt);

            if (GameBase.KeyboardState.IsKeyDown(Keys.Escape))
                OptionsMenu.Active = false;
            
            OptionsMenu.Update(dt);
            Container.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            OptionsMenu.Draw();
            Container.Draw();
        }

        /// <summary>
        ///     Adds a button to the navbar with the correct alignment.
        ///     - USE THIS WHEN ADDING NAVBAR BUTTONS, as it does all the initialization for you.
        /// </summary>
        /// <param name="alignment"></param>
        /// <param name="tex"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="clickAction"></param>
        private NavbarButton CreateNavbarButton(NavbarAlignment alignment, Texture2D tex, string name, string description, EventHandler clickAction)
        {       
            var button = new NavbarButton(this, tex, alignment, name, description, clickAction) { Parent = Container };   
            Buttons[alignment].Add(button);
            return button;
        }

        /// <summary>
        ///     Peforms an animation which hides the navbar.
        /// </summary>
        /// <param name="dt"></param>
        internal void PerformHideAnimation(double dt)
        {
            // The position in which the navbar is considered hidden.
            const float hiddenPos = -50f;

            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(Container.PosY - hiddenPos) < 0.1)
            {
                IsShown = false;
                InAnimation = false;
                Container.Visible = false;
                return;
            }
            
            Container.PosY = GraphicsHelper.Tween(hiddenPos, Container.PosY, Math.Min(dt / 30, 1));
        }

         /// <summary>
        ///     Performs an animation which shows the navbar.
        /// </summary>
        /// <param name="dt"></param>
        internal void PerformShowAnimation(double dt)
         {
             // Make the container visible again when performing this animation.
             Container.Visible = true;
             
            // The original position of the navbar
            const int origPos = 0;
          
            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(Container.PosY - origPos) < 0.1)
            {
                IsShown = true;
                InAnimation = false;
                return;
            }
            
            Container.PosY = GraphicsHelper.Tween(origPos, Container.PosY, Math.Min(dt / 30, 1));
        }

        /// <summary>
        ///     Called when the home button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHomeButtonClicked(object sender, EventArgs e)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            GameBase.GameStateManager.ChangeState(new MainMenuScreen());
        }

        /// <summary>
        ///     Called when the settings button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSettingsButtonClicked(object sender, EventArgs e)
        {            
            // Open up the options overlay.
            OptionsMenu.Active = !OptionsMenu.Active;
            
            if (OptionsMenu.Active)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            else
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundBack);
        }
        
        /// <summary>
        ///     Called when the home button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayButtonClicked(object sender, EventArgs e)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Called when the Discord button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDiscordButtonClicked(object sender, EventArgs e)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            System.Diagnostics.Process.Start("https://discord.gg/nJa8VFr");
        }

        /// <summary>
        ///     Called when the GitHub button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGithubButtonClicked(object sender, EventArgs e)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            System.Diagnostics.Process.Start("https://github.com/Swan/Quaver");
        }
        
        /// <summary>
        ///     Called when the Import button is clicked
        ///     Imports mapsets to the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImportButtonClicked(object sender, EventArgs e)
        {
            // Create the openFileDialog object.
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = "c:\\",
                Filter = "Mapset (*.qp, *.osz, *.sm)| *.qp; *.osz; *.sm;",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = true
            };

            // If the dialog couldn't be shown, that's an issue, so we'll return for now.
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            // Run the converter for all selected files
            Task.Run(() =>
            {
                Logger.LogImportant($"Importing mapsets. This process runs in the background, so you can continue to play!", LogType.Runtime, 5f);
                for (var i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    if (openFileDialog.FileNames[i].EndsWith(".osz")) 
                        Osu.ConvertOsz(openFileDialog.FileNames[i], i);
                    else if (openFileDialog.FileNames[i].EndsWith(".qp"))
                        MapsetImporter.Import(openFileDialog.FileNames[i]);
                    else if (openFileDialog.FileNames[i].EndsWith(".sm"))
                        StepManiaConverter.ConvertSm(openFileDialog.FileNames[i]);
                }
                // When all the maps have been converted, select the last imported map and make that the selected one.
            }).ContinueWith(async t => await MapsetImporter.AfterImport());    
        }

        /// <summary>
        ///     Exports the currently selected map to an archive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportButtonClicked(object sender, EventArgs e)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);

            Task.Run(() =>
            {
                try
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
                
                    Console.WriteLine(outputPath);
                    System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + $@"{outputPath.Replace("/", "\\")}" + "\"");
                }
                catch (Exception ex)
                {
                    Logger.LogError("There was an issue exportng your mapset", LogType.Runtime);
                }
            });
        }

        /// <summary>
        ///     When the user is choosing to watch a replay.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReplayButtonClicked(object sender, EventArgs e)
        {
            // Create the openFileDialog object.
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = ConfigManager.GameDirectory.Value,
                Filter = "Replay (*.qr)| *.qr;",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = false
            };

            // If the dialog couldn't be shown, that's an issue, so we'll return for now.
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            
            GameBase.GameStateManager.ChangeState(new ResultsScreen(new Replay(openFileDialog.FileName)));
        }
    }
}