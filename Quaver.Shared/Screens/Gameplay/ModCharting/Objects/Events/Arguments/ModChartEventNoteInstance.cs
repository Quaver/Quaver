using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventNoteEntryInstance(GameplayHitObjectKeysInfo HitObject)
    : ModChartEventInstance(ModChartEventType.NoteEntry);