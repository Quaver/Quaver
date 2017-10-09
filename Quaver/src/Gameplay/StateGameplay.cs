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
        //TEST (These variables will be removed later)
        private Texture2D _TestImage;
        private double fpsPos;
        private double pos;
        private int fpsCounter = 0;
        private Rectangle Boundary;
        private List<Sprite> spriteList;
        private List<Vector2> rand;
        private int iterations = 24;
        private Color curColor = new Color(Util.Random(0, 1), Util.Random(0, 1), Util.Random(0, 1), 1);

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
        public override void LoadContent(ContentManager content)
        {
            //TEMP Create Sprite
            _TestImage = content.Load<Texture2D>("TestImages/arpiapic");
            spriteList = new List<Sprite>();
            rand = new List<Vector2>();
            for (int i = 0; i < iterations; i++)
            {
                Sprite testSprite = new Sprite(GraphicsDevice);
                testSprite.Image = _TestImage;
                testSprite.Size = Vector2.One * 50f;
                spriteList.Add(testSprite);

                for (int j = 0; j  < 5; j++)
                {
                    Sprite testChild = new Sprite(GraphicsDevice);
                    testChild.Image = _TestImage;
                    testChild.Size = Vector2.One * 20f;
                    testChild.Parent = testSprite;
                }

                Vector2 random = new Vector2(Util.Random(-150f, 150f), Util.Random(-150f, 150f));
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
                curColor = new Color(Util.Random(0, 1), Util.Random(0, 1), Util.Random(0, 1), 1);
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
        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(curColor);
            //GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            //Draw stuff here
            for (int i = 0; i < spriteList.Count; i ++)
            {
                float interval = ((float)i / iterations) * (float)Math.PI * 2f;
                spriteList[i].LocalRect = Util.DrawRect(Alignment.MidCenter, spriteList[i].Size, Boundary, new Vector2((float)Math.Cos(pos + interval) * 200f + rand[i].X, (float)Math.Sin(pos + interval) * 200f + rand[i].Y));

                for (int j = 0; j < spriteList[i].Children.Count; j++)
                {
                    float childinterval = ((float)j / spriteList[i].Children.Count) * (float)Math.PI * 2f;
                    spriteList[i].Children[j].Position = new Vector2((float)Math.Cos((pos + childinterval)*3f) * 50f, (float)Math.Sin((pos + childinterval) * 3f) * 50f);
                }

                spriteList[i].Draw(spriteBatch);
            }

            //End
            spriteBatch.End();
        }
    }
}
