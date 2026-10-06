using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;

public static class ScrollDirectionHelper
{
    public static ScrollDirection[] ExpandScrollDirections(int keys,
        ScrollDirection scrollDirection)
    {
        var scrollDirections = new ScrollDirection[keys];

        switch (scrollDirection)
        {
            // Case: Config = Split Scroll
            case ScrollDirection.Split:
                {
                    var halfIndex = (int)Math.Ceiling(keys / 2.0);
                    for (var i = 0; i < keys; i++)
                    {
                        if (i >= halfIndex)
                            scrollDirections[i] = ScrollDirection.Up;
                        else
                            scrollDirections[i] = ScrollDirection.Down;
                    }

                    break;
                }
            // Case: Config = Reverse Split Scroll
            case ScrollDirection.SplitReverse:
                {
                    var halfIndex = (int)Math.Floor(keys / 2.0);
                    for (var i = 0; i < keys; i++)
                    {
                        if (i >= halfIndex)
                            scrollDirections[i] = ScrollDirection.Down;
                        else
                            scrollDirections[i] = ScrollDirection.Up;
                    }

                    break;
                }
            case ScrollDirection.Alternate or ScrollDirection.AlternateReverse:
                {
                    var cur = scrollDirection is ScrollDirection.Alternate
                        ? ScrollDirection.Down
                        : ScrollDirection.Up;
                    for (var i = 0; i < keys; i++)
                    {
                        scrollDirections[i] = cur;
                        cur = cur == ScrollDirection.Down
                            ? ScrollDirection.Up
                            : ScrollDirection.Down;
                    }

                    break;
                }
            case ScrollDirection.Mid or ScrollDirection.MidReverse:
                {
                    var startIdx = (keys + 3) / 4;
                    var count = keys - startIdx * 2;
                    var cur = scrollDirection is ScrollDirection.MidReverse
                        ? ScrollDirection.Down
                        : ScrollDirection.Up;
                    Array.Fill(scrollDirections, cur);
                    cur = cur == ScrollDirection.Down
                        ? ScrollDirection.Up
                        : ScrollDirection.Down;
                    Array.Fill(scrollDirections, cur, startIdx, count);
                    break;
                }
            default:
                // Case: Config = Down/Up Scroll
                scrollDirections = Enumerable.Repeat(scrollDirection, keys).ToArray();
                break;
        }

        return scrollDirections;
    }
}