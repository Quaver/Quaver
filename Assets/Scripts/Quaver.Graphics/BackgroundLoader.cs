// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

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
        private static GameObject s_bgSpriteObj; //will remove the old bg image buffer if one already exists


        // Responsible for loading a beatmap's background as a sprite.
        public static void LoadSprite(CachedBeatmap map, GameObject bg)
        {
            string bgPath = map.BackgroundPath.Replace("\"", "");
            if (File.Exists(bgPath))
            {
                if (s_bgSpriteObj != null)
                    GameObject.Destroy(s_bgSpriteObj);

                bg.GetComponent<BackgroundDimAnimator>().dim = 0;
                s_bgSpriteObj = new GameObject("BG Image Buffer");
                s_bgSpriteObj.AddComponent<BackgroundBufferer>().init(true, bgPath, null, bg);
            }
        }

        // Responsible for loading a beatmap's background as a texture.
        public static void LoadTexture(CachedBeatmap map, RawImage bg)
        {
            string bgPath = map.BackgroundPath.Replace("\"", "");
            if (File.Exists(bgPath))
            {
                GameObject obj = new GameObject("UI Image Buffer");
                obj.AddComponent<BackgroundBufferer>().init(false, bgPath, bg);
            }
        }
    }
}