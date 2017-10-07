using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;

namespace Quaver.Gameplay
{
    /// <summary>
    /// This is the GameState when the player is actively playing.
    /// </summary>
    internal class StateGameplay : GameStateBase
    {
        //TEST
        private Texture2D _TestImage;
        private double fpsPos;
        private double pos;
        private int fpsCounter = 0;
        private Rectangle Boundary;

        //State Variables

        public StateGameplay(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            //Important to assign a state to this class.
            CurrentState = State.PlayScreen;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Initialize()
        {
            //TEMP
            int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Console.WriteLine("[STATE_GAMEPLAY]: Initialized Gameplay State.");
            Console.WriteLine("Screen Height: {0}, Screen Width: {1}",width,height);
            Boundary = new Rectangle(0, 0, 800,480);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void LoadContent(ContentManager content)
        {

            //TEST
            _TestImage = content.Load<Texture2D>("TestImages/arpiapic");
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            //Console.WriteLine((double)(gameTime.ElapsedGameTime.TotalSeconds));
            pos += (double)(gameTime.ElapsedGameTime.TotalSeconds);
            fpsPos += (double)(gameTime.ElapsedGameTime.TotalSeconds);
            fpsCounter++;

            if (fpsCounter >= 100)
            {
                fpsCounter = 0;
                //Console.WriteLine(1 / (fpsPos / 100) + " FPS");
                fpsPos = 0;
            }
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw sprites here
            //Optimize Later
            int iterations = 50;
            Sprite[] spriteList = new Sprite[iterations]; //Temporary
            
            for (int i = 0; i < iterations; i ++)
            {
                float interval = ((float)i / iterations) * (float)Math.PI * 2f;
                spriteList[i] = new Sprite(GraphicsDevice);
                spriteList[i].Image = _TestImage;
                spriteList[i].Size = Vector2.One * 50f;
                spriteList[i].Rect = Util.DrawRect(Alignment.MidCenter, spriteList[i].Size, Boundary, new Vector2((float)Math.Cos(pos + interval) * 200f, (float)Math.Sin(pos + interval) * 200f));
                spriteList[i].Tint = Color.White;
                spriteList[i].Draw(spriteBatch);
            }

            //End
            spriteBatch.End();
        }
    }
}
