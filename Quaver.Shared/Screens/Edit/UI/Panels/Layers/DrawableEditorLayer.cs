using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Actions.Layers.Rename;
using Quaver.Shared.Screens.Edit.UI.Panels.Layers.Dialogs;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;
using Checkbox = Wobble.Graphics.UI.Form.Checkbox;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

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

        private EditorPanelLayersScrollContainer LayerContainer => (EditorPanelLayersScrollContainer) Container;

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

            LayerContainer.ActionManager.LayerRenamed += OnLayerRenamed;
            LayerContainer.ActionManager.LayerColorChanged += OnLayerColorChanged;
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

            if (Visibility.BindedValue.Value == Item.Hidden)
            {
                Visibility.BindedValue.ChangeWithoutTrigger(!Item.Hidden);

                var tex = Visibility.BindedValue.Value ? Visibility.ActiveImage : Visibility.InactiveImage;

                if (Visibility.Image != tex)
                    Visibility.Image = tex;
            }

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            LayerContainer.ActionManager.LayerRenamed -= OnLayerRenamed;
            LayerContainer.ActionManager.LayerColorChanged -= OnLayerColorChanged;

            base.Destroy();
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

            AddScheduledUpdate(() =>
            {
                Name.Text = Item.Name;
                Name.Tint = GetColor();

                Visibility.Tint = Name.Tint;
                EditButton.Tint = Name.Tint;
            });
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

            Button.Clicked += (sender, args) => SelectedLayer.Value = Item;

            Button.RightClicked += (sender, args) =>
            {
                LayerContainer.ActivateRightClickOptions(new DrawableEditorLayerRightClickOptions(Item,
                    LayerContainer.ActionManager,
                    LayerContainer.WorkingMap));
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
            Visibility = new ContainedCheckbox(LayerContainer, new Bindable<bool>(!Item.Hidden), new Vector2(20, 20),
                FontAwesome.Get(FontAwesomeIcon.fa_check),
                FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty), true)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true,
                Tint = GetColor()
            };

            Visibility.BindedValue.ValueChanged += (sender, args) => LayerContainer.ActionManager.ToggleLayerVisibility(Item);
        }

        /// <summary>
        ///     Creates <see cref="EditButton"/>
        /// </summary>
        private void CreateEditButton()
        {
            EditButton = new ContainedIconButton(LayerContainer, FontAwesome.Get(FontAwesomeIcon.fa_pencil))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Visibility.X,
                Size = new ScalableVector2(18, 18),
                Tint = GetColor(),
                UsePreviousSpriteBatchOptions = true
            };

            EditButton.Clicked += (sender, args) =>
            {
                LayerContainer.ActivateRightClickOptions(new DrawableEditorLayerRightClickOptions(Item, LayerContainer.ActionManager,
                    LayerContainer.WorkingMap));
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
        private bool IsSelected() => SelectedLayer.Value == Item;

        private void OnLayerRenamed(object sender, EditorLayerRenamedEventArgs e) => UpdateContent(Item, Index);
        private void OnLayerColorChanged(object sender, EditorLayerColorChangedEventArgs e) => UpdateContent(Item, Index);
    }
}