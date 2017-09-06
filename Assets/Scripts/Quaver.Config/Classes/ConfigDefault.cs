using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config {

	public static class ConfigDefault
	{
		public static bool IsValid = false;
		public static string GameDirectory = Application.dataPath;
		public static string ConfigDirectory = Application.dataPath + "/quaver.cfg";
		public static string SongDirectory = GameDirectory + "/songs/";
		public static string SkinsDirectory = GameDirectory + "/skins/";
		public static string ScreenshotsDirectory = GameDirectory + "/screenshots/";
		public static string ReplaysDirectory = GameDirectory + "/replays/";
		public static string LogsDirectory = GameDirectory + "/logs/";

		public static byte VolumeGlobal = 100;
		public static byte VolumeEffect = 50;
		public static byte VolumeMusic = 75;

		public static byte BackgroundDim = 0;

		public static int WindowHeight = Screen.height;
		public static int WindowWidth = Screen.width;
		public static bool WindowFullScreen = false;
		public static bool WindowLetterboxed = false;
		
		public static short CustomFrameLimit = 240;
		public static bool FPSCounter = true;
		public static bool FrameTimeDisplay = false;

		public static string Language = "en";
		public static string QuaverVersion = "db0.0.1";
		public static string QuaverBuildHash = "Not Implemented";

		public static byte ScrollSpeed = 24;
		public static bool ScrollSpeedBPMScale = false;
		public static bool DownScroll = true;

		public static byte GlobalOffset = 0;
		public static bool LeaderboardVisible = false;
		public static string Skin = "";

		public static string KeyLaneMania1 = "d";
		public static string KeyLaneMania2 = "f";
		public static string KeyLaneMania3 = "j";
		public static string KeyLaneMania4 = "k";

		public static string KeyScreenshot = "f12";
		public static string KeyQuickRetry = "`";
		public static string KeyIncreaseScrollSpeed = "f4";
		public static string KeyDecreaseScrollSpeed = "f5";
		public static string KeyPause = "escape";
		public static string KeyVolumeUp = "up";
		public static string KeyVolumeDown = "down";

	}
}
