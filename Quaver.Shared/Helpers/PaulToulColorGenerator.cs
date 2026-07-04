using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Helpers;

public class PaulToulColorGenerator
{
    /// <summary>
    ///     Paul Toul's Color Scheme. https://personal.sron.nl/~pault/
    /// </summary>
    public static readonly Color[] ColorScheme =
    {
        new (0xffddaa77), new(0xffffdd99), new(0xff998844), new(0xff33ccbb),
        new (0xff00aaaa), new(0xff88ddee), new(0xff6688ee), new(0xffbbaaff),
        new (0xff3377ee), new(0xff1133cc), new(0xff7733ee), new(0xff7766cc),
        new (0xff552288), new(0xff9944aa), new(0xff0000ee), new(0xff00ee00),
        new (0xffeecc99), new(0xffffeebb), new(0xffbbaa66), new(0xff55dddd),
        new (0xff22cccc), new(0xffaaeeff), new(0xff88aaff), new(0xffddccff),
        new (0xff5599ff), new(0xff3355dd), new(0xff9955ff), new(0xff9988dd),
        new (0xff7744aa), new(0xffbb66cc), new(0xff2222ff), new(0xff22ff22)
    };

    private Random random = new(114514);

    private Color RandomColor() => new(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));

    public Color NextColor(HashSet<Color> existingColors = default)
    {
        existingColors ??= new HashSet<Color>();

        foreach (var color in ColorScheme)
        {
            if (!existingColors.Contains(color))
                return color;
        }

        Color c;

        while (existingColors.Contains(c = RandomColor()))
        {
        }

        return c;
    }
}