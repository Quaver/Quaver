﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Quaver.Tests
{
    internal static class QuaTest
    {
        [Conditional("DEBUG")]
        internal static void ParseQuaTest(bool run)
        {
            if (!run)
                return;

            // Parsing a new Qua for testing purposes. We've specified a preprocessor directive here, 
            // so this'll only run in debug mode.
            var filePath = @"C:\Users\swan\Desktop\Stuff\Git\Quaver2.0\Quaver\Example\Qua\backbeat.qua";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error on ParseQuaTest: Specified file does not exist.");
                return;
            }

            /*var watch = Stopwatch.StartNew();
            var qua = new Qua(filePath);
            watch.Stop();

            Console.WriteLine($"Displaying data for parsed .qua file: {filePath}\n\n" +
                              $"Artist: {qua.Artist}\n" +
                              $"Title: {qua.Title}\n" +
                              $"Source: {qua.Source}\n" +
                              $"Tags: {qua.Tags}\n" +
                              $"Creator: {qua.Creator}\n" +
                              $"DifficultyName {qua.DifficultyName}\n" +
                              $"MapId: {qua.MapId}\n" +
                              $"MapSetId: {qua.MapSetId}\n" +
                              $"AudioFile: {qua.AudioFile}\n" +
                              $"AudioLeadIn: {qua.AudioLeadIn}\n" +
                              $"SongPreviewTime: {qua.SongPreviewTime}\n" +
                              $"BackgroundFile: {qua.BackgroundFile}\n" +
                              $"AccuracyStreain: {qua.Judge}\n" +
                              $"Timing Points: {qua.TimingPoints.Count}\n" +
                              $"Slider Velocities: {qua.SliderVelocities.Count}\n" +
                              $"HitObjects: {qua.HitObjects.Count}\n" +
                              $"Parsing Took: {watch.ElapsedMilliseconds}ms to execute.\n" +
                              $"-----------------------------------------\n");*/
        }
    }
}
