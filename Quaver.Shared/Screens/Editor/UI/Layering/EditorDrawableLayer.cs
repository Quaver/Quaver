using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorDrawableLayer : PoolableSprite<EditorLayerInfo>
    {
        /// <summary>
        /// </summary>
        public EditorLayerCompositor LayerCompositor { get; }

        /// <summary>
        /// </summary>
        private EditorLayerVisiblityCheckbox VisibilityCheckbox { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton EditLayerNameButton { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton SelectLayerButton { get; set; }

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
        /// <param name="item"></param>
        /// <param name="index"></param>
        public EditorDrawableLayer(EditorLayerCompositor layerCompositor, EditorLayerInfo item, int index) : base(item, index)
        {
            LayerCompositor = layerCompositor;
            Tint = Color.White;

            Alpha = layerCompositor.SelectedLayerIndex.Value == index ? 0.45f : 0;
            Size = new ScalableVector2(LayerCompositor.Width, HEIGHT);

            CreateVisibilityCheckbox();
            CreateEditNamePencil();
            CreateLayerName();
            CreateSelectLayerButton();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AnimateSelection(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateVisibilityCheckbox() => VisibilityCheckbox = new EditorLayerVisiblityCheckbox(this)
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
        private void CreateLayerName() => LayerName = new SpriteTextBitmap(FontsBitmap.AllerRegular, Item.Name)
        {
            Parent = this,
            FontSize = 16,
            Alignment = Alignment.MidLeft,
            X = EditLayerNameButton.X + EditLayerNameButton.Width + 10
        };

        /// <summary>
        /// </summary>
        private void CreateSelectLayerButton() => SelectLayerButton = new ImageButton(UserInterface.BlankBox,
            (sender, args) =>
            {
                LayerCompositor.SelectedLayerIndex.Value = Index;
                Logger.Debug($"Selected layer: {LayerCompositor.ScrollContainer.AvailableItems[Index].Name} ({Index})", LogType.Runtime, false);
            })
        {
            Parent = this,
            Size = new ScalableVector2(Width - EditLayerNameButton.X - EditLayerNameButton.Width, Height),
            X = EditLayerNameButton.X + EditLayerNameButton.Width,
            Alpha = 0
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        public override void UpdateContent(EditorLayerInfo layer, int index)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateSelection(GameTime gameTime)
        {
            float targetAlpha;

            if (LayerCompositor.SelectedLayerIndex.Value == Index)
                targetAlpha = 0.45f;
            else if (GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position))
                targetAlpha = 0.25f;
            else
                targetAlpha = 0f;

            Alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));
        }
    }
}