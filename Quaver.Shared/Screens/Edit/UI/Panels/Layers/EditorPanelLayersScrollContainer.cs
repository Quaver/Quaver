using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public class EditorPanelLayersScrollContainer : PoolableScrollContainer<EditorLayerInfo>
    {
        public Qua WorkingMap { get; }

        private Bindable<EditorLayerInfo> SelectedLayer { get; }

        private EditorLayerInfo DefaultLayer { get; }

        public EditorActionManager ActionManager { get; }

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        public RightClickOptions ActiveRightClickOptions { get; private set; }

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
            ScrollSpeed = 150;

            CreateLayersList();
            CreatePool();

            ActionManager.LayerCreated += OnLayerCreated;
            ActionManager.LayerDeleted += OnLayerDeleted;
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


        /// <summary>
        /// </summary>
        /// <param name="rco"></param>
        public void ActivateRightClickOptions(RightClickOptions rco)
        {
            if (ActiveRightClickOptions != null)
            {
                ActiveRightClickOptions.Visible = false;
                ActiveRightClickOptions.Parent = null;
                ActiveRightClickOptions.Destroy();
            }

            ActiveRightClickOptions = rco;
            ActiveRightClickOptions.Parent = this;

            ActiveRightClickOptions.ItemContainer.Height = 0;
            ActiveRightClickOptions.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveRightClickOptions.Width - AbsolutePosition.X, 0,
                Width - ActiveRightClickOptions.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y - AbsolutePosition.Y, 0,
                Height - ActiveRightClickOptions.Items.Count * ActiveRightClickOptions.Items.First().Height);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);
            ActiveRightClickOptions.Open(350);
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

            if (e.Index >= 1)
            {
                AddObjectAtIndex(e.Index, e.Layer, true);
                var item = Pool[e.Index] as DrawableEditorLayer;
                SelectedLayer.Value = item?.Item;
            }
            else
            {
                AddObjectToBottom(e.Layer, true);
                var item = Pool.Last() as DrawableEditorLayer;
                SelectedLayer.Value = item?.Item;
            }
        }

        private void OnLayerDeleted(object sender, EditorLayerRemovedEventArgs e)
        {
            var index = AvailableItems.IndexOf(e.Layer);
            AvailableItems.Remove(e.Layer);

            var item = Pool.Find(x => x.Item == e.Layer) as DrawableEditorLayer;

            AddScheduledUpdate(() =>
            {
                // Remove the item if it exists in the pool.
                if (item != null)
                {
                    item.Destroy();
                    RemoveContainedDrawable(item);
                    Pool.Remove(item);
                }

                RecalculateContainerHeight();

                // Reset the pool item index
                for (var i = 0; i < Pool.Count; i++)
                {
                    Pool[i].Index = i;
                    Pool[i].Y = (PoolStartingIndex + i) * Pool[i].HEIGHT;
                    Pool[i].UpdateContent(Pool[i].Item, i);
                }

                if (index - 1 >= 0)
                    SelectedLayer.Value = AvailableItems[index - 1];
            });
        }
    }
}