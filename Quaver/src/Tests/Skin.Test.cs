using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Skins;

namespace Quaver.Tests
{
    internal static class SkinTest
    {
        [Conditional("DEBUG")]
        internal static void ParseSkinTest(bool run)
        {
            if (!run)
                return;

            const string directory = @"Swan";

            var watch = Stopwatch.StartNew();
            var skin = new Skin(directory);
            watch.Stop();
            
            Console.Write($"Displaying values for parsed skin: {directory}\n\n" +
                          $"Name: {skin.Name}\n" +
                          $"Author: {skin.Author}\n" +
                          $"Version: {skin.Version}\n" +
                          $"CustomBackground: {skin.CustomBackground}\n" +
                          $"CursorRotate: {skin.CursorRotate}\n" +
                          $"CursorTrailRotate: {skin.CursorRotate}\n" +
                          $"CursorExpand: {skin.CursorExpand}\n" +
                          $"BgMaskBufferSize: {skin.BgMaskBufferSize}\n" +
                          $"NoteBufferSpacing: {skin.NoteBufferSpacing}\n" +
                          $"TimingBarPixelSize: {skin.TimingBarPixelSize}\n" +
                          $"HitLightingScale: {skin.HitLightingScale}\n" +
                          $"ColumnSize: {skin.ColumnSize}\n" +
                          $"ReceptorYOffset: {skin.ReceptorYOffset}\n" +
                          $"ColourLight1: {skin.ColourLight1}\n" +
                          $"ColourLight2: {skin.ColourLight2}\n" +
                          $"ColourLight3: {skin.ColourLight3}\n" +
                          $"ColourLight4: {skin.ColourLight4}\n" +
                          $"Colour1: {skin.Colour1}\n" +
                          $"Colour2: {skin.Colour2}\n" +
                          $"Colour3: {skin.Colour3}\n" +
                          $"Colour4: {skin.Colour4}\n" +
                          $"Parsing Took : {watch.ElapsedMilliseconds}ms to execute.\n" +
                          $"----------------------------------------------------------\n");
        }
    }
}
