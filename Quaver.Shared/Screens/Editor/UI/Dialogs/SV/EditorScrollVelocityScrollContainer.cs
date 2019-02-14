using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityScrollContainer : PoolableScrollContainer<SliderVelocityInfo>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableItems"></param>
        /// <param name="poolSize"></param>
        /// <param name="poolStartingIndex"></param>
        /// <param name="size"></param>
        /// <param name="contentSize"></param>
        /// <param name="startFromBottom"></param>
        public EditorScrollVelocityScrollContainer(List<SliderVelocityInfo> availableItems, int poolSize, int poolStartingIndex,
            ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false)
            : base(availableItems, poolSize, poolStartingIndex, size, contentSize, startFromBottom)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<SliderVelocityInfo> CreateObject(SliderVelocityInfo item, int index)
        {
            return null;
        }
    }
}