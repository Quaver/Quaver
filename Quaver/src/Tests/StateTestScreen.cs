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
using Quaver.Main;

namespace Quaver.Tests
{
    /// <summary>
    /// This is the GameState when the player is actively playing.
    /// </summary>
    internal class StateTestScreen : GameStateBase
    {
        //TEST (These variables will be removed later)
        private Texture2D _TestImage;
        private double fpsPos;
        private double pos;
        private int fpsCounter = 0;
        private Rectangle Boundary;
        private List<Sprite> spriteList;
        private List<Vector2> rand;
        private int iterations = 234;

        public StateTestScreen()
        {
            //Important to assign a state to this class.
            CurrentState = State.TestScreen;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Initialize()
        {
            //TEMP Declare temp variables
            int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Console.WriteLine("[STATE_GAMEPLAY]: Initialized Gameplay State.");
            Console.WriteLine("Screen Height: {0}, Screen Width: {1}",width,height);
            Boundary = new Rectangle(0, 0, 800,480);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void LoadContent()
        {
            //TEMP Create Sprite
            _TestImage = GameBase.Content.Load<Texture2D>("TestImages/arpiapic");
            spriteList = new List<Sprite>();
            rand = new List<Vector2>();
            for (int i = 0; i < iterations; i++)
            {
                Sprite testSprite = new Sprite();
                testSprite.Image = _TestImage;
                testSprite.Size = Vector2.One * 50f;
                testSprite.Alignment = Alignment.MidCenter;
                spriteList.Add(testSprite);

                for (int j = 0; j  < 5; j++)
                {
                    Sprite testChild = new Sprite();
                    testChild.Image = _TestImage;
                    testChild.Size = Vector2.One * 20f;
                    testChild.Alignment = Alignment.MidCenter;
                    testChild.Parent = testSprite;
                }

                Vector2 random = new Vector2(Util.Random(-100f, 100f), Util.Random(-100f, 100f));
                rand.Add(random);
            }
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
            //Update pos of the objects (Temporary
            pos += (double)(gameTime.ElapsedGameTime.TotalSeconds);
            if (pos > Math.PI * 2)
            {
                pos -= Math.PI * 2;
            }

            //FPS COUNTER (Temporary)
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
        public override void Draw()
        {
            //GraphicsDevice.Clear(Color.Black);
            GameBase.SpriteBatch.Begin();

            //Draw stuff here
            for (int i = 0; i < spriteList.Count; i ++)
            {
                float interval = ((float)i / iterations) * (float)Math.PI * 2f;
                spriteList[i].Position = new Vector2((float)Math.Cos(pos + interval) * 100f + rand[i].X, (float)Math.Sin(pos + interval) * 100f + rand[i].Y);

                for (int j = 0; j < spriteList[i].Children.Count; j++)
                {
                    float childinterval = ((float)j / spriteList[i].Children.Count) * (float)Math.PI * 2f;
                    spriteList[i].Children[j].Position = new Vector2((float)Math.Cos((pos + childinterval)*3f) * 25f, (float)Math.Sin((pos + childinterval) * 3f) * 25f);
                }

                spriteList[i].Draw();
            }

            //End
            GameBase.SpriteBatch.End();
        }
    }
}
