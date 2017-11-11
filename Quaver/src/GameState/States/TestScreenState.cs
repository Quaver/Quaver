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

namespace Quaver.GameState.States
{
    /// <summary>
    /// This is the GameState when the player is actively playing.
    /// </summary>
    internal class StateTestScreen : IGameState
    {
        public State CurrentState { get; set; } = State.TestScreen;
        public bool UpdateReady { get; set; }

        //TEST (These variables will be removed later)
        private Texture2D _TestImage;
        private double _pos;
        private Boundary _testBoundary;
        private List<Sprite> _spriteList;
        private List<Vector2> _rand;
        private int _iterations = 50;
        private int _totalChildren = 50;

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            //TEMP Declare temp variables
            int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Console.WriteLine("[STATE_TESTSCREEN]: Initialized Test State.");
            Console.WriteLine("Screen Height: {0}, Screen Width: {1}",width,height);
            Console.WriteLine("Total Test Objects: {0}", _iterations * _totalChildren);

            UpdateReady = true;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void LoadContent()
        {
            //TEMP Create Sprite
            _testBoundary = new Boundary();
            _TestImage = GameBase.Content.Load<Texture2D>("note-hitobject1");
            _spriteList = new List<Sprite>();
            _rand = new List<Vector2>();
            
            for (int i = 0; i < _iterations; i++)
            {
                Sprite testSprite = new Sprite();
                testSprite.Image = _TestImage;
                testSprite.Size = Vector2.One * 50f;
                testSprite.Alignment = Alignment.MidCenter;
                testSprite.Parent = _testBoundary;
                _spriteList.Add(testSprite);

                for (int j = 0; j  < _totalChildren; j++)
                {
                    Sprite testChild = new Sprite();
                    testChild.Image = _TestImage;
                    testChild.Size = Vector2.One * 20f;
                    testChild.Alignment = Alignment.MidCenter;
                    testChild.Parent = _spriteList[i];
                }

                Vector2 random = new Vector2(Util.Random(-300f, 300f), Util.Random(-300f, 300f));
                _rand.Add(random);
            }
            
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(GameTime gameTime)
        {
            //Update pos of the objects (Temporary
            _pos += (double)(gameTime.ElapsedGameTime.TotalSeconds);
            if (_pos > Math.PI * 2)
            {
                _pos -= Math.PI * 2;
            }
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            //Draw stuff here
            for (int i = 0; i < _iterations; i ++)
            {
                float interval = ((float)i / _iterations) * (float)Math.PI * 2f;
                _spriteList[i].Position = new Vector2((float)Math.Cos(_pos + interval) * 100f + _rand[i].X, (float)Math.Sin(_pos + interval) * 100f + _rand[i].Y);

                for (int j = 0; j < _totalChildren; j++)
                {
                    float childinterval = ((float)j / _spriteList[i].Children.Count) * (float)Math.PI * 2f;
                    _spriteList[i].Children[j].Position = new Vector2((float)Math.Cos((_pos + childinterval)*3f) * 25f, (float)Math.Sin((_pos + childinterval) * 3f) * 25f);
                }
            }
            _testBoundary.Draw();
        }
    }
}
