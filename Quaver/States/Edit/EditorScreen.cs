using System;
using Quaver.API.Maps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;

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
        ///     
        /// </summary>
        /// <param name="map"></param>
        public EditorScreen(Qua map)
        {
            if (GameBase.AudioEngine.IsPlaying)
                GameBase.AudioEngine.Pause();
            
            Map = map;
            LastSavedMap = ObjectHelper.DeepClone(Map);

            DiscordManager.Client.CurrentPresence.Details = Map.ToString();
            DiscordManager.Client.CurrentPresence.State = "Editing Map";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            GameBase.SpriteBatch.End();
        }
    }
}