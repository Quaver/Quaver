using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
// ReSharper disable ExpressionIsAlwaysNull

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardEvents
{
    private ElementAccessShortcut Shortcut { get; set; }
    
    public HitObjectManagerKeys HitObjectManagerKeys =>
        (HitObjectManagerKeys)Shortcut.GameplayScreen.Ruleset.HitObjectManager;

    public StoryboardEvents(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
        HitObjectManagerKeys.RenderedHitObjectInfoAdded += CallNoteEntry;
    }

    public event Action<GameplayHitObjectKeysInfo> NoteEntry;
    private void CallNoteEntry(GameplayHitObjectKeysInfo info) => NoteEntry?.Invoke(info);

    ~StoryboardEvents()
    {
        HitObjectManagerKeys.RenderedHitObjectInfoAdded -= CallNoteEntry;
    }
}