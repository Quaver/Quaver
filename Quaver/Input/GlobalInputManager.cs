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
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States;
using Quaver.States.Enums;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Input
{
    internal class GlobalInputManager : IInputManager
    {
        /// <summary>
        ///     The current state for the specifc input manager
        ///     Global State, so this isn't necessary.
        /// </summary>
        public State CurrentState { get; set; } 

        /// <summary>
        ///     Keeps track of if the user is currently taking a screenshot.
        /// </summary>
        private bool CurrentlyTakingScreenshot { get; set; }

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
            ImportMapsets();
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
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (GameOverlayButtonIsDown)
            {
                if (GameBase.KeyboardState.IsKeyUp(ConfigManager.KeyToggleOverlay.Value))
                {
                    GameOverlayButtonIsDown = false;
                }
            }
            else
            {
                if (GameBase.KeyboardState.IsKeyDown(ConfigManager.KeyToggleOverlay.Value))
                {
                    GameOverlayButtonIsDown = true;
                    GameOverlayToggled?.Invoke(this, null);
                }
            }
        }

        /// <summary>
        ///     Checks if the map import queue is ready, and imports then if the user decides to.
        /// </summary>
        private void ImportMapsets()
        {
            // TODO: This is a map import and sync test, eventually add this to its own game state
            if (GameBase.KeyboardState.IsKeyDown(Keys.F5) && GameBase.ImportQueueReady)
            {
                GameBase.ImportQueueReady = false;

                // Asynchronously load and set the GameBase mapsets and visible ones.
                Task.Run(async () =>
                {
                    await MapCache.LoadAndSetMapsets();
                    GameBase.VisibleMapsets = GameBase.Mapsets;
                });
            }
        }

        /// <summary>
        ///     Handles the taking of screenshots and saving them.
        /// </summary>
        private void TakeScreenshot()
        {
            if (GameBase.KeyboardState.IsKeyUp(ConfigManager.KeyTakeScreenshot.Value))
                CurrentlyTakingScreenshot = false;

            // Prevent spamming. Don't run if we're already taking a screenshot.
            if (CurrentlyTakingScreenshot)
                return;

            if (!GameBase.KeyboardState.IsKeyDown(ConfigManager.KeyTakeScreenshot.Value))
                return;

            CurrentlyTakingScreenshot = true;

            // Play screenshot sound effect
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundScreenshot);

            // Create path for file
            var path = ConfigManager.ScreenshotDirectory + "/" + DateTime.Now.ToString("yyyy-MM-dd HHmmssfff") + ".jpg";

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
