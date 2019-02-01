using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorDrawableLayer : PoolableSprite<int>
    {
        /// <summary>
        /// </summary>
        private EditorLayerCompositor LayerCompositor { get; }

        /// <summary>
        /// </summary>
        private EditorLayerVisiblityCheckbox VisibilityCheckbox { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton EditLayerNameButton { get; set; }

        /// <summary>
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap LayerName { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 40;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="layerCompositor"></param>
        /// <param name="name"></param>
        public EditorDrawableLayer(EditorLayerCompositor layerCompositor, string name)
        {
            LayerCompositor = layerCompositor;
            Name = name;
            Tint = Color.White;
            Alpha = 0.45f;

            Size = new ScalableVector2(LayerCompositor.Width, HEIGHT);

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

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(int item, int index)
        {
        }
    }
}