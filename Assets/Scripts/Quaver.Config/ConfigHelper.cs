using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config
{
	public class ConfigHelper
	{
		/// <summary>
        /// Increases the scroll speed in the config file + config object
        /// </summary>
        /// <param name="userConfig"></param>
        /// <returns></returns>
		public static Cfg IncreaseScrollSpeed(Cfg userConfig)
		{
			userConfig.ScrollSpeed++;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Speed has been increased by 1. Now: " + userConfig.ScrollSpeed);
			return userConfig;
		}

		/// <summary>
        /// Decreases the scroll speed.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <returns></returns>
		public static Cfg DecreaseScrollSpeed(Cfg userConfig)
		{
			userConfig.ScrollSpeed--;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Speed has been decreased by 1. Now: " + userConfig.ScrollSpeed);
			return userConfig;
		}

		/// <summary>
        /// Changes the scroll direction.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <returns></returns>
		public static Cfg ChangeScrollDirection(Cfg userConfig)
		{
			userConfig.DownScroll = !userConfig.DownScroll;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Direction has been flipped. Downscroll is now: " + userConfig.DownScroll);
			return userConfig;
		}

		/// <summary>
        /// Changes the resolution
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="windowHeight">The new height</param>
        /// <param name="windowWidth">The new width</param>
        /// <returns></returns>
		public static Cfg ChangeResolution(Cfg userConfig, int windowHeight, int windowWidth)
		{
			userConfig.WindowHeight = windowHeight;
			userConfig.WindowWidth = windowWidth;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Resolution hass been changed to: " + userConfig.WindowWidth + "x" + userConfig.WindowHeight);
			return userConfig;
		}

		/// <summary>
        /// Changes the master volume
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newVolume"></param>
        /// <returns></returns>
		public static Cfg ChangeVolumeGlobal(Cfg userConfig, byte newVolume)
		{
			userConfig.VolumeGlobal = newVolume;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Global Volume has been changed to: " + userConfig.VolumeGlobal);
			return userConfig;
		}

		/// <summary>
        /// Changes the effect volume.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newVolume"></param>
        /// <returns></returns>
		public static Cfg ChangeVolumeEffect(Cfg userConfig, byte newVolume)
		{
			userConfig.VolumeEffect = newVolume;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Effect volume has been changed to: " + userConfig.VolumeEffect);
			return userConfig;
		} 

		/// <summary>
        /// Changes the music volume.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newVolume"></param>
        /// <returns></returns>
		public static Cfg ChangeVolumeMusic(Cfg userConfig, byte newVolume)
		{
			userConfig.VolumeMusic = newVolume;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Music volume has been changed to: " + userConfig.VolumeMusic);
			return userConfig;
		}

		/// <summary>
        /// Changes the FPSCounterDisplay bool.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <returns></returns>
		public static Cfg ChangeFPSCounterDisplay(Cfg userConfig)
		{
			userConfig.FPSCounter = !userConfig.FPSCounter;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] FPSCounter Display has been changed to: " + userConfig.FPSCounter);
			return userConfig;
		}

		/// <summary>
        /// Changes the FrameTimeDisplay bool.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <returns></returns>
		public static Cfg ChangeFrameTimeDisplay(Cfg userConfig)
		{
			userConfig.FrameTimeDisplay = !userConfig.FrameTimeDisplay;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] FrameTimeDisplay has been changed to: " + userConfig.FrameTimeDisplay);
			return userConfig;
		}

		/// <summary>
        /// Change the language of the game.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="language"></param>
        /// <returns></returns>
		public static Cfg ChangeLanguage(Cfg userConfig, string language)
		{
			// v4l1d4t10n! ok not now but later
			userConfig.Language = language;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Language has been changed to: " + userConfig.Language);
			return userConfig;
		}

		/// <summary>
        /// Change the global offset of the notes.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newOffset"></param>
        /// <returns></returns>
		public static Cfg ChangeGlobalOffset(Cfg userConfig, byte newOffset)
		{
			userConfig.GlobalOffset = newOffset;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Global offset has been changed to: " + userConfig.GlobalOffset);
			return userConfig;
		}

		/// <summary>
        /// Toggle leaderboard visibilty
        /// </summary>
        /// <param name="userConfig"></param>
        /// <returns></returns>
		public static Cfg ChangeLeaderboardVisiblity(Cfg userConfig)
		{
			userConfig.LeaderboardVisible = !userConfig.LeaderboardVisible;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] LeaderboardVisibility changed to: " + userConfig.LeaderboardVisible);
			return userConfig;
		}

		/// <summary>
        /// Change the user defined skin.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="skin"></param>
        /// <returns></returns>
		public static Cfg ChangeSkin(Cfg userConfig, string skin)
		{
			userConfig.Skin = skin;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Skin changed to: " + userConfig.Skin);
			return userConfig;
		}

		/// <summary>
        /// Change the key pressed for lane 1
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
		public static Cfg ChangeKeyLane1(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania1 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania1 changed to: " + userConfig.KeyLaneMania1);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed for lane 2
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public static Cfg ChangeKeyLane2(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania2 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania2 changed to: " + userConfig.KeyLaneMania2);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed for lane 3
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public static Cfg ChangeKeyLane3(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania3 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania3 changed to: " + userConfig.KeyLaneMania3);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed for lane 4
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public static Cfg ChangeKeyLane4(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania4 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania4 changed to: " + userConfig.KeyLaneMania4);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed to take a screenshot
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
		public static Cfg ChangeKeyScreenshot(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyScreenshot = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyScreenshot changed to: " + userConfig.KeyScreenshot);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed to quick retry the map.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
		public static Cfg ChangeQuickRetry (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyQuickRetry = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyQuickRetry changed to: " + userConfig.KeyQuickRetry);
			return userConfig;
		}	

        /// <summary>
        /// Change the key pressed to pause the map.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
		public static Cfg ChangeKeyPause (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyPause = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyPause changed to: " + userConfig.KeyPause);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed to change the volume up
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
		public static Cfg ChangeKeyVolumeUp (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyVolumeUp = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyVolumeUp changed to: " + userConfig.KeyVolumeUp);
			return userConfig;
		}

        /// <summary>
        /// Change the key pressed to change the volume down.
        /// </summary>
        /// <param name="userConfig"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
		public static Cfg ChangeKey (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyVolumeDown = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyVolumeDown changed to: " + userConfig.KeyVolumeDown);
			return userConfig;
		}												
	}
}