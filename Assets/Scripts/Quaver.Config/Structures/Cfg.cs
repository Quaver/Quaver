// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config
{
    public struct Cfg
    {
        public bool IsValid;
        public string GameDirectory;
        public string SongDirectory;
        public string SkinsDirectory;
        public string ScreenshotsDirectory;
        public string ReplaysDirectory;
        public string LogsDirectory;

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

        public KeyCode KeyLaneMania1;
        public KeyCode KeyLaneMania2;
        public KeyCode KeyLaneMania3;
        public KeyCode KeyLaneMania4;

        public KeyCode KeyScreenshot;
        public KeyCode KeyQuickRetry;
        public KeyCode KeyIncreaseScrollSpeed;
        public KeyCode KeyDecreaseScrollSpeed;
        public KeyCode KeyPause;
        public KeyCode KeyVolumeUp;
        public KeyCode KeyVolumeDown;

        public bool TimingBars;
    }
}
