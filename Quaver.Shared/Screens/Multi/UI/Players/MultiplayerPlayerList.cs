using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multi.UI.Players
{
    public class MultiplayerPlayerList : ScrollContainer
    {
        /// <summary>
        /// </summary>
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(900, 566);

        /// <summary>
        /// </summary>
        public MultiplayerPlayerList() : base(ContainerSize, ContainerSize)
        {
            Alpha = 0.45f;
        }
    }
}