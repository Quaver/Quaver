using UnityEngine;
using System;
using Quaver.Osu.Beatmap;
using Quaver.Qua;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class CalculateDifficultyCommand
    {
        public static readonly string name = "DIFF";
        public static readonly string description = "Calculates a beatmap's difficulty and returns the difficulty values. | Arguments: (filePath)";
        public static readonly string usage = "DIFF";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "ERROR: You need to specify a file path!";
            }

            QuaFile qFile = QuaParser.Parse(String.Join(" ", args));

            if (qFile.IsValidQua == false)
            {
                return "ERROR: The specified beatmap could not be found or is not valid.";
            }
            else
            {
                Difficulty CalculatedDifficulty = DifficultyCalculator.CalculateDifficulty(qFile.HitObjects);
                string beatmapLogData = "----------- Beatmap Difficulty Stats -----------\n" +
                    "Beatmap: "+qFile.Artist+" - "+qFile.Title+", " + qFile.DifficultyName+ "\n" +
                    "STAR DIFF: " + CalculatedDifficulty.StarDifficulty + "\n" +
                    "Ctrl: " + CalculatedDifficulty.ControlStrain + " | " +
                    "Jack: " + CalculatedDifficulty.JackStrain + " | " +
                    "Sped: " + CalculatedDifficulty.SpeedStrain + " | " +
                    "Stam: " + CalculatedDifficulty.StaminaStrain + " | " +
                    "Tech: " + CalculatedDifficulty.TechStrain + "\n" +
                    "Average NPS: " + CalculatedDifficulty.AverageNPS + "\n" +
                    "     NPS INTERVAL LIST:";

                for (int i=0; i < CalculatedDifficulty.npsInterval.Length; i++)
                {
                    if (i%50 == 0) beatmapLogData = beatmapLogData + "\n "+i+" |    ";
                    beatmapLogData = beatmapLogData + CalculatedDifficulty.npsInterval[i] + "/";
                }
                return beatmapLogData;

            }
            
        }
    }
}