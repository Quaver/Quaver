using UnityEngine;
using System;
using Config.Scripts;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// ParseBeatmap Command. Takes in a file path and parses a beatmap.
    /// </summary>
    public static class ParseConfigCommand
    {
        public static readonly string name = "CONFIG";
        public static readonly string description = "Parses a config file and displays the details | Arguments: (filePath)";
        public static readonly string usage = "CONFIG";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "ERROR: You need to specify a file path!";
            }

            Cfg cfg = ConfigParser.Parse(String.Join(" ", args));

            if (cfg.IsValid == false)
            {
                return "ERROR: The specified beatmap could not be found or is not valid.";
            }

			string logString = "SongDirectory: " + cfg.SongDirectory + "\n" + 
								"SkinsDirectory: " + cfg.SkinsDirectory + "\n" + 
								"VolumeGlobal: " + cfg.VolumeGlobal + "\n" + 
								"VolumeEffect: " + cfg.VolumeEffect + "\n" + 
								"VolumeMusic: " + cfg.VolumeMusic + "\n" + 
								"BackgroundDim: " + cfg.BackgroundDim + "\n" + 
								"MonitorDisplay: " + cfg.MonitorDisplay + "\n" + 
								"MonitorRefreshRate: " + cfg.MonitorRefreshRate + "\n" + 
								"WindowHeight: " + cfg.WindowHeight + "\n" + 
								"WindowWidth: " + cfg.WindowWidth + "\n" + 
								"WindowFullScreen: " + cfg.WindowFullScreen + "\n" + 
								"WindowLetterboxed: " + cfg.WindowLetterboxed + "\n" +
								"FullScreenHeight: " + cfg.FullScreenHeight + "\n" + 
								"FullScreenWidth: " + cfg.FullScreenWidth +  "\n" + 
								"CustomFrameLimit: " + cfg.CustomFrameLimit + "\n" +
								"FPSCounter: " + cfg.FPSCounter + "\n" + 
								"FrameTimeDisplay: " + cfg.FrameTimeDisplay + "\n" +
								"Language: " + cfg.Language + "\n" + 
								"QuaverVersion: " + cfg.QuaverVersion + "\n" +
								"QuaverBuildHash: " + cfg.QuaverBuildHash + "\n" +  
								"ScrollSpeed: " + cfg.ScrollSpeed + "\n" + 
								"ScrollSpeedBPMScale: "  + cfg.ScrollSpeedBPMScale + "\n" +
								"DownScroll: " + cfg.DownScroll + "\n" + 
								"GlobalOffset: " + cfg.GlobalOffset + "\n" + 
								"LeaderboardVisible: " + cfg.LeaderboardVisible + "\n" + 
								"Skin: " + cfg.Skin + "\n" +
								"KeyManiaLane1: " + cfg.KeyLaneMania1 + "\n" + 
								"KeyManiaLane2: " + cfg.KeyLaneMania2 + "\n" + 
								"KeyManiaLane3: " + cfg.KeyLaneMania3 + "\n" + 
								"KeyManiaLane4: " + cfg.KeyLaneMania4 + "\n" + 
								"KeyScreenshot: " + cfg.KeyScreenshot + "\n" + 
								"KeyQuickRetry: " + cfg.KeyQuickRetry + "\n" + 
								"KeyIncreaseScrollSpeed: " + cfg.KeyIncreaseScrollSpeed + "\n" +
								"KeyDecreaseScrollSpeed: " + cfg.KeyDecreaseScrollSpeed + "\n" + 
								"KeyPause: " + cfg.KeyPause + "\n" + 
								"KeyVolumeUp: " + cfg.KeyVolumeUp + "\n" + 
								"KeyVolumeDown: " + cfg.KeyVolumeDown + "\n";
			return logString;
            
        }
    }
}