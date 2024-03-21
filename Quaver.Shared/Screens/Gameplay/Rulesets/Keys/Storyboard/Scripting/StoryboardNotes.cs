using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardNotes
{
    [MoonSharpHidden] private ElementAccessShortcut Shortcut { get; set; }

    public HitObjectManagerKeys HitObjectManagerKeys =>
        (HitObjectManagerKeys)Shortcut.GameplayScreen.Ruleset.HitObjectManager;

    public StoryboardNotes(GameplayScreenView screenView)
    {
        Shortcut = new ElementAccessShortcut(screenView);
    }

    public List<GameplayHitObjectKeysInfo> HitObjectInfos => HitObjectManagerKeys.HitObjectInfos;
    public HashSet<GameplayHitObjectKeysInfo> RenderedHitObjectInfos => HitObjectManagerKeys.RenderedHitObjectInfos;

    public GameplayHitObjectKeysInfo NextHitObjectOnLane(int lane)
    {
        return HitObjectManagerKeys.HitObjectQueueLanes[lane - 1].TryPeek(out var peek)
            ? peek
            : null;
    }

    public GameplayHitObjectKeysInfo NextHitObject
    {
        get
        {
            GameplayHitObjectKeysInfo nextHitObject = null;
            foreach (var queueLane in HitObjectManagerKeys.HitObjectQueueLanes)
            {
                if (!queueLane.TryPeek(out var peek)) continue;
                if (nextHitObject == null || peek.StartTime < nextHitObject.StartTime)
                {
                    nextHitObject = peek;
                }
            }

            return nextHitObject;
        }
    }

    public bool IsInScreen(GameplayHitObjectKeysInfo info) => RenderedHitObjectInfos.Contains(info);
    public List<GameplayHitObjectKeysInfo> AtTime(long time) => HitObjectManagerKeys.SpatialHashMap.GetValues(time);
}