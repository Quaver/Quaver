using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Containers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public class EditorPanelLayersScrollContainer : PoolableScrollContainer<EditorLayerInfo>
    {
        private Qua WorkingMap { get; }

        private Bindable<EditorLayerInfo> SelectedLayer { get; }

        private EditorLayerInfo DefaultLayer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="selectedLayer"></param>
        /// <param name="defaultLayer"></param>
        /// <param name="size"></param>
        public EditorPanelLayersScrollContainer(Qua workingMap, Bindable<EditorLayerInfo> selectedLayer, EditorLayerInfo defaultLayer,
            ScalableVector2 size)
            : base(new List<EditorLayerInfo>(), int.MaxValue, 0, size, size)
        {
            WorkingMap = workingMap;
            SelectedLayer = selectedLayer;
            DefaultLayer = defaultLayer;

            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 3;
            Alpha = 0;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 400;
            ScrollSpeed = 320;

            CreateLayersList();

            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the list of displayed layers. Adds the default layer on top
        /// </summary>
        private void CreateLayersList()
        {
            var items = new List<EditorLayerInfo> { DefaultLayer };
            items.AddRange(WorkingMap.EditorLayers);
            AvailableItems = items;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<EditorLayerInfo> CreateObject(EditorLayerInfo item, int index)
            => new DrawableEditorLayer(this, item, index, SelectedLayer);
    }
}