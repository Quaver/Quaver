using System.Collections.Generic;
using System.Drawing;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Select.UI.Modifiers.Windows
{
    public class JudgementWindowContainer : PoolableScrollContainer<JudgementWindows>
    {
        public JudgementWindowContainer(List<JudgementWindows> availableItems) : base(availableItems, int.MaxValue, 0,
            new ScalableVector2(220, 402), new ScalableVector2(220, 402))
        {
            Alpha = 0;
            CreatePool();

            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 3;
            Scrollbar.X = -Width - 6;
            EasingType = Easing.OutQuint;
            ScrollSpeed = 150;
            TimeToCompleteScroll = 1200;
        }

        protected override PoolableSprite<JudgementWindows> CreateObject(JudgementWindows item, int index) => new DrawableJudgementWindows(this, item, index);

        public void AddObject(JudgementWindows windows)
        {
            AddObjectToBottom(windows, true);
        }

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
    }
}