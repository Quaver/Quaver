using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.QuaFile;

namespace Quaver.Tests
{
    public static class QuaTest
    {
        [Conditional("DEBUG")]
        public static void ParseQuaTest()
        {
            // Parsing a new Qua for testing purposes. We've specified a preprocessor directive here, 
            // so this'll only run in debug mode.
            var filePath = @"C:\Users\swan\Desktop\Stuff\Git\Quaver2.0\Quaver\Test\Qua\backbeat.qua";
            var qua = new Qua(filePath, false);
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
                              $"HpDrain: {qua.HpDrain}\n" +
                              $"AccuracyStreain: {qua.AccuracyStrain}\n" +
                              $"Timing Points: {qua.TimingPoints.Count}\n" +
                              $"Slider Velocities: {qua.SliderVelocities.Count}\n" +
                              $"HitObjects: {qua.HitObjects.Count}");
        }
    }
}
