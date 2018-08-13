using System;

namespace Quaver.Screens.Select.UI.Selector
{
    [Flags]
    public enum MapAssetsToLoad
    {
        Background = 1 << 0,
        Audio = 1 << 1
    }
}