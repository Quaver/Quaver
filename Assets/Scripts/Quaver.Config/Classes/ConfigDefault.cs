using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config {

	public static class ConfigDefault
	{
		public static bool IsValid = false;
		public static string GameDirectory = Application.dataPath;
		public static string ConfigDirectory = Application.dataPath + "/quaver.cfg";
		public static string SongDirectory = GameDirectory + "/Songs/";
		public static string SkinsDirectory = GameDirectory + "/Skins/";
		public static string ScreenshotsDirectory = GameDirectory + "/Screenshots/";
		public static string ReplaysDirectory = GameDirectory + "/Replays/";
		public static string LogsDirectory = GameDirectory + "/Logs/";

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

		public static KeyCode KeyLaneMania1 = KeyCode.D;
		public static KeyCode KeyLaneMania2 = KeyCode.F;
		public static KeyCode KeyLaneMania3 = KeyCode.J;
		public static KeyCode KeyLaneMania4 = KeyCode.K;

		public static KeyCode KeyScreenshot = KeyCode.F12;
		public static KeyCode KeyQuickRetry = KeyCode.BackQuote;
		public static KeyCode KeyIncreaseScrollSpeed = KeyCode.F4;
		public static KeyCode KeyDecreaseScrollSpeed = KeyCode.F5;
		public static KeyCode KeyPause = KeyCode.Escape;
		public static KeyCode KeyVolumeUp = KeyCode.UpArrow;
		public static KeyCode KeyVolumeDown = KeyCode.DownArrow;

	}
}
