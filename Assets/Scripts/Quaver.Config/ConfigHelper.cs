using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config
{
	public class ConfigHelper
	{
		// For the real homies that wanna play with some speed in their life.
		public static Cfg IncreaseScrollSpeed(Cfg userConfig)
		{
			userConfig.ScrollSpeed++;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Speed has been increased by 1. Now: " + userConfig.ScrollSpeed);
			return userConfig;
		}

		// Noobs. Raise it up a bit fam.
		public static Cfg DecreaseScrollSpeed(Cfg userConfig)
		{
			userConfig.ScrollSpeed--;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Speed has been decreased by 1. Now: " + userConfig.ScrollSpeed);
			return userConfig;
		}

		// Ok if you actually use upscroll you're a freak.
		public static Cfg ChangeScrollDirection(Cfg userConfig)
		{
			userConfig.DownScroll = !userConfig.DownScroll;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Direction has been flipped. Downscroll is now: " + userConfig.DownScroll);
			return userConfig;
		}

		// Idk how people can play on some square ass resolution tbh, but do you.
		public static Cfg ChangeResolution(Cfg userConfig, int windowHeight, int windowWidth)
		{
			userConfig.WindowHeight = windowHeight;
			userConfig.WindowWidth = windowWidth;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Resolution hass been changed to: " + userConfig.WindowWidth + "x" + userConfig.WindowHeight);
			return userConfig;
		}

		// Changes the VolumeGlobal variable in user config.
		public static Cfg ChangeVolumeGlobal(Cfg userConfig, byte newVolume)
		{
			userConfig.VolumeGlobal = newVolume;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Global Volume has been changed to: " + userConfig.VolumeGlobal);
			return userConfig;
		}

		// Changes the VolumeEffect variable in user config.
		public static Cfg ChangeVolumeEffect(Cfg userConfig, byte newVolume)
		{
			userConfig.VolumeEffect = newVolume;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Effect volume has been changed to: " + userConfig.VolumeEffect);
			return userConfig;
		} 

		// Changes the VolumeMusic variable in user config.
		public static Cfg ChangeVolumeMusic(Cfg userConfig, byte newVolume)
		{
			userConfig.VolumeMusic = newVolume;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Music volume has been changed to: " + userConfig.VolumeMusic);
			return userConfig;
		}

		// FPSCounter Displayed?
		public static Cfg ChangeFPSCounterDisplay(Cfg userConfig)
		{
			userConfig.FPSCounter = !userConfig.FPSCounter;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] FPSCounter Display has been changed to: " + userConfig.FPSCounter);
			return userConfig;
		}

		// FrameTimeDisplay?
		public static Cfg ChangeFrameTimeDisplay(Cfg userConfig)
		{
			userConfig.FrameTimeDisplay = !userConfig.FrameTimeDisplay;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] FrameTimeDisplay has been changed to: " + userConfig.FrameTimeDisplay);
			return userConfig;
		}

		// Language
		public static Cfg ChangeLanguage(Cfg userConfig, string language)
		{
			// v4l1d4t10n! ok not now but later
			userConfig.Language = language;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Language has been changed to: " + userConfig.Language);
			return userConfig;
		}

		// IF YOUR ACCURACY IS SHIT!!!!
		public static Cfg ChangeGlobalOffset(Cfg userConfig, byte newOffset)
		{
			userConfig.GlobalOffset = newOffset;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Global offset has been changed to: " + userConfig.GlobalOffset);
			return userConfig;
		}

		// DO YOU WANT TO SEE THE LEADERBOARD FAM
		public static Cfg ChangeLeaderboardVisiblity(Cfg userConfig)
		{
			userConfig.LeaderboardVisible = !userConfig.LeaderboardVisible;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] LeaderboardVisibility changed to: " + userConfig.LeaderboardVisible);
			return userConfig;
		}

		// What do you prefer, bars or arrows? I think I'm a bar type of guy tbh. 
		public static Cfg ChangeSkin(Cfg userConfig, string skin)
		{
			userConfig.Skin = skin;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Skin changed to: " + userConfig.Skin);
			return userConfig;
		}

		// OK HERES THE FUN PART BOYS WE GOTTA CHANGE SOME KEYBINDS
		public static Cfg ChangeKeyLane1(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania1 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania1 changed to: " + userConfig.KeyLaneMania1);
			return userConfig;
		}

		public static Cfg ChangeKeyLane2(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania2 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania2 changed to: " + userConfig.KeyLaneMania2);
			return userConfig;
		}

		public static Cfg ChangeKeyLane3(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania3 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania3 changed to: " + userConfig.KeyLaneMania3);
			return userConfig;
		}

		public static Cfg ChangeKeyLane4(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyLaneMania4 = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyLaneMania4 changed to: " + userConfig.KeyLaneMania4);
			return userConfig;
		}

		public static Cfg ChangeKeyScreenshot(Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyScreenshot = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyScreenshot changed to: " + userConfig.KeyScreenshot);
			return userConfig;
		}

		public static Cfg ChangeQuickRetry (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyQuickRetry = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyQuickRetry changed to: " + userConfig.KeyQuickRetry);
			return userConfig;
		}	

		public static Cfg ChangeKeyPause (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyPause = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyPause changed to: " + userConfig.KeyPause);
			return userConfig;
		}

		public static Cfg ChangeKeyVolumeUp (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyVolumeUp = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyVolumeUp changed to: " + userConfig.KeyVolumeUp);
			return userConfig;
		}

		public static Cfg ChangeKey (Cfg userConfig, KeyCode newKey)
		{
			userConfig.KeyVolumeDown = newKey;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] KeyVolumeDown changed to: " + userConfig.KeyVolumeDown);
			return userConfig;
		}												
	}
}