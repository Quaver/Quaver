#define WINDOWS_STOREAPP
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using ManagedBass;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Commands;
using Quaver.Config;
using Quaver.Database;
using Quaver.Database.Beatmaps;
using Quaver.GameState;
using Quaver.Logging;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Input
{
    internal class GlobalInputManager : IInputManager
    {
        /// <summary>
        ///     The current state for the specifc input manager
        /// </summary>
        public State CurrentState { get; set; } // Global State, so this isn't necessary.

        /// <summary>
        ///     Keeps track of the last scroll wheel value.
        /// </summary>
        private int LastScrollWheelValue { get; set; }

        /// <summary>
        ///     Keeps track of if the user is currently taking a screenshot.
        /// </summary>
        private bool CurrentlyTakingScreenshot { get; set; }

        /// <summary>
        ///     Global click event
        /// </summary>
        public event EventHandler LeftClicked;

        /// <summary>
        ///     Gets triggered everytime GameOverlay key gets pressed
        /// </summary>
        public event EventHandler GameOverlayToggled;

        /// <summary>
        ///     Is determined by whether left mouse button is down or not.
        /// </summary>
        private bool LeftMouseButtonIsDown { get; set; }

        /// <summary>
        ///     Is determined by whether game overlay button is down or not.
        /// </summary>
        private bool GameOverlayButtonIsDown { get; set; }

        /// <summary>
        ///     Check the input.
        /// </summary>
        public void CheckInput()
        {
            HandleVolumeChanges();
            ImportBeatmaps();
            TakeScreenshot();
            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void HandleMouseInput()
        {
            if (LeftMouseButtonIsDown)
            {
                if (GameBase.MouseState.LeftButton == ButtonState.Released)
                {
                    LeftMouseButtonIsDown = false;
                }
            }
            else
            {
                if (GameBase.MouseState.LeftButton == ButtonState.Pressed)
                {
                    LeftMouseButtonIsDown = true;
                    LeftClicked?.Invoke(this, null);
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (GameOverlayButtonIsDown)
            {
                if (GameBase.KeyboardState.IsKeyUp(ConfigManager.KeyToggleOverlay))
                {
                    GameOverlayButtonIsDown = false;
                }
            }
            else
            {
                if (GameBase.KeyboardState.IsKeyDown(ConfigManager.KeyToggleOverlay))
                {
                    GameOverlayButtonIsDown = true;
                    GameOverlayToggled?.Invoke(this, null);
                }
            }
        }

        /// <summary>
        ///     Handles all global volume changes.
        ///     For this to be activated, the user must be holding down either ALT key while they are scrolling the mouse.
        /// </summary>
        private void HandleVolumeChanges()
        {
            //  Raise volume if the user scrolls up.
            if (GameBase.MouseState.ScrollWheelValue > LastScrollWheelValue 
                && (GameBase.KeyboardState.IsKeyDown(Keys.RightAlt) || GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt)) 
                && Config.ConfigManager.VolumeGlobal < 100)
            {
                Config.ConfigManager.VolumeGlobal += 5;

                // Set the last scroll wheel value
                LastScrollWheelValue = GameBase.MouseState.ScrollWheelValue;

                // Change the master volume based on the new config value.
                GameBase.AudioEngine.MasterVolume = ConfigManager.VolumeGlobal;
                Logger.LogInfo($"VolumeGlobal Changed To: {ConfigManager.VolumeGlobal}", LogType.Runtime);
            }
            // Lower volume if the user scrolls down
            else if (GameBase.MouseState.ScrollWheelValue < LastScrollWheelValue 
                && (GameBase.KeyboardState.IsKeyDown(Keys.RightAlt) || GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt)) 
                && ConfigManager.VolumeGlobal > 0)
            {
                ConfigManager.VolumeGlobal -= 5;

                // Set the last scroll wheel value
                LastScrollWheelValue = GameBase.MouseState.ScrollWheelValue;

                // Change the master volume based on the new config value.
                GameBase.AudioEngine.MasterVolume = ConfigManager.VolumeGlobal;
                Logger.LogInfo($"VolumeGlobal Changed To: {ConfigManager.VolumeGlobal}", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Checks if the beatmap import queue is ready, and imports then if the user decides to.
        /// </summary>
        private void ImportBeatmaps()
        {
            // TODO: This is a beatmap import and sync test, eventually add this to its own game state
            if (GameBase.KeyboardState.IsKeyDown(Keys.F5) && GameBase.ImportQueueReady)
            {
                GameBase.ImportQueueReady = false;

                // Asynchronously load and set the GameBase beatmaps and visible ones.
                Task.Run(async () =>
                {
                    await BeatmapCache.LoadAndSetBeatmaps();
                    GameBase.VisibleMapsets = GameBase.Mapsets;
                });
            }
        }

        /// <summary>
        ///     Handles the taking of screenshots and saving them.
        /// </summary>
        private void TakeScreenshot()
        {
            if (GameBase.KeyboardState.IsKeyUp(ConfigManager.KeyTakeScreenshot))
                CurrentlyTakingScreenshot = false;

            // Prevent spamming. Don't run if we're already taking a screenshot.
            if (CurrentlyTakingScreenshot)
                return;

            if (!GameBase.KeyboardState.IsKeyDown(ConfigManager.KeyTakeScreenshot))
                return;

            CurrentlyTakingScreenshot = true;

            // Play screenshot sound effect
            GameBase.LoadedSkin.SoundScreenshot.Play(GameBase.SoundEffectVolume, 0, 0);

            // Create path for file
            var path = Config.ConfigManager.ScreenshotDirectory + "/" + DateTime.Now.ToString("yyyy-MM-dd HHmmssfff") + ".jpg";

            // Get Window Bounds
            var bounds = GameBase.GraphicsDevice.PresentationParameters.Bounds;

            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var point = new Point(GameBase.GameWindow.Position.X, GameBase.GameWindow.Position.Y);
                    g.CopyFromScreen(point, Point.Empty, new Size(bounds.Width, bounds.Height));
                }

                // Save the screenshot
                bitmap.Save(path, ImageFormat.Jpeg);
                Logger.LogSuccess($"Screenshot taken. Saved at: {path}", LogType.Runtime);
            }
        }
    }
}
