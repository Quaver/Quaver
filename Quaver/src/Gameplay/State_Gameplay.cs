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

namespace Quaver.Gameplay
{
    internal partial class State_Gameplay : GameStateBase
    {

        //TEST
        private Texture2D _TestImage;
        private double fpsPos;
        private double pos;
        private int fpsCounter = 0;

        public State_Gameplay(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            //Important to assign a state to this class.
            _currentState = State.PlayScreen;
        }
        public override void Initialize()
        {
            int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Console.WriteLine("[STATE_GAMEPLAY]: Initialized Gameplay State.");
            Console.WriteLine("Screen Height: {0}, Screen Width: {1}",width,height);
        }

        public override void LoadContent(ContentManager content)
        {

            //TEST
            _TestImage = content.Load<Texture2D>("TestImages/arpiapic");
        }

        public override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            GameStateManager.Instance.UnloadContent();
        }

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

        public override void Draw(SpriteBatch spriteBatch, Vector2 WindowSize)
        {
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw sprites here
            //Optimize Later
            int iterations = 50;
            for (int i = 0; i < iterations; i ++)
            {
                float interval = ((float)i / iterations) * (float)Math.PI * 2f;
                spriteBatch.Draw(
                    _TestImage,
                        /*new Rectangle(
                        (int)(Util.Align(0.5f, 100, new Vector2(0, WindowSize.X)) + Math.Sin(pos + interval) * 200f),
                        (int)(Util.Align(0.5f, 100, new Vector2(0, WindowSize.Y)) + Math.Cos(pos + interval) * 200f),
                        100, 100)*/
                        Util.DrawRect(Alignment.MidCenter,new Vector2(100,100),new Rectangle(0,0,(int)WindowSize.X,(int)WindowSize.Y),new Vector2((float)Math.Cos(pos+interval)*200f, (float)Math.Sin(pos + interval) *200f)),
                    Color.White);
            }

            //End
            spriteBatch.End();
        }

    }
}
