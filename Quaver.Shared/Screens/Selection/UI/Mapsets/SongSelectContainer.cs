using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public abstract class SongSelectContainer<T> : PoolableScrollContainer<T>
    {
        /// <summary>
        /// </summary>
        public abstract SelectScrollContainerType Type { get; }

        /// <summary>
        ///     The index of the selected available item
        /// </summary>
        public BindableInt SelectedIndex { get; set; }

        /// <summary>
        ///     The height of the scroll container
        /// </summary>
        public static int HEIGHT { get; } = 880;

        /// <summary>
        /// </summary>
        protected Sprite ScrollbarBackground { get; set; }

        /// <summary>
        ///     Event invoked when the mapset container has had its maps initialized
        /// </summary>
        public event EventHandler<SelectContainerInitializedEventArgs> ContainerInitialized;

        /// <summary>
        ///     The area that is clickable for buttons within the container
        /// </summary>
        public Container ClickableArea { get; }

        /// <summary>
        /// </summary>
        /// <param name="availableItems"></param>
        /// <param name="poolSize"></param>
        public SongSelectContainer(List<T> availableItems, int poolSize) : base(availableItems, poolSize, 0,
            new ScalableVector2(DrawableMapset.WIDTH, HEIGHT), new ScalableVector2(DrawableMapset.WIDTH,0))
        {
            AutoScaleHeight = true;

            if (PoolSize != int.MaxValue)
                PoolSize = (int) (poolSize * WindowManager.BaseToVirtualRatio);

            PaddingBottom = 10;

            InputEnabled = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 320;

            Alpha = 0;
            CreateScrollbar();

            ClickableArea = new Container()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Width = Width,
                Height = HEIGHT,
                AutoScaleHeight = true
            };

            SelectedIndex = new BindableInt(-1, 0, int.MaxValue);

            // ReSharper disable once VirtualMemberCallInConstructor
            SetSelectedIndex();

            PoolStartingIndex = DesiredPoolStartingIndex(SelectedIndex.Value);
            CreatePool();
            PositionAndContainPoolObjects();
            SnapToSelected();
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

            if (DialogManager.Dialogs.Count == 0 && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) &&
                !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
            {
                HandleInput(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ContainerInitialized = null;
            SelectedIndex?.Dispose();
            base.Destroy();
        }

        /// <summary>
        ///     Creates the scrollbar sprite and aligns it properly
        /// </summary>
        private void CreateScrollbar()
        {
            ScrollbarBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = 30,
                Size = new ScalableVector2(4, Height - 50),
                Tint = ColorHelper.HexToColor("#474747")
            };

            Scrollbar.Width = ScrollbarBackground.Width;
            Scrollbar.Parent = ScrollbarBackground;
            Scrollbar.Alignment = Alignment.BotCenter;
            Scrollbar.Tint = Color.White;
        }

        /// <summary>
        ///     Scrolls the container to the selected position
        /// </summary>
        public virtual void ScrollToSelected(int time = 1800)
        {
            // Scroll the the place where the map is.
            var targetScroll = GetSelectedPosition();
            ScrollTo(targetScroll, time);
        }

        /// <summary>
        ///     Snaps the scroll container to the initial mapset.
        /// </summary>
        protected void SnapToSelected()
        {
            ContentContainer.Y = SelectedIndex.Value < 7 ? 0 : GetSelectedPosition();

            ContentContainer.Animations.Clear();
            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;
            HandlePoolShifting();
        }

        /// <summary>
        ///     Destroys all of the objects in the pool and clears the list
        /// </summary>
        protected void DestroyAndClearPool()
        {
            lock (Pool)
            {
                Pool.ForEach(x => x.Destroy());
                Pool.Clear();
            }
        }

        /// <summary>
        ///     Makes sure all of the objects in the pool are positioned properly
        ///     and contained in the container
        /// </summary>
        protected void PositionAndContainPoolObjects()
        {
            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Y = (PoolStartingIndex + i) * Pool[i].HEIGHT + PaddingTop;
                AddContainedDrawable(Pool[i]);
            }
        }

        /// <summary>
        ///     Fires the <see cref="ContainerInitialized"/> event
        /// </summary>
        protected void FireInitializedEvent() => ContainerInitialized?.Invoke(this, new SelectContainerInitializedEventArgs());

        /// <summary>
        ///     Gets the position of the selected item
        /// </summary>
        /// <returns></returns>
        protected abstract float GetSelectedPosition();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract override PoolableSprite<T> CreateObject(T item, int index);

        /// <summary>
        ///     Handles input for the scroll container
        /// </summary>
        /// <param name="gameTime"></param>
        protected abstract void HandleInput(GameTime gameTime);

        /// <summary>
        ///     Sets the appropriate index of the selected mapset
        /// </summary>
        protected abstract void SetSelectedIndex();
    }
}
