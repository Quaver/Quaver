using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorDrawableLayer : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorLayerer Layerer { get; }

        /// <summary>
        /// </summary>
        private EditorLayerVisiblityCheckbox VisibilityCheckbox { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton EditLayerNameButton { get; set; }

        /// <summary>
        /// </summary>
        private string Name { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap LayerName { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="layerer"></param>
        /// <param name="name"></param>
        public EditorDrawableLayer(EditorLayerer layerer, string name)
        {
            Layerer = layerer;
            Name = name;
            Tint = Color.White;
            Alpha = 0.45f;

            Size = new ScalableVector2(Layerer.Width, 40);

            CreateVisibilityCheckbox();
            CreateEditNamePencil();
            CreateLayerName();
        }

        /// <summary>
        /// </summary>
        private void CreateVisibilityCheckbox() => VisibilityCheckbox = new EditorLayerVisiblityCheckbox(true)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 12,
            Size = new ScalableVector2(16, 16),
        };

        /// <summary>
        /// </summary>
        private void CreateEditNamePencil() => EditLayerNameButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil))
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = VisibilityCheckbox.X + VisibilityCheckbox.Width + 10,
            Size = VisibilityCheckbox.Size
        };

        /// <summary>
        /// </summary>
        private void CreateLayerName() => LayerName = new SpriteTextBitmap(FontsBitmap.AllerRegular, Name)
        {
            Parent = this,
            FontSize = 16,
            Alignment = Alignment.MidLeft,
            X = EditLayerNameButton.X + EditLayerNameButton.Width + 10
        };
    }
}