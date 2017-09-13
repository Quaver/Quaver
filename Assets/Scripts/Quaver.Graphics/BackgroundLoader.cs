using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Quaver.Cache;
using UnityEngine.UI;

namespace Quaver.Graphics
{
	public class BackgroundLoader
	{
		// Responsible for loading a beatmap's background as a sprite.
		public static void LoadSprite(CachedBeatmap map, GameObject bg, bool xsnap = false, bool ysnap = false)
		{
			try
			{
                string bgPath = map.BackgroundPath.Replace("\"", "");
				Sprite sprite = new Sprite();

				if (File.Exists(bgPath))
				{
					byte[] data = File.ReadAllBytes(bgPath);
					Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, true);
					texture.LoadImage(data);
					sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f,0.5f));
				}
                bg.GetComponent<SpriteRenderer>().sprite = sprite;
                //bg.transform.localScale = Vector3.one * (20f * (100f / (float)sprite.rect.size.y));
                bg.transform.localScale = Vector3.one * ((float)Screen.width / (float)Screen.height) * 20f * (100f / (float)sprite.rect.size.x);

            } catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

        // Responsible for loading a beatmap's background as a texture.
        public static void LoadTexture(CachedBeatmap map, RawImage bg)
        {
            try
            {
                string bgPath = map.BackgroundPath.Replace("\"", "");

                if (File.Exists(bgPath))
                {
                    byte[] data = File.ReadAllBytes(bgPath);
                    Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, true);
                    texture.LoadImage(data);
                    bg.texture = texture;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}