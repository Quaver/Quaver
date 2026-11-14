using System;
using System.Text.RegularExpressions;

namespace Quaver.Shared.Assets;

internal static class ChatEmoteGlyphs
{
    private const int FirstCodepoint = 0xF0000;

    private static readonly Regex AliasPattern = new(@":[a-z0-9-]+:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string ReplaceAliases(string text)
    {
        return AliasPattern.Replace(text ?? string.Empty, match =>
        {
            return ChatEmoteAtlas.TryGetIndex(match.Value, out var index)
                ? char.ConvertFromUtf32(FirstCodepoint + index)
                : match.Value;
        });
    }
}
