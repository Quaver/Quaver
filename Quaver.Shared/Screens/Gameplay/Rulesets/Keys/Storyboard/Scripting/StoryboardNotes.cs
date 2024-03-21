using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardNotes
{
    [MoonSharpHidden] private ElementAccessShortcut Shortcut { get; set; }

    public HitObjectManagerKeys HitObjectManagerKeys => (HitObjectManagerKeys)Shortcut.GameplayScreen.Ruleset.HitObjectManager;

    public StoryboardNotes(GameplayScreenView screenView)
    {
        Shortcut = new ElementAccessShortcut(screenView);
    }

    public List<GameplayHitObjectKeysInfo> HitObjectInfos => HitObjectManagerKeys.HitObjectInfos;
    public HashSet<GameplayHitObjectKeysInfo> RenderedHitObjectInfos => HitObjectManagerKeys.RenderedHitObjectInfos;
    public HitObjectInfo NextHitObject => HitObjectManagerKeys.NextHitObject;

    public List<GameplayHitObjectKeysInfo> AtTime(long time) => HitObjectManagerKeys.SpatialHashMap.GetValues(time);
}