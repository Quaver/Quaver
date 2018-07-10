using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.Resources;
using Quaver.Shaders;

namespace Quaver.States.Tests
{
    internal class SemiTransparentTestScreen : IGameState
    {
        public State CurrentState { get; set; } = State.Test;

        public bool UpdateReady { get; set; }

        public Container Container { get; set; }
        public Sprite Test { get; set; }

        /// <summary>
        ///     Shader to make a texture partially transparent with a rectangle mask.
        ///
        ///     Parameters:
        ///         `p_dimensions`: Vector2 - Width and Height of the texture (Vector2)
        ///         `p_position`: Vector2 - The position where to make it transparent. (Vector2)
        ///         `p_rectangle`: Vector2 - Width and height of the transparent rectangle.
        ///         `p_alpha`: Float - Value from 0-1 that dictates how transparent the rectangle will be.
        /// </summary>
        public Effect SemiTransparent { get; set; }

        public void Initialize()
        {
            UpdateReady = true;

            Container = new Container();

            Test = new Sprite
            {
                Parent = Container,
                Tint = Color.Black,
                Position = new UDim2D(0, 0),
                Alignment = Alignment.MidCenter,
                Size = new UDim2D(500, 30)
            };

            SemiTransparent = ResourceHelper.LoadShader(ShaderStore.semi_transparent);
        }

        public void UnloadContent()
        {
            Container.Destroy();
        }

        public void Update(double dt)
        {
            if (GameBase.KeyboardState.IsKeyDown(Keys.Right))
            {
                var currentVal = SemiTransparent.Parameters["p_rectangle"].GetValueVector2();
                var newWidth = MathHelper.Lerp(currentVal.X, Test.SizeX, (float)Math.Min(dt / 120, 1));

                SemiTransparent.Parameters["p_rectangle"].SetValue(new Vector2(newWidth, Test.SizeY));
            }
            if (GameBase.KeyboardState.IsKeyDown(Keys.Left))
            {
                var currentVal = SemiTransparent.Parameters["p_rectangle"].GetValueVector2();
                var newWidth = MathHelper.Lerp(currentVal.X, 0, (float)Math.Min(dt / 120, 1));

                SemiTransparent.Parameters["p_rectangle"].SetValue(new Vector2(newWidth, Test.SizeY));
            }

            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.MediumPurple);

            SemiTransparent.Parameters["p_dimensions"].SetValue(new Vector2(Test.SizeX, Test.SizeY));
            SemiTransparent.Parameters["p_position"].SetValue(new Vector2(0f, 0f));
            SemiTransparent.Parameters["p_alpha"].SetValue(0f);

            GameBase.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, SemiTransparent);

            Container.Draw();
            GameBase.SpriteBatch.End();
        }
    }
}