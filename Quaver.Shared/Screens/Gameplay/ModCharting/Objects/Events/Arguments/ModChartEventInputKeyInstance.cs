using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventInputKeyPressInstance(
    GameplayHitObjectKeysInfo HitObject,
    int Time,
    Judgement Judgement)
    : ModChartEventInstance(ModChartEventType.InputKeyPress);

[MoonSharpUserData]
public record ModChartEventInputKeyReleaseInstance(
    GameplayHitObjectKeysInfo HitObject,
    int Time,
    Judgement Judgement)
    : ModChartEventInstance(ModChartEventType.InputKeyRelease);