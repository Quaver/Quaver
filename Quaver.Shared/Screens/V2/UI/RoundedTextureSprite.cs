using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Draws an image through the rounded rectangle shader while preserving scissor clipping
    /// </summary>
    internal class RoundedTextureSprite : Sprite
    {
        private float CornerRadius { get; }
        private Vector2 ShaderSize { get; set; }
        private SpriteBatchOptions PlainScissorOptions { get; } = RoundedRectShader.CreateScissorSafeOptions();

        public RoundedTextureSprite(float width, float height, float cornerRadius, Texture2D image)
        {
            CornerRadius = cornerRadius;
            Size = new ScalableVector2(width, height);
            SetSource(image);
        }

        public void SetSource(Texture2D source)
        {
            if (source != null && !source.IsDisposed)
                Image = source;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible || IsDisposed)
                return;

            if (SpriteBatchOptions == null)
            {
                GameBase.Game.TryEndBatch();
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    RasterizerState = RoundedRectShader.ScissorSafeRasterizerState,
                    Shader = RoundedRectShader.Create(CornerRadius)
                };
            }

            var size = new Vector2(Width, Height);
            if (ShaderSize != size)
            {
                RoundedRectShader.UpdateSize(SpriteBatchOptions.Shader, size);
                ShaderSize = size;
            }

            base.Draw(gameTime);
            PlainScissorOptions.Begin();
        }
    }
}
