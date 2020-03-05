using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Checkbox = Wobble.Graphics.UI.Form.Checkbox;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public sealed class DrawableEditorLayer : PoolableSprite<EditorLayerInfo>
    {
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 46;

        /// <summary>
        /// </summary>
        private Bindable<EditorLayerInfo> SelectedLayer { get; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BorderLine { get; set; }

        /// <summary>
        /// </summary>
        private Checkbox Visibility { get; set; }

        /// <summary>
        /// </summary>
        private IconButton EditButton { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="selectedLayer"></param>
        public DrawableEditorLayer(PoolableScrollContainer<EditorLayerInfo> container,
            EditorLayerInfo item, int index, Bindable<EditorLayerInfo> selectedLayer) : base(container, item, index)
        {
            SelectedLayer = selectedLayer;

            UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateButton();
            CreateBorderLine();
            CreateVisibilityCheckbox();
            CreateEditButton();
            CreateName();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsSelected())
                Button.Alpha = 1;
            else if (Button.IsHovered)
                Button.Alpha = 0.45f;
            else
                Button.Alpha = 0;

            BorderLine.Alpha = Button.Alpha;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(EditorLayerInfo item, int index)
        {
            Item = item;
            Index = index;

            AddScheduledUpdate(() => Name.Text = Item.Name);
        }

        /// <summary>
        ///     Creates <see cref="Button"/>
        /// </summary>
        private void CreateButton()
        {
            Button = new ContainedButton(Container, UserInterface.OptionsSidebarButtonBackground)
            {
                Parent = this,
                Size = Size,
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0
            };

            Button.Clicked += (sender, args) =>
            {
                // Default layer, so reset it back to null
                if (Item == Container.AvailableItems.First())
                {
                    SelectedLayer.Value = null;
                    return;
                }

                SelectedLayer.Value = Item;
            };
        }

        /// <summary>
        ///     Creates <see cref="BorderLine"/>
        /// </summary>
        private void CreateBorderLine()
        {
            BorderLine = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(4, Height),
                Alpha = 0
            };
        }

        /// <summary>
        ///     Creates <see cref="Visibility"/>
        /// </summary>
        private void CreateVisibilityCheckbox()
        {
            Visibility = new Checkbox(new Bindable<bool>(!Item.Hidden), new Vector2(20, 20),
                FontAwesome.Get(FontAwesomeIcon.fa_check),
                FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty), true)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true,
                Tint = GetColor()
            };

            Visibility.BindedValue.ValueChanged += (sender, args) => Item.Hidden = !Item.Hidden;
        }

        /// <summary>
        ///     Creates <see cref="EditButton"/>
        /// </summary>
        private void CreateEditButton()
        {
            EditButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Visibility.X,
                Size = new ScalableVector2(18, 18),
                Tint = GetColor(),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="Name"/>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Visibility.X + Visibility.Width + 14,
                Tint = GetColor(),
                Y = 2
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Color GetColor() => ColorHelper.ToXnaColor(Item.GetColor());

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private bool IsSelected()
        {
            // Default layer is selected when SelectedLayer is null
            if (SelectedLayer.Value == null && Item == Container.AvailableItems.First())
                return true;

            return SelectedLayer.Value == Item;
        }
    }
}