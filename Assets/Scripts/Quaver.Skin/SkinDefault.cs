
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Skin
{
    public static class SkinDefault
    {
        public static string Name = "Default";
        public static string Author = "Staravia";
        public static string Version = "1.0.0";

         // [Menu]
        public static bool CustomBackground = false;

        // [Cursor]
        public static bool CursorRotate = true;
        public static bool CursorTrailRotate = false;
        public static bool CursorExpand = false;
        public static bool CursorCentre = true;

        // [Gameplay]
        public static int BgMaskBufferSize = 12;
        public static int NoteBufferSpacing = 0;
        public static int TimingBarPixelSize = 2;
        public static float HitLightingScale = 4.0f;
        public static int ColumnSize = 250;
        public static int ReceptorYOffset = 50;

        // [Colours]
        public static Color ColourLight1 = new Color(178, 178, 255);
        public static Color ColourLight2 = new Color(255, 255, 178);
        public static Color ColourLight3 = new Color(178, 178, 255);
        public static Color ColourLight4 = new Color(255, 255, 178);
        public static Color Colour1;
        public static Color Colour2;
        public static Color Colour3;
        public static Color Colour4;
        public static Color ColourHold;       
    }
}