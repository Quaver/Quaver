using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Select.UI.Mapsets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Containers
{
    public abstract class PoolableScrollContainer<T> : ScrollContainer
    {
        /// <summary>
        ///     The pool of sprites to be used within the container
        /// </summary>
        public List<PoolableSprite<T>> Pool { get; private set; }

        /// <summary>
        ///     The size of the object pool
        /// </summary>
        public int PoolSize { get; }

        /// <summary>
        ///     The items that are available to use for the drawables.
        ///     Essentially what the drawable represents.
        /// </summary>
        public List<T> AvailableItems { get; }

        /// <summary>
        ///    The index at which the object pool begins, so we'll be aware of where to scroll.
        /// </summary>
        public int PoolStartingIndex { get; private set; }

        /// <summary>
        ///     Keeps track of the Y position of the content container in the previous frame
        ///     So we can know how to shift the pool.
        /// </summary>
        private float PreviousContentContainerY { get; set; }

        /// <summary>
        ///    Quick way to get the drawable's height.
        /// </summary>
        private int DrawableHeight => Pool.Count > 0 ? Pool.First().HEIGHT : 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableItems"></param>
        /// <param name="poolSize"></param>
        /// <param name="poolStartingIndex"></param>
        /// <param name="size"></param>
        /// <param name="contentSize"></param>
        /// <param name="startFromBottom"></param>
        protected PoolableScrollContainer(List<T> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size, ScalableVector2 contentSize,
            bool startFromBottom = false) : base(size, contentSize, startFromBottom)
        {
            AvailableItems = availableItems;
            PoolSize = poolSize;
            PoolStartingIndex = poolStartingIndex;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (ContentContainer.Y < PreviousContentContainerY)
                HandlePoolShifting(Direction.Forward);
            else if (ContentContainer.Y > PreviousContentContainerY)
                HandlePoolShifting(Direction.Backward);

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Begins creation of the pool. This should be called last in the constructor when the pool
        ///     is ready to be created
        /// </summary>
        protected void CreatePool()
        {
            Pool = new List<PoolableSprite<T>>(PoolSize);

            // Create enough objects to use for the pool and contain them inside the drawable.
            for (var i = 0; i < PoolSize && i < AvailableItems.Count; i++)
            {
                var drawable = AddObject(i);

                if (i >= AvailableItems.Count)
                    continue;

                AddContainedDrawable(drawable);
            }

            RecalculateContainerHeight();
        }

        /// <summary>
        ///    Makes sure that the content container's height is up to date
        /// </summary>
        protected void RecalculateContainerHeight()
        {
            var totalUserHeight = DrawableHeight * AvailableItems.Count;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;
        }

        /// <summary>
        ///     Handles the shifting of the object pool when the user scrolls up or down.
        /// </summary>
        /// <param name="direction"></param>
        private void HandlePoolShifting(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    if (PoolStartingIndex > AvailableItems.Count - 1 || PoolStartingIndex + PoolSize > AvailableItems.Count - 1)
                        return;

                    var firstDrawable = Pool.First();

                    // Check if the object is in the rect of the ScrollContainer.
                    // If it is, then there's no updating that needs to happen.
                    if (!Rectangle.Intersect(firstDrawable.ScreenRectangle, ScreenRectangle).IsEmpty)
                        return;

                    // Update the mapset's information and y position.
                    firstDrawable.Y = (PoolStartingIndex + PoolSize) * DrawableHeight;

                    firstDrawable.UpdateContent(AvailableItems[PoolStartingIndex + PoolSize], PoolStartingIndex + PoolSize);

                    // Circularly shift the drawable in the list so it's at the end.
                    Pool.Remove(firstDrawable);
                    Pool.Add(firstDrawable);

                    PoolStartingIndex++;
                    break;
                case Direction.Backward:
                    if (PoolStartingIndex - 1 > AvailableItems.Count - 1 || PoolStartingIndex - 1 < 0)
                        return;

                    var lastDrawable = Pool.Last();

                    // Check if the object is in the rect of the ScrollContainer.
                    // If it is, then there's no updating that needs to happen.
                    if (!Rectangle.Intersect(lastDrawable.ScreenRectangle, ScreenRectangle).IsEmpty)
                        return;

                    lastDrawable.Y = (PoolStartingIndex - 1) * DrawableHeight;
                    lastDrawable.UpdateContent(AvailableItems[PoolStartingIndex - 1], PoolStartingIndex - 1);

                    // Circularly shift the drawable in the list so it's at the beginning
                    Pool.Remove(lastDrawable);
                    Pool.Insert(0, lastDrawable);

                    PoolStartingIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///     Adds a drawable to the pool and returns it
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private PoolableSprite<T> AddObject(int index)
        {
            var drawable = CreateObject(AvailableItems[index], index);
            drawable.DestroyIfParentIsNull = false;
            drawable.Y = (PoolStartingIndex + index) * drawable.HEIGHT;

            drawable.UpdateContent(AvailableItems[index], index);
            Pool.Add(drawable);

            return drawable;
        }

        /// <summary>
        ///     Adds an object to the available ones and displays it at the bottom.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="scrollTo"></param>
        protected void AddObjectToBottom(T obj, bool scrollTo)
        {
            AvailableItems.Add(obj);

            // Need another drawable to use
            if (Pool.Count < PoolSize)
                AddContainedDrawable(AddObject(AvailableItems.Count - 1));

            RecalculateContainerHeight();

            if (scrollTo)
                ScrollTo(-(AvailableItems.Count + 1) * DrawableHeight, 1000);
        }

        /// <summary>
        ///     Creates an object for the sprite to use.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected abstract PoolableSprite<T> CreateObject(T item, int index);
    }
}