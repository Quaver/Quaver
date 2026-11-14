using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Assets;

internal sealed class ChatEmoteAtlas
{
    private const int SourceSize = 30;
    private const int SourceStride = 38;

    private static readonly string[] EmoteNames =
    [
        "smile",
        "laugh",
        "surprise",
        "rofl",
        "frown",
        "sad-tear",
        "grin-beam-sweat",
        "angry",
        "grin-hearts",
        "grin-tongue",
        "thumbs-up",
        "thumbs-down",
        "heart",
        "quaver",
        "swan",
        "ln",
        "mines",
        "arrow-up",
        "arrow-down",
        "jerry"
    ];

    private static readonly Dictionary<string, int> Indices = CreateIndices();

    private TextureRegion[] Regions { get; }

    public static IReadOnlyList<string> Names => EmoteNames;

    private ChatEmoteAtlas(Texture2D sheet)
    {
        Regions = new TextureRegion[EmoteNames.Length];

        for (var i = 0; i < Regions.Length; i++)
            Regions[i] = new TextureRegion(sheet, new Rectangle(0, i * SourceStride, SourceSize, SourceSize));
    }

    public static ChatEmoteAtlas Create(Texture2D sheet, Texture2D fallback)
    {
        var texture = IsValid(sheet) ? sheet : fallback;
        if (!IsValid(texture))
            throw new ArgumentException("The chat emote sheet does not contain all expected emote regions.", nameof(fallback));

        return new ChatEmoteAtlas(texture);
    }

    public TextureRegion Get(string name)
    {
        if (!Indices.TryGetValue(name, out var index))
            throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown chat emote name.");

        return Regions[index];
    }

    public static bool TryGetName(string alias, out string name)
    {
        name = null;
        if (!TryGetIndex(alias, out var index))
            return false;

        name = EmoteNames[index];
        return true;
    }

    public static bool TryGetIndex(string alias, out int index)
    {
        index = -1;
        if (string.IsNullOrEmpty(alias) || alias.Length < 3 || alias[0] != ':' || alias[^1] != ':')
            return false;

        return Indices.TryGetValue(alias.Substring(1, alias.Length - 2), out index);
    }

    public static string GetAlias(string name) => $":{name}:";

    private static bool IsValid(Texture2D texture) =>
        texture != null && !texture.IsDisposed && texture.Width >= SourceSize &&
        texture.Height >= (EmoteNames.Length - 1) * SourceStride + SourceSize;

    private static Dictionary<string, int> CreateIndices()
    {
        var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < EmoteNames.Length; i++)
            indices.Add(EmoteNames[i], i);

        return indices;
    }
}
