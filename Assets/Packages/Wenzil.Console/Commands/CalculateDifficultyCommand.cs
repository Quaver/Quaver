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
        public static readonly string name = "DIFFICULTY";
        public static readonly string description = "Calculates a beatmap's difficulty and returns the difficulty values. | Arguments: (filePath)";
        public static readonly string usage = "DIFFICULTY";

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
                    "Star Difficulty: " + CalculatedDifficulty.StarDifficulty + "\n" +
                    "ControlS train: " + CalculatedDifficulty.ControlStrain + "\n" +
                    "Jack Strain: " + CalculatedDifficulty.JackStrain + "\n" +
                    "Speed Strain: " + CalculatedDifficulty.SpeedStrain + "\n" +
                    "Stamina Strain: " + CalculatedDifficulty.StaminaStrain + "\n" +
                    "Tech Strain: " + CalculatedDifficulty.TechStrain + "\n";
                return beatmapLogData;
            }
            
        }
    }
}