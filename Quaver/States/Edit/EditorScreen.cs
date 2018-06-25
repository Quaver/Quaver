using System;
using Quaver.API.Maps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Edit.UI;

namespace Quaver.States.Edit
{
    internal class EditorScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.Edit;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     The map that is currently being edited.
        /// </summary>
        private Qua Map { get; }
        
        /// <summary>
        ///     The last saved version of the map.
        /// </summary>
        private Qua LastSavedMap { get; set; }

        /// <summary>
        ///     The entire user interface for the editor.
        /// </summary>
        private EditorInterface UI { get; }

        /// <summary>
        ///     Handles all the input for the editor.
        /// </summary>
        private EditorInputManager InputManager { get; }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="map"></param>
        public EditorScreen(Qua map)
        {
            if (GameBase.AudioEngine.IsPlaying)
                GameBase.AudioEngine.Pause();
            
            Map = map;
            LastSavedMap = ObjectHelper.DeepClone(Map);
            UI = new EditorInterface(this);
            InputManager = new EditorInputManager();

            DiscordManager.Client.CurrentPresence.Details = Map.ToString();
            DiscordManager.Client.CurrentPresence.State = "Editing Map";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            UI.Initialize(this);
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
            UI.UnloadContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            InputManager.HandleInput(dt);
            UI.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            UI.Draw();
        }
    }
}