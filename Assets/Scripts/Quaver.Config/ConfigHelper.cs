using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Config
{
	public class ConfigHelper
	{
		// Helper method to increase a player's scroll speed.
		public static Cfg IncreaseScrollSpeed(Cfg userConfig)
		{
			userConfig.ScrollSpeed++;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Speed has been increased by 1. Now: " + userConfig.ScrollSpeed);
			return userConfig;
		}

		// Helper method to decrease a player's scroll speed.
		public static Cfg DecreaseScrollSpeed(Cfg userConfig)
		{
			userConfig.ScrollSpeed--;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Speed has been decreased by 1. Now: " + userConfig.ScrollSpeed);
			return userConfig;
		}

		// Helper method to change a player's scroll direction
		public static Cfg ChangeScrollDirection(Cfg userConfig)
		{
			userConfig.DownScroll = !userConfig.DownScroll;
			ConfigUpdater.Update(userConfig);
			Debug.Log("[CONFIG] Scroll Direction has been flipped. Downscroll is now: " + userConfig.DownScroll);
			return userConfig;
		}
	}
}