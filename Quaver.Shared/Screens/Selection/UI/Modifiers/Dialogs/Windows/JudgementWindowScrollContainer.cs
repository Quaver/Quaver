using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowScrollContainer : PoolableScrollContainer<JudgementWindows>
    {
        public JudgementWindowScrollContainer(List<JudgementWindows> availableItems, ScalableVector2 size)
            : base(availableItems, int.MaxValue, 0, size, size)
        {
            Alpha = 0;
            CreatePool();

            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 4;
            EasingType = Easing.OutQuint;
            ScrollSpeed = 150;
            TimeToCompleteScroll = 1200;

            // Scroll background/divider line
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(4, Height),
                Tint = ColorHelper.HexToColor("#474747")
            };

            // Kakes it so the scrollbar appears over
            Scrollbar.Parent = this;

            SnapToSelected();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<JudgementWindows> CreateObject(JudgementWindows item, int index)
            => new DrawableJudgementWindows(this, item, index);

        /// <summary>
        ///     Adds an object to the bottom of the container
        /// </summary>
        /// <param name="windows"></param>
        public void AddObject(JudgementWindows windows) => AddObjectToBottom(windows, true);

        /// <summary>
        ///     Removes an object from the container
        /// </summary>
        /// <param name="windows"></param>
        public void Remove(JudgementWindows windows)
        {
            var item = Pool.Find(x => x.Item == windows);

            if (item == null)
                return;

            item.Destroy();
            AvailableItems.Remove(windows);
            Pool.Remove(item);

            for (var i = 0; i < Pool.Count; i++)
            {
                var pool = Pool[i];
                pool.Index = i;
                pool.Y = pool.Height * i;
            }

            RecalculateContainerHeight();
        }

        /// <summary>
        ///     Snaps the scroll container to the initial mapset.
        /// </summary>
        protected void SnapToSelected()
        {
            var index = JudgementWindowsDatabaseCache.Presets.IndexOf(JudgementWindowsDatabaseCache.Selected.Value);

            ContentContainer.Animations.Clear();
            ContentContainer.Y =  index < 8 ? 0  : (-index + 6) * Pool.First().Height;


            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;

        }
    }
}