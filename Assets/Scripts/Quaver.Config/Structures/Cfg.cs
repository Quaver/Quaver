using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config {

	public struct Cfg
	{
		public bool IsValid;
		public string GameDirectory;
		public string SongDirectory;
		public string SkinsDirectory;

		public byte VolumeGlobal;
		public byte VolumeEffect;
		public byte VolumeMusic;

		public byte BackgroundDim;

		public int WindowHeight;
		public int WindowWidth;
		public bool WindowFullScreen;
		public bool WindowLetterboxed;

		public short CustomFrameLimit;
		public bool FPSCounter;
		public bool FrameTimeDisplay;

		public string Language;
		public string QuaverVersion;
		public string QuaverBuildHash;

		public byte ScrollSpeed;
		public bool ScrollSpeedBPMScale;
		public bool DownScroll;

		public byte GlobalOffset;
		public bool LeaderboardVisible;
		public string Skin;

		public string KeyLaneMania1;
		public string KeyLaneMania2;
		public string KeyLaneMania3;
		public string KeyLaneMania4;

		public string KeyScreenshot;
		public string KeyQuickRetry;
		public string KeyIncreaseScrollSpeed;
		public string KeyDecreaseScrollSpeed;
		public string KeyPause;
		public string KeyVolumeUp;
		public string KeyVolumeDown;
	}

}
