using MoonSharp.Interpreter;
using Quaver.Shared.Audio;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartAudio : ModChartGlobalVariable
{
    public ModChartAudio(ElementAccessShortcut shortcut) : base(shortcut)
    {
    }

    public void PlaySample(int index, int volume = 100) => CustomAudioSampleCache.Play(index, volume);

    public void PauseAllSamples() => CustomAudioSampleCache.PauseAll();

    public void ResumeAllSamples() => CustomAudioSampleCache.ResumeAll();

    public void StopAllSamples() => CustomAudioSampleCache.StopAll();
}