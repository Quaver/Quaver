using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Localization;
using Wobble;
using Wobble.Graphics.ImGUI;

namespace Quaver.Shared.Screens.Edit;

public static class EditorImGuiOptions
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static ImGuiOptions GetOptions()
    {
        var interPath = $@"{WobbleGame.WorkingDirectory}/Fonts/inter.ttf";
        var notoSansCjkPath = $@"{WobbleGame.WorkingDirectory}/Fonts/noto-sans-cjk.ttc";
        return new ImGuiOptions([
            new ImGuiFont(interPath, ConfigManager.EditorImGuiFontSize.Value,
            [
                new ImGuiFontFallback(interPath, glyphRanges: ImGuiGlyphRanges.Cyrillic),

                new ImGuiFontFallback(notoSansCjkPath,
                    QuaverLocalization.GetNotoCjkFaceIndex(ConfigManager.Language.Value),
                    ImGuiGlyphRanges.ChineseFull),

                new ImGuiFontFallback(notoSansCjkPath,
                    QuaverLocalization.NotoCjkScFaceIndex,
                    ImGuiGlyphRanges.ChineseSimplifiedCommon),

                new ImGuiFontFallback(notoSansCjkPath,
                    QuaverLocalization.NotoCjkHkFaceIndex,
                    ImGuiGlyphRanges.Korean),

                new ImGuiFontFallback(notoSansCjkPath,
                    QuaverLocalization.NotoCjkJpFaceIndex,
                    ImGuiGlyphRanges.Japanese)
            ])
        ], false);
    }
}
