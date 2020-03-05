using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
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

        private EditorActionManager ActionManager { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="selectedLayer"></param>
        /// <param name="defaultLayer"></param>
        /// <param name="size"></param>
        public EditorPanelLayersScrollContainer(EditorActionManager actionManager, Qua workingMap,
            Bindable<EditorLayerInfo> selectedLayer, EditorLayerInfo defaultLayer, ScalableVector2 size)
            : base(new List<EditorLayerInfo>(), int.MaxValue, 0, size, size)
        {
            WorkingMap = workingMap;
            SelectedLayer = selectedLayer;
            DefaultLayer = defaultLayer;
            ActionManager = actionManager;

            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 3;
            Alpha = 0;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 800;
            ScrollSpeed = 320;

            CreateLayersList();
            CreatePool();

            ActionManager.LayerCreated += OnLayerCreated;
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

        public override void Destroy()
        {
            ActionManager.LayerCreated -= OnLayerCreated;
            base.Destroy();
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

        private void OnLayerCreated(object sender, EditorLayerCreatedEventArgs e)
        {
            if (AvailableItems.Contains(e.Layer))
                return;

            if (Pool.Any(x => x.Item == e.Layer))
                return;

            AddObjectToBottom(e.Layer, true);

            var item = Pool.Last() as DrawableEditorLayer;
            SelectedLayer.Value = item?.Item;
        }
    }
}