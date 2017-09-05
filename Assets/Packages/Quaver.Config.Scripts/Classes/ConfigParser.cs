using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Config.Scripts {

	public class ConfigParser
	{

		public static Cfg Parse(string filePath)
		{

            if (!File.Exists(filePath.Trim()))
            {
                Cfg tempCfg = new Cfg();
                tempCfg.IsValid = false;

                return tempCfg;
            }			

			Cfg cfg = new Cfg();
			cfg.IsValid = true;

			foreach(string line in File.ReadAllLines(filePath))
			{
				// Parse the config values, also we need to have defaults just in case some dont exist. and update the file.
				if (line.Contains("="))
				{
					string key = line.Substring(0, line.IndexOf('=')).Trim();
                    string value = line.Split('=').Last().Trim();

					switch(key)
					{
						case "SongDirectory":
							if (value == "")
							{
								cfg.SongDirectory = "Non Existent";
							}
							cfg.SongDirectory = value;
							break;
						case "SkinsDirectory":
							if (value == "")
							{
								cfg.SkinsDirectory = "Non Existent";
							}
							cfg.SkinsDirectory = value;
							break;
						case "VolumeGlobal":
							if (value == "")
							{
								cfg.VolumeGlobal = 100;
							}
							cfg.VolumeGlobal = byte.Parse(value);
							break;	
						case "VolumeEffect":
							if (value == "")
							{
								cfg.VolumeEffect = 100;
							}
							cfg.VolumeEffect = byte.Parse(value);
							break;
						case "VolumeMusic":
							if (value == "")
							{
								cfg.VolumeMusic = 100;
							}
							cfg.VolumeMusic = byte.Parse(value);
							break;							
						case "BackgroundDim":
							if (value == "")
							{
								cfg.BackgroundDim = 0;
							}
							cfg.BackgroundDim = byte.Parse(value);
							break;	
						case "MonitorDisplay":
							if (value == "")
							{
								cfg.MonitorDisplay = 1;
							}
							cfg.MonitorDisplay = byte.Parse(value);
							break;
						case "MonitorRefreshRate":
							if (value == "")
							{
								cfg.MonitorRefreshRate = 60;
							}
							cfg.MonitorRefreshRate = byte.Parse(value);
							break;
						case "WindowHeight":
							if (value == "")
							{
								cfg.WindowHeight = 900;
							}
							cfg.WindowHeight = short.Parse(value);
							break;
						case "WindowWidth":
							if (value == "")
							{
								cfg.WindowWidth = 1600;
							}
							cfg.WindowWidth = short.Parse(value);
							break;
						case "WindowFullScreen":
							if (value == "")
							{
								cfg.WindowFullScreen = true;
							}
							cfg.WindowFullScreen = bool.Parse(value);
							break;
						case "WindowLetterboxed":
							if (value == "")
							{
								cfg.WindowLetterboxed = false;
							}
							cfg.WindowLetterboxed = bool.Parse(value);
							break;
						case "FullScreenHeight":
							if (value == "")
							{
								cfg.FullScreenHeight = 1080;
							}
							cfg.FullScreenHeight = short.Parse(value);
							break;
						case "FullScreenWidth":
							if (value == "")
							{
								cfg.FullScreenWidth = 1920;
							}
							cfg.FullScreenWidth = short.Parse(value);
							break;
						case "CustomFrameLimit":
							if (value == "")
							{
								cfg.CustomFrameLimit = 240;
							}
							cfg.CustomFrameLimit = short.Parse(value);
							break;
						case "FPSCounter":
							if (value == "")
							{
								cfg.FPSCounter = false;
							}
							cfg.FPSCounter = bool.Parse(value);
							break;
						case "FrameTimeDisplay":
							if (value == "")
							{
								cfg.FrameTimeDisplay = false;
							}
							cfg.FrameTimeDisplay = bool.Parse(value);
							break;
						case "Language":
							if (value == "")
							{
								cfg.Language = "en";
							}
							cfg.Language = value;
							break;
						case "QuaverVersion":
							if (value == "")
							{
								cfg.QuaverVersion = "0.1";
							}
							cfg.QuaverVersion = value;
							break;
						case "QuaverBuildHash":
							if (value == "")
							{
								cfg.QuaverBuildHash = "d92d0u8f32dh7c98c9d382h89";
							}
							cfg.QuaverBuildHash = value;
							break;
						case "ScrollSpeed":
							if (value == "")
							{
								cfg.ScrollSpeed = 34;
							}
							cfg.ScrollSpeed = byte.Parse(value);
							break;		
						case "ScrollSpeedBPMScale":
							if (value == "")
							{
								cfg.ScrollSpeedBPMScale = false;
							}
							cfg.ScrollSpeedBPMScale = bool.Parse(value);
							break;
						case "DownScroll":
							if (value == "")
							{
								cfg.DownScroll = false;
							}
							cfg.DownScroll = bool.Parse(value);
							break;
						case "GlobalOffset":
							if (value == "")
							{
								cfg.GlobalOffset = 0;
							}
							cfg.GlobalOffset = byte.Parse(value);
							break;
						case "LeaderboardVisible":
							if (value == "")
							{
								cfg.LeaderboardVisible = false;
							}
							cfg.LeaderboardVisible = bool.Parse(value);
							break;	
						case "Skin":
							if (value == "")
							{
								cfg.Skin = "";
							}
							cfg.Skin = value;
							break;						
						case "KeyManiaLane1":
							if (value == "")
							{
								cfg.KeyLaneMania1 = "d";
							}
							cfg.KeyLaneMania1 = value;
							break;
						case "KeyManiaLane2":
							if (value == "")
							{
								cfg.KeyLaneMania2 = "f";
							}
							cfg.KeyLaneMania2 = value;
							break;
						case "KeyManiaLane3":
							if (value == "")
							{
								cfg.KeyLaneMania3 = "k";
							}
							cfg.KeyLaneMania3 = value;
							break;
						case "KeyManiaLane4":
							if (value == "")
							{
								cfg.KeyLaneMania4 = "k";
							}
							cfg.KeyLaneMania4 = value;
							break;	
						case "KeyScreenshot":
							if (value == "")
							{
								cfg.KeyScreenshot = "f12";
							}
							cfg.KeyScreenshot = value;
							break;	
						case "KeyQuickRetry":
							if (value == "")
							{
								cfg.KeyQuickRetry = "`";
							}
							cfg.KeyQuickRetry = value;
							break;
						case "KeyIncreaseScrollSpeed":
							if (value == "")
							{
								cfg.KeyIncreaseScrollSpeed = "f4";
							}
							cfg.KeyIncreaseScrollSpeed = value;
							break;	
						case "KeyDecreaseScrollSpeed":
							if (value == "")
							{
								cfg.KeyDecreaseScrollSpeed = "f3";
							}
							cfg.KeyDecreaseScrollSpeed = value;
							break;
						case "KeyPause":
							if (value == "")
							{
								cfg.KeyPause = "escape";
							}
							cfg.KeyPause = value;
							break;		
						case "KeyVolumeUp":
							if (value == "")
							{
								cfg.KeyVolumeUp = "up";
							}
							cfg.KeyVolumeUp = value;
							break;
						case "KeyVolumeDown":
							if (value == "")
							{
								cfg.KeyVolumeDown = "down";
							}
							cfg.KeyVolumeDown = value;
							break;																																																							
					}
				}				
			}

			return cfg;
		}

	}

}