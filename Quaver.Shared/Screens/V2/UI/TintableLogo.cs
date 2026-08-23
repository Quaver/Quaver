using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Draws an unchanged logo base with one or more colorized accent masks over it.
    /// </summary>
    internal sealed class TintableLogo : Sprite
    {
        private List<Sprite> Layers { get; } = new List<Sprite>();

        public TintableLogo(Texture2D baseTexture, Color accentColor, params Texture2D[] accentTextures)
            : this(baseTexture)
        {
            foreach (var texture in accentTextures)
                AddLayer(texture, accentColor);
        }

        public TintableLogo(Texture2D baseTexture, Color accentColor,
            IEnumerable<Texture2D> unchangedTextures, IEnumerable<Texture2D> accentTextures)
            : this(baseTexture)
        {
            foreach (var texture in unchangedTextures)
                AddLayer(texture, Color.White);
            foreach (var texture in accentTextures)
                AddLayer(texture, accentColor);
        }

        public TintableLogo(Texture2D baseTexture, Texture2D underlayTexture, Color underlayColor,
            Texture2D accentTexture, Color accentColor)
            : this(baseTexture)
        {
            AddLayer(underlayTexture, underlayColor);
            AddLayer(accentTexture, accentColor);
        }

        private TintableLogo(Texture2D baseTexture)
        {
            Image = baseTexture;
            SetChildrenAlpha = true;
            Size = new ScalableVector2(baseTexture.Width, baseTexture.Height);
            SizeChanged += (sender, size) => ResizeLayers(size);
        }

        private void AddLayer(Texture2D texture, Color tint)
        {
            Layers.Add(new Sprite
            {
                Parent = this,
                Image = texture,
                Size = new ScalableVector2(texture.Width, texture.Height),
                Tint = tint,
                UsePreviousSpriteBatchOptions = true
            });
        }

        private void ResizeLayers(ScalableVector2 size)
        {
            foreach (var layer in Layers)
            {
                var widthRatio = (float) layer.ImageWidth / ImageWidth;
                var heightRatio = (float) layer.ImageHeight / ImageHeight;
                layer.Size = new ScalableVector2(
                    size.X.Value * widthRatio,
                    size.Y.Value * heightRatio,
                    size.X.Scale * widthRatio,
                    size.Y.Scale * heightRatio);
            }
        }
    }
}
