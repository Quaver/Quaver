using System;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public enum KeybindActions
    {
        ExitEditor,
        PlayPause,
        ZoomIn,
        ZoomInLarge,
        ZoomOut,
        ZoomOutLarge,
        SeekForwards,
        SeekBackwards,
        SeekForwards1ms,
        SeekBackwards1ms,
        SeekToStartOfSelection,
        SeekToEndOfSelection,
        SeekToStart,
        SeekToEnd,
    }
}