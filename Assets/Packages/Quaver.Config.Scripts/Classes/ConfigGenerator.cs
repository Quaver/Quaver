using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;

namespace Config.Scripts {

	public static class ConfigGenerator
	{

		public static bool Generate()
		{
			string configPath = ConfigDefault.GameDirectory + "/quaver.cfg";

			if (File.Exists(configPath))
			{
				Debug.Log("Config File Already Exists!");
				return false;
			}

			// Create a string we'll append to the config file.
			StringBuilder fileString = new StringBuilder();

			fileString.Append("# Quaver Configuration File\n");
			fileString.Append("# Last Updated On = " + DateTime.Today + "\n");
			fileString.Append("GameDirectory = " + ConfigDefault.GameDirectory + "\n");
			fileString.Append("SongDirectory = " + ConfigDefault.SongDirectory + "\n");
			fileString.Append("SkinsDirectory = " + ConfigDefault.SkinsDirectory + "\n");
			fileString.Append("VolumeGlobal = " + ConfigDefault.VolumeGlobal + "\n");
			fileString.Append("VolumeEffect = " + ConfigDefault.VolumeEffect + "\n");		
			fileString.Append("VolumeMusic = " + ConfigDefault.VolumeMusic + "\n");				
			fileString.Append("BackgroundDim = " + ConfigDefault.BackgroundDim + "\n");				
			fileString.Append("WindowHeight = " + ConfigDefault.WindowHeight + "\n");				
			fileString.Append("WindowWidth = " + ConfigDefault.WindowWidth + "\n");				 
			fileString.Append("WindowFullScreen = " + ConfigDefault.WindowFullScreen + "\n");				
			fileString.Append("WindowLetterboxed = " + ConfigDefault.WindowLetterboxed + "\n");					
			fileString.Append("CustomFrameLimit = " + ConfigDefault.CustomFrameLimit + "\n");				
			fileString.Append("FPSCounter = " + ConfigDefault.FPSCounter + "\n");					
			fileString.Append("FrameTimeDisplay = " + ConfigDefault.FrameTimeDisplay + "\n");					
			fileString.Append("Language = " + ConfigDefault.Language + "\n");					
			fileString.Append("QuaverVersion = " + ConfigDefault.QuaverVersion + "\n");					
			fileString.Append("QuaverBuildHash = " + ConfigDefault.QuaverBuildHash + "\n");					
			fileString.Append("ScrollSpeed = " + ConfigDefault.ScrollSpeed + "\n");
			fileString.Append("ScrollSpeedBPMScale = "  + ConfigDefault.ScrollSpeedBPMScale + "\n");
			fileString.Append("DownScroll = " + ConfigDefault.DownScroll + "\n");					
			fileString.Append("GlobalOffset = " + ConfigDefault.GlobalOffset + "\n");				 
			fileString.Append("LeaderboardVisible = " + ConfigDefault.LeaderboardVisible + "\n");				 
			fileString.Append("Skin = " + ConfigDefault.Skin + "\n");
			fileString.Append("KeyManiaLane1 = " + ConfigDefault.KeyLaneMania1 + "\n");
			fileString.Append("KeyManiaLane2 = " + ConfigDefault.KeyLaneMania2 + "\n");
			fileString.Append("KeyManiaLane3 = " + ConfigDefault.KeyLaneMania3 + "\n");
			fileString.Append("KeyManiaLane4 = " + ConfigDefault.KeyLaneMania4 + "\n");					
			fileString.Append("KeyScreenshot = " + ConfigDefault.KeyScreenshot + "\n");					
			fileString.Append("KeyQuickRetry = " + ConfigDefault.KeyQuickRetry + "\n");					
			fileString.Append("KeyIncreaseScrollSpeed = " + ConfigDefault.KeyIncreaseScrollSpeed + "\n");					 
			fileString.Append("KeyDecreaseScrollSpeed = " + ConfigDefault.KeyDecreaseScrollSpeed + "\n");					
			fileString.Append("KeyPause = " + ConfigDefault.KeyPause + "\n");				
			fileString.Append("KeyVolumeUp = " + ConfigDefault.KeyVolumeUp + "\n");					
			fileString.Append("KeyVolumeDown = " + ConfigDefault.KeyVolumeDown + "\n");					

			// Write to config file.
			try 
			{
				StreamWriter file = new StreamWriter(configPath);
				file.AutoFlush = true; 
				Debug.Log(fileString.ToString());
				file.WriteLine(fileString.ToString());
				return true;

			} catch (Exception e) 
			{	
				Debug.Log("Could not generate config file!");
				return false;
			}							
		}
	}

}