using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Containers;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorDrawableScrollVelocity : PoolableSprite<SliderVelocityInfo>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 40;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public EditorDrawableScrollVelocity(PoolableScrollContainer<SliderVelocityInfo> container, SliderVelocityInfo item, int index) : base(container, item, index)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(SliderVelocityInfo item, int index)
        {
        }
    }
}