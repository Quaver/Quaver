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
                          $"BgMaskPadding: {skin.BgMaskPadding4K}\n" +
                          $"BgMaskPadding: {skin.BgMaskPadding7K}\n" +
                          $"NotePadding: {skin.NotePadding4K}\n" +
                          $"NotePadding: {skin.NotePadding7K}\n" +
                          $"TimingBarPixelSize: {skin.TimingBarPixelSize}\n" +
                          $"HitLightingScale: {skin.ColumnLightingScale}\n" +
                          $"ColumnSize4K: {skin.ColumnSize4K}\n" +
                          $"ColumnSize4K: {skin.ColumnSize7K}\n" +
                          $"ReceptorYOffset4K: {skin.ReceptorPositionOffset4K}\n" +
                          $"ReceptorYOffset7K: {skin.ReceptorPositionOffset7K}\n" +
                          $"Parsing Took : {watch.ElapsedMilliseconds}ms to execute.\n" +
                          $"----------------------------------------------------------\n");
        }
    }
}
