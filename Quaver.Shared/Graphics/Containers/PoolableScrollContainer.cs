/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Containers
{
    public abstract class PoolableScrollContainer<T> : ScrollContainer
    {
        /// <summary>
        ///     The pool of sprites to be used within the container
        /// </summary>
        public List<PoolableSprite<T>> Pool { get; protected set; }

        /// <summary>
        ///     The size of the object pool
        /// </summary>
        public int PoolSize { get; protected set; }

        /// <summary>
        ///     The amount of padding from the top that the scroll container will have
        /// </summary>
        protected int PaddingTop { get; set; }

        /// <summary>
        ///     The amount of padding from the bottom that the scroll container will have
        /// </summary>
        protected int PaddingBottom { get; set; }

        /// <summary>
        ///     The items that are available to use for the drawables.
        ///     Essentially what the drawable represents.
        /// </summary>
        public List<T> AvailableItems { get; set; }

        /// <summary>
        ///    The index at which the object pool begins, so we'll be aware of where to scroll.
        /// </summary>
        public int PoolStartingIndex { get; protected set; }

        /// <summary>
        ///     Keeps track of the Y position of the content container in the previous frame
        ///     So we can know how to shift the pool.
        /// </summary>
        protected float PreviousContentContainerY { get; set; }

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
        /// <param name="createUponScrolling"></param>
        protected PoolableScrollContainer(List<T> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size, ScalableVector2 contentSize,
            bool startFromBottom = false) : base(size, contentSize)
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
            // First make sure ContentContainer.Y is up to date.
            base.Update(gameTime);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ContentContainer.Y != PreviousContentContainerY)
                HandlePoolShifting();

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;
        }

        public override void Destroy()
        {
            Pool?.ForEach(x => x?.Destroy());
            base.Destroy();
        }

        /// <summary>
        ///     Begins creation of the pool. This should be called last in the constructor when the pool
        ///     is ready to be created
        /// </summary>
        protected void CreatePool(bool containDrawables = true, bool updateContent = true)
        {
            Pool = new List<PoolableSprite<T>>();

            if (AvailableItems == null)
                return;

            // Create enough objects to use for the pool and contain them inside the drawable.
            for (var i = 0; i < PoolSize && i < AvailableItems?.Count; i++)
            {
                if (i + PoolStartingIndex >= AvailableItems.Count)
                    break;

                var drawable = AddObject(PoolStartingIndex + i, updateContent);

                if (containDrawables)
                    AddContainedDrawable(drawable);
            }

            RecalculateContainerHeight();
        }

        /// <summary>
        ///    Makes sure that the content container's height is up to date
        /// </summary>
        public virtual void RecalculateContainerHeight(bool usePoolCount = false)
        {
            var count = usePoolCount ? Pool.Count : AvailableItems.Count;

            var totalUserHeight = DrawableHeight * count + PaddingTop + PaddingBottom;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;
        }

        /// <summary>
        ///     Returns the target PoolStartingIndex given the index of the object currently in the middle of the screen.
        /// </summary>
        /// <param name="middleObjectIndex"></param>
        /// <returns></returns>
        protected int DesiredPoolStartingIndex(int middleObjectIndex)
        {
            if ((middleObjectIndex + 1) < (float)PoolSize / 2)
                return 0;

            int index;

            if ((middleObjectIndex + 1) + (float)PoolSize / 2 > AvailableItems.Count)
                index = AvailableItems.Count - PoolSize;
            else
                index = middleObjectIndex - PoolSize / 2;

            return Math.Max(index, 0);
        }

        /// <summary>
        ///     Handles the shifting of the object pool when the user scrolls up or down.
        /// </summary>
        protected void HandlePoolShifting()
        {
            if (AvailableItems == null || Pool.Count != PoolSize)
                return;

            // Compute the index of the object currently in the middle of the container.
            var middleObjectIndex = (int) ((-ContentContainer.Y + Height / 2 - PaddingTop) / DrawableHeight);

            // Compute the corresponding PoolStartingIndex.
            var desiredPoolStartingIndex = DesiredPoolStartingIndex(middleObjectIndex);

            // If our PoolStartingIndex is already correct, then there's nothing to do.
            if (PoolStartingIndex == desiredPoolStartingIndex)
                return;

            // Compute the overlap: the number of pooled objects that can be re-used from the previous position.
            var difference = Math.Abs(PoolStartingIndex - desiredPoolStartingIndex);
            var overlap = Math.Max(Pool.Count - difference, 0);
            var refresh = Pool.Count - overlap;

            if (PoolStartingIndex > desiredPoolStartingIndex)
            {
                // The container has been scrolled back. The re-usable objects are in the beginning of the buffer.
                for (var i = 0; i < refresh; i++)
                {
                    var objectIndex = desiredPoolStartingIndex + Pool.Count - 1 - overlap - i;

                    var drawable = Pool.Last();
                    drawable.Y = objectIndex * DrawableHeight + PaddingTop;
                    drawable.UpdateContent(AvailableItems[objectIndex], objectIndex);

                    // Circularly shift the list back one.
                    Pool.RemoveAt(Pool.Count - 1);
                    Pool.Insert(0, drawable);
                }
            }
            else
            {
                // The container has been scrolled forward. The re-usable objects are in the end of the buffer.
                for (var i = 0; i < refresh; i++)
                {
                    var objectIndex = desiredPoolStartingIndex + overlap + i;

                    var drawable = Pool.First();
                    drawable.Y = objectIndex * DrawableHeight + PaddingTop;
                    drawable.UpdateContent(AvailableItems[objectIndex], objectIndex);

                    // Circularly shift the list forward one.
                    Pool.RemoveAt(0);
                    Pool.Add(drawable);
                }
            }

            PoolStartingIndex = desiredPoolStartingIndex;
        }

        /// <summary>
        ///     Adds a drawable to the pool and returns it
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected PoolableSprite<T> AddObject(int index, bool updateContent = true)
        {
            lock (AvailableItems)
            {
                var drawable = CreateObject(AvailableItems[index], index);
                drawable.DestroyIfParentIsNull = false;

                Pool.Insert(index - PoolStartingIndex, drawable);

                for (int i = index; i < Pool.Count; i++)
                {
                    var foo = Pool[i];
                    foo.Y = (PoolStartingIndex + i) * foo.Height + PaddingTop;
                }

                if (updateContent)
                    drawable.UpdateContent(AvailableItems[index], index);

                return drawable;
            }
        }

        protected void AddObjectAtIndex(int index, T obj, bool scrollTo, bool usePoolCount = false)
        {
            lock (AvailableItems)
            lock (Pool)
            {
                if (!AvailableItems.Contains(obj))
                    AvailableItems.Insert(index, obj);

                // Need another drawable to use
                if (Pool.Count < PoolSize)
                    AddContainedDrawable(AddObject(index));

                RecalculateContainerHeight(usePoolCount);

                if (scrollTo)
                    ScrollTo(-index * DrawableHeight, 1000);
            }
        }

        /// <summary>
        ///     Adds an object to the available ones and displays it at the bottom.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="scrollTo"></param>
        protected void AddObjectToBottom(T obj, bool scrollTo, bool usePoolCount = false)
        {
            lock (AvailableItems)
            lock (Pool)
            {
                if (!AvailableItems.Contains(obj))
                    AvailableItems.Add(obj);

                // Need another drawable to use
                if (Pool.Count < PoolSize)
                    AddContainedDrawable(AddObject(AvailableItems.Count - 1));

                RecalculateContainerHeight(usePoolCount);

                if (scrollTo)
                    ScrollTo(-(AvailableItems.Count + 1) * DrawableHeight, 1000);
            }
        }

        /// <summary>
        ///     Removes an object from the pool
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void RemoveObject(T obj)
        {
            /*
            var index = AvailableItems.IndexOf(obj);
            var drawable = Pool.Find(x => x.Index == index);

            // Removing the last item in the list while index is still 0.
            // get rid of it entirely.
            if (index == AvailableItems.Count - 1 && PoolStartingIndex == 0)
            {
                if (drawable != null)
                {
                    Pool.Remove(drawable);
                    RemoveContainedDrawable(drawable);
                    drawable.Destroy();
                }

                AvailableItems.Remove(obj);
                RecalculateContainerHeight();
                return;
            }
            // Removing the last item in the list while the pool index isn't 0.
            // in this case, we want to destroy the object entirely if it isn't needed
            // or rearrange it, so that it's on top.
            else if (index == AvailableItems.Count - 1 && PoolStartingIndex != 0)
            {
                if (drawable != null)
                {
                    // The object needs to be destroyed since it's no longer needed.
                    if (index == PoolSize - 1)
                    {
                        Pool.Remove(drawable);
                        RemoveContainedDrawable(drawable);
                        drawable.Destroy();
                    }
                    // Reuse the object by placing it at the beginning of the pool.
                    else
                    {
                        Pool.Remove(drawable);
                        Pool.Insert(0, drawable);
                    }
                }

                AvailableItems.Remove(obj);
                RecalculateContainerHeight();
                PoolStartingIndex--;
            }
            */

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Creates an object for the sprite to use.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected abstract PoolableSprite<T> CreateObject(T item, int index);

        /// <summary>
        /// </summary>
        public void DestroyPool()
        {
            Pool.ForEach(x => x.Destroy());
            Pool.Clear();
        }
    }
}
