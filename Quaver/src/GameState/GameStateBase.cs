using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.GameState
{
    /// <summary>
    ///     Any GameState will be inheriting from this class.
    /// </summary>
    /// <inheritdoc />
    internal abstract class GameStateBase
    {
        /// <summary>
        ///     TODO: Summary goes here.
        /// </summary>
        protected GraphicsDevice GraphicsDevice;

        /// <summary>
        ///     The State of the GameState class.
        /// </summary>
        public State CurrentState { get; set; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        protected GameStateBase(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
        
        /// <summary>
        ///     TODO: Summary goes here
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        ///     TODO: Summary goes here
        /// </summary>
        /// <param name="content"></param>
        public abstract void LoadContent(ContentManager content);

        /// <summary>
        ///     TODO: Summary goes here
        /// </summary>
        public abstract void UnloadContent();

        /// <summary>
        ///     TODO: Summary goes here.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        ///     TODO: Summary goes here.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="windowSize"></param>
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}