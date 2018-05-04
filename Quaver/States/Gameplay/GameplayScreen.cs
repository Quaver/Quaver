using System;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Gameplay
{
    internal class GameplayScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.PlayScreen;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     The specific audio timimg for this gameplay state.
        /// </summary>
        internal GameplayAudio AudioTiming { get; }

        /// <summary>
        ///     If the play session is finished.
        /// </summary>
        internal bool Finished { get; set; }

        /// <summary>
        ///     The current parsed .qua file that is being played.
        /// </summary>
        private Qua Map { get; }
        
        /// <summary>
        ///     The hash of the map that was played.
        /// </summary>
        private string MapHash { get; }

        /// <summary>
        ///     Keeps track of the previous start time in the delay.
        /// </summary>
        private long InitializationTime { get; set;  }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal GameplayScreen(Qua map, string md5)
        {
            Map = map;
            MapHash = md5;
            
            AudioTiming = new GameplayAudio(this);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {           
            AudioTiming.Initialize(this);
            
            // Set the delay time last, so that we can begin to start the audio track.
            InitializationTime = GameBase.GameTime.ElapsedMilliseconds;

            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
            AudioTiming.UnloadContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            AudioTiming.Update(dt);            
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.BlanchedAlmond);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            AudioTiming.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}