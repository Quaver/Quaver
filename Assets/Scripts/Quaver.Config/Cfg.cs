
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config
{
    public struct Cfg
    {
        /// <summary>
        /// Is the config file valid?
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// The directory of the game
        /// </summary>
        public string GameDirectory;
        
        /// <summary>
        /// The directory of the songs
        /// </summary>
        public string SongDirectory;

        /// <summary>
        /// The directory for the skins.
        /// </summary>
        public string SkinsDirectory;

        /// <summary>
        /// The directory for screenshots
        /// </summary>
        public string ScreenshotsDirectory;

        /// <summary>
        /// The directory for replays
        /// </summary>
        public string ReplaysDirectory;

        /// <summary>
        /// The directory for logs.
        /// </summary>
        public string LogsDirectory;

        /// <summary>
        /// The master volume
        /// </summary>
        public byte VolumeGlobal;

        /// <summary>
        /// The effect volume
        /// </summary>
        public byte VolumeEffect;

        /// <summary>
        /// The music volume
        /// </summary>
        public byte VolumeMusic;

        /// <summary>
        /// The amount of dim for the song backgrounds
        /// </summary>
        public byte BackgroundDim;

        /// <summary>
        /// The window height
        /// </summary>
        public int WindowHeight;

        /// <summary>
        /// The window width
        /// </summary>
        public int WindowWidth;

        /// <summary>
        /// Is the window fullscreen?
        /// </summary>
        public bool WindowFullScreen;

        /// <summary>
        /// Window letterboxing
        /// </summary>
        public bool WindowLetterboxed;

        /// <summary>
        /// The custom frame limit
        /// </summary>
        public short CustomFrameLimit;

        /// <summary>
        /// Toggle FPS display
        /// </summary>
        public bool FPSCounter;

        /// <summary>
        /// Toggle for FrameTimeDisplay
        /// </summary>
        public bool FrameTimeDisplay;

        /// <summary>
        /// The language of the game
        /// </summary>
        public string Language;

        /// <summary>
        /// The version of the game
        /// </summary>
        public string QuaverVersion;

        /// <summary>
        /// The build hash of the game
        /// </summary>
        public string QuaverBuildHash;

        /// <summary>
        /// The scroll speed
        /// </summary>
        public byte ScrollSpeed;

        /// <summary>
        /// Toggle scroll speed scaling by BPM
        /// </summary>
        public bool ScrollSpeedBPMScale;

        /// <summary>
        /// Scroll direction
        /// </summary>
        public bool DownScroll;

        /// <summary>
        /// The offset of the notes
        /// </summary>
        public byte GlobalOffset;
        
        /// <summary>
        /// Toggle LeaderboardVisiblity
        /// </summary>
        public bool LeaderboardVisible;

        /// <summary>
        /// The loaded Skin
        /// </summary>
        public string Skin;

        /// <summary>
        /// The key for lane 1
        /// </summary>
        public KeyCode KeyLaneMania1;
        /// <summary>
        /// The key for lane 2
        /// </summary>
        public KeyCode KeyLaneMania2;
        /// <summary>
        /// The key for lane 3
        /// </summary>
        public KeyCode KeyLaneMania3;
        /// <summary>
        /// The key for lane 4
        /// </summary>
        public KeyCode KeyLaneMania4;

        /// <summary>
        /// The key for taking a screenshot
        /// </summary>
        public KeyCode KeyScreenshot;

        /// <summary>
        /// The key for quick retrying a map.
        /// </summary>
        public KeyCode KeyQuickRetry;

        /// <summary>
        /// The key for increasing the scroll speed by 1
        /// </summary>
        public KeyCode KeyIncreaseScrollSpeed;

        /// <summary>
        /// The key for decreasing the scroll speed by 1
        /// </summary>
        public KeyCode KeyDecreaseScrollSpeed;

        /// <summary>
        /// The key for pausing the game
        /// </summary>
        public KeyCode KeyPause;

        /// <summary>
        /// The key for turning the volume up 
        /// </summary>
        public KeyCode KeyVolumeUp;

        /// <summary>
        /// The key for turning the volume down 
        /// </summary>
        public KeyCode KeyVolumeDown;

        /// <summary>
        /// Toggle for if the user wants to see timingbars during gameplay 
        /// </summary>
        public bool TimingBars;
    }
}
