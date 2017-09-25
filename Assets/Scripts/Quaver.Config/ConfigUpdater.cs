
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;

namespace Quaver.Config
{
    public static class ConfigUpdater
    {
        /// <summary>
        /// Updates a quaver.cfg file with all the updated values in the Cfg object.
        /// </summary>
        /// <param name="configFile"></param>
        public static void Update(Cfg configFile)
        {
            string outputPath = Application.dataPath + "/quaver.cfg";

            // Create a new string w/ Cfg Values
            StringBuilder fileString = new StringBuilder();

            fileString.Append("# Quaver Configuration File\n");
            fileString.Append("# Last Updated On = " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
            fileString.Append("SongDirectory = " + configFile.SongDirectory + "\n");
            fileString.Append("SkinsDirectory = " + configFile.SkinsDirectory + "\n");
            fileString.Append("ScreenshotsDirectory = " + configFile.ScreenshotsDirectory + "\n");
            fileString.Append("ReplaysDirectory = " + configFile.ReplaysDirectory + "\n");
            fileString.Append("LogsDirectory = " + configFile.LogsDirectory + "\n");
            fileString.Append("VolumeGlobal = " + configFile.VolumeGlobal + "\n");
            fileString.Append("VolumeEffect = " + configFile.VolumeEffect + "\n");
            fileString.Append("VolumeMusic = " + configFile.VolumeMusic + "\n");
            fileString.Append("BackgroundDim = " + configFile.BackgroundDim + "\n");
            fileString.Append("WindowHeight = " + configFile.WindowHeight + "\n");
            fileString.Append("WindowWidth = " + configFile.WindowWidth + "\n");
            fileString.Append("WindowFullScreen = " + configFile.WindowFullScreen + "\n");
            fileString.Append("WindowLetterboxed = " + configFile.WindowLetterboxed + "\n");
            fileString.Append("CustomFrameLimit = " + configFile.CustomFrameLimit + "\n");
            fileString.Append("FPSCounter = " + configFile.FPSCounter + "\n");
            fileString.Append("FrameTimeDisplay = " + configFile.FrameTimeDisplay + "\n");
            fileString.Append("Language = " + configFile.Language + "\n");
            fileString.Append("QuaverVersion = " + configFile.QuaverVersion + "\n");
            fileString.Append("QuaverBuildHash = " + configFile.QuaverBuildHash + "\n");
            fileString.Append("ScrollSpeed = " + configFile.ScrollSpeed + "\n");
            fileString.Append("ScrollSpeedBPMScale = " + configFile.ScrollSpeedBPMScale + "\n");
            fileString.Append("DownScroll = " + configFile.DownScroll + "\n");
            fileString.Append("GlobalOffset = " + configFile.GlobalOffset + "\n");
            fileString.Append("LeaderboardVisible = " + configFile.LeaderboardVisible + "\n");
            fileString.Append("Skin = " + configFile.Skin + "\n");
            fileString.Append("KeyManiaLane1 = " + configFile.KeyLaneMania1 + "\n");
            fileString.Append("KeyManiaLane2 = " + configFile.KeyLaneMania2 + "\n");
            fileString.Append("KeyManiaLane3 = " + configFile.KeyLaneMania3 + "\n");
            fileString.Append("KeyManiaLane4 = " + configFile.KeyLaneMania4 + "\n");
            fileString.Append("KeyScreenshot = " + configFile.KeyScreenshot + "\n");
            fileString.Append("KeyQuickRetry = " + configFile.KeyQuickRetry + "\n");
            fileString.Append("KeyIncreaseScrollSpeed = " + configFile.KeyIncreaseScrollSpeed + "\n");
            fileString.Append("KeyDecreaseScrollSpeed = " + configFile.KeyDecreaseScrollSpeed + "\n");
            fileString.Append("KeyPause = " + configFile.KeyPause + "\n");
            fileString.Append("KeyVolumeUp = " + configFile.KeyVolumeUp + "\n");
            fileString.Append("KeyVolumeDown = " + configFile.KeyVolumeDown + "\n");
            fileString.Append("TimingBars = " + configFile.TimingBars + "\n");

            try
            {
                StreamWriter file = new StreamWriter(outputPath);
                file.AutoFlush = true;
                file.WriteLine(fileString.ToString());
                Debug.Log("[CONFIG] Config file successfully updated!\n" + fileString);
                file.Close();
            }
            catch (Exception e)
            {
                Debug.LogError("[CONFIG] Could not generate config file!");
                Debug.Log(e);
            }
        }
    }
}