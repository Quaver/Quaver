/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
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
        public EditorLayerVisiblityCheckbox VisibilityCheckbox { get; private set; }

        /// <summary>
        /// </summary>
        public JukeboxButton EditLayerNameButton { get; private set; }

        /// <summary>
        /// </summary>
        public EditorDrawableLayerButton SelectLayerButton { get; private set; }

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
        /// <param name="scrollContainer"></param>
        /// <param name="layerCompositor"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public EditorDrawableLayer(EditorLayerScrollContainer scrollContainer, EditorLayerCompositor layerCompositor,
            EditorLayerInfo item, int index) : base(scrollContainer, item, index)
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
            if (Rectangle.Intersect(ScreenRectangle, Container.ScreenRectangle).IsEmpty)
                return;

            AnimateSelection(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (Rectangle.Intersect(ScreenRectangle, Container.ScreenRectangle).IsEmpty)
                return;

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SelectLayerButton.Destroy();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateVisibilityCheckbox() => VisibilityCheckbox = new EditorLayerVisiblityCheckbox(this)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 12,
            Size = new ScalableVector2(16, 16),
            Tint = ColorHelper.ToXnaColor(Item.GetColor())
        };

        /// <summary>
        /// </summary>
        private void CreateEditNamePencil() => EditLayerNameButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil),
            (o, e) =>
            {
                if (Index == 0)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You cannot edit the default layer!");
                    return;
                }

                LayerCompositor.SelectedLayerIndex.Value = Index;
                LayerCompositor.Screen.ActiveLayerInterface.Value = EditorLayerInterface.Editing;
            })
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = VisibilityCheckbox.X + VisibilityCheckbox.Width + 10,
            Size = VisibilityCheckbox.Size,
            Tint = ColorHelper.ToXnaColor(Item.GetColor())
        };

        /// <summary>
        /// </summary>
        private void CreateLayerName() => LayerName = new SpriteTextBitmap(FontsBitmap.AllerRegular, Item.Name)
        {
            Parent = this,
            FontSize = 16,
            Alignment = Alignment.MidLeft,
            X = EditLayerNameButton.X + EditLayerNameButton.Width + 10,
            Tint = ColorHelper.ToXnaColor(Item.GetColor())
        };

        /// <summary>
        /// </summary>
        private void CreateSelectLayerButton() => SelectLayerButton = new EditorDrawableLayerButton(LayerCompositor, UserInterface.BlankBox,
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
            LayerName.Text = layer.Name;
            VisibilityCheckbox.Tint = ColorHelper.ToXnaColor(Item.GetColor());
            LayerName.Tint = ColorHelper.ToXnaColor(Item.GetColor());
            EditLayerNameButton.Tint = ColorHelper.ToXnaColor(Item.GetColor());
            Item = layer;
            Index = index;
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
