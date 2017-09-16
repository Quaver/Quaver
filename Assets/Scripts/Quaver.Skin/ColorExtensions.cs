using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Quaver.Skin
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Convert string to Color (if defined as a static property of Color)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color ToColor(this string color)
        {
            string[] colorSplit = color.Split(',');
            return new Color(float.Parse(colorSplit[0]) / 255, float.Parse(colorSplit[1]) / 255, float.Parse(colorSplit[2]) / 255, float.Parse(colorSplit[3]));
        }
    }    
}