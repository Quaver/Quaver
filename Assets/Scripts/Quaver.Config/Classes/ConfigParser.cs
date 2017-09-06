using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Quaver.Config {

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
						case "GameDirectory":
							if (value == "")
							{
								cfg.GameDirectory = ConfigDefault.GameDirectory;
								continue;
							}
							cfg.GameDirectory = value;
							break;						
						case "SongDirectory":
							if (value == "")
							{
								cfg.SongDirectory = ConfigDefault.SongDirectory;
								continue;
							}
							cfg.SongDirectory = value;
							break;
						case "SkinsDirectory":
							if (value == "")
							{
								cfg.SkinsDirectory = ConfigDefault.SkinsDirectory;
								continue;
							}
							cfg.SkinsDirectory = value;
							break;
						case "ScreenshotsDirectory":
							if (value == "")
							{
								cfg.ScreenshotsDirectory = ConfigDefault.ScreenshotsDirectory;
								continue;
							}
							cfg.ScreenshotsDirectory = value;
							break;
						case "ReplaysDirectory":
							if (value == "")
							{
								cfg.ReplaysDirectory = ConfigDefault.ReplaysDirectory;
								continue;
							}
							cfg.ReplaysDirectory = value;
							break;
						case "LogsDirectory":
							if (value == "")
							{
								cfg.LogsDirectory = ConfigDefault.LogsDirectory;
								continue;
							}
							cfg.LogsDirectory = value;
							break;																					
						case "VolumeGlobal":
							if (value == "")
							{
								cfg.VolumeGlobal = ConfigDefault.VolumeGlobal;
								continue;
							}
							cfg.VolumeGlobal = byte.Parse(value);
							break;	
						case "VolumeEffect":
							if (value == "")
							{
								cfg.VolumeEffect = ConfigDefault.VolumeEffect;
								continue;
							}
							cfg.VolumeEffect = byte.Parse(value);
							break;
						case "VolumeMusic":
							if (value == "")
							{
								cfg.VolumeMusic = ConfigDefault.VolumeMusic;
								continue;
							}
							cfg.VolumeMusic = byte.Parse(value);
							break;							
						case "BackgroundDim":
							if (value == "")
							{
								cfg.BackgroundDim = ConfigDefault.BackgroundDim;
								continue;
							}
							cfg.BackgroundDim = byte.Parse(value);
							break;	
						case "WindowHeight":
							if (value == "")
							{
								cfg.WindowHeight = ConfigDefault.WindowHeight;
								continue;
							}
							cfg.WindowHeight = short.Parse(value);
							break;
						case "WindowWidth":
							if (value == "")
							{
								cfg.WindowWidth = ConfigDefault.WindowWidth;
								continue;
							}
							cfg.WindowWidth = short.Parse(value);
							break;
						case "WindowFullScreen":
							if (value == "")
							{
								cfg.WindowFullScreen = ConfigDefault.WindowFullScreen;
								continue;
							}
							cfg.WindowFullScreen = bool.Parse(value);
							break;
						case "WindowLetterboxed":
							if (value == "")
							{
								cfg.WindowLetterboxed = ConfigDefault.WindowLetterboxed;
								continue;
							}
							cfg.WindowLetterboxed = bool.Parse(value);
							break;
						case "CustomFrameLimit":
							if (value == "")
							{
								cfg.CustomFrameLimit = ConfigDefault.CustomFrameLimit;
								continue;
							}
							cfg.CustomFrameLimit = short.Parse(value);
							break;
						case "FPSCounter":
							if (value == "")
							{
								cfg.FPSCounter = ConfigDefault.FPSCounter;
								continue;
							}
							cfg.FPSCounter = bool.Parse(value);
							break;
						case "FrameTimeDisplay":
							if (value == "")
							{
								cfg.FrameTimeDisplay = ConfigDefault.FrameTimeDisplay;
								continue;
							}
							cfg.FrameTimeDisplay = bool.Parse(value);
							break;
						case "Language":
							if (value == "")
							{
								cfg.Language = ConfigDefault.Language;
								continue;
							}
							cfg.Language = value;
							break;
						case "QuaverVersion":
							if (value == "")
							{
								cfg.QuaverVersion = ConfigDefault.QuaverVersion;
								continue;
							}
							cfg.QuaverVersion = value;
							break;
						case "QuaverBuildHash":
							if (value == "")
							{
								cfg.QuaverBuildHash = ConfigDefault.QuaverBuildHash;
								continue;
							}
							cfg.QuaverBuildHash = value;
							break;
						case "ScrollSpeed":
							if (value == "")
							{
								cfg.ScrollSpeed = ConfigDefault.ScrollSpeed;
								continue;
							}
							cfg.ScrollSpeed = byte.Parse(value);
							break;		
						case "ScrollSpeedBPMScale":
							if (value == "")
							{
								cfg.ScrollSpeedBPMScale = ConfigDefault.ScrollSpeedBPMScale;
								continue;
							}
							cfg.ScrollSpeedBPMScale = bool.Parse(value);
							break;
						case "DownScroll":
							if (value == "")
							{
								cfg.DownScroll = ConfigDefault.DownScroll;
								continue;
							}
							cfg.DownScroll = bool.Parse(value);
							break;
						case "GlobalOffset":
							if (value == "")
							{
								cfg.GlobalOffset = ConfigDefault.GlobalOffset;
								continue;
							}
							cfg.GlobalOffset = byte.Parse(value);
							break;
						case "LeaderboardVisible":
							if (value == "")
							{
								cfg.LeaderboardVisible = ConfigDefault.LeaderboardVisible;
								continue;
							}
							cfg.LeaderboardVisible = bool.Parse(value);
							break;	
						case "Skin":
							if (value == "")
							{
								cfg.Skin = ConfigDefault.Skin;
								continue;
							}
							cfg.Skin = value;
							break;						
						case "KeyManiaLane1":
							if (value == "")
							{
								cfg.KeyLaneMania1 = ConfigDefault.KeyLaneMania1;
								continue;
							}
							cfg.KeyLaneMania1 = value;
							break;
						case "KeyManiaLane2":
							if (value == "")
							{
								cfg.KeyLaneMania2 = ConfigDefault.KeyLaneMania2;
								continue;
							}
							cfg.KeyLaneMania2 = value;
							break;
						case "KeyManiaLane3":
							if (value == "")
							{
								cfg.KeyLaneMania3 = ConfigDefault.KeyLaneMania3;
								continue;
							}
							cfg.KeyLaneMania3 = value;
							break;
						case "KeyManiaLane4":
							if (value == "")
							{
								cfg.KeyLaneMania4 = ConfigDefault.KeyLaneMania4;
								continue;
							}
							cfg.KeyLaneMania4 = value;
							break;	
						case "KeyScreenshot":
							if (value == "")
							{
								cfg.KeyScreenshot = ConfigDefault.KeyScreenshot;
								continue;
							}
							cfg.KeyScreenshot = value;
							break;	
						case "KeyQuickRetry":
							if (value == "")
							{
								cfg.KeyQuickRetry = ConfigDefault.KeyQuickRetry;
								continue;
							}
							cfg.KeyQuickRetry = value;
							break;
						case "KeyIncreaseScrollSpeed":
							if (value == "")
							{
								cfg.KeyIncreaseScrollSpeed = ConfigDefault.KeyIncreaseScrollSpeed;
								continue;
							}
							cfg.KeyIncreaseScrollSpeed = value;
							break;	
						case "KeyDecreaseScrollSpeed":
							if (value == "")
							{
								cfg.KeyDecreaseScrollSpeed = ConfigDefault.KeyDecreaseScrollSpeed;
								continue;
							}
							cfg.KeyDecreaseScrollSpeed = value;
							break;
						case "KeyPause":
							if (value == "")
							{
								cfg.KeyPause = ConfigDefault.KeyPause;
								continue;
							}
							cfg.KeyPause = value;
							break;		
						case "KeyVolumeUp":
							if (value == "")
							{
								cfg.KeyVolumeUp = ConfigDefault.KeyVolumeUp;
								continue;
							}
							cfg.KeyVolumeUp = value;
							break;
						case "KeyVolumeDown":
							if (value == "")
							{
								cfg.KeyVolumeDown = ConfigDefault.KeyVolumeDown;
								continue;
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