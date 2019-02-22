using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Toolkit
{
    public class EditorCompositionToolbox : Sprite
    {
        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorCompositionToolbox()
        {
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorCompositionToolsPanel;

            CreateHeader();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            HeaderBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 38),
                Tint = Color.Transparent,
            };
        }
    }
}