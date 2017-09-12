using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Quaver.Cache;

namespace Quaver.UI
{
	public class BackgroundLoader
	{
		// Responsible for loading a beatmap's background as a sprite.
		public static Sprite Load(CachedBeatmap map)
		{
			try
			{
				string bgPath = map.BackgroundPath.Replace("\"", "");
				Sprite sprite = new Sprite();

				if (File.Exists(bgPath))
				{
					byte[] data = File.ReadAllBytes(bgPath);
					Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
					texture.LoadImage(data);
					sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width/2, texture.height/2));
				}

				return sprite;

			} catch (Exception e)
			{
				Debug.LogException(e);
			}

			return new Sprite();
		}
	}
}