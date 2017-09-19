using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Skin
{
    public struct Skin
    {
        // [General]
        public string Name;
        public string Author;
        public string Version;

        // [Menu]
        public bool CustomBackground;

        // [Cursor]
        public bool CursorRotate;
        public bool CursorTrailRotate;
        public bool CursorExpand;
        public bool CursorCentre;

        // [Gameplay]
        public int BgMaskBufferSize;
        public int NoteBufferSpacing;
        public int TimingBarPixelSize;
        public float HitLightingScale;
        public int ColumnSize;
        public int ReceptorYOffset;

        // [Colours]
        public Color ColourLight1;
        public Color ColourLight2;
        public Color ColourLight3;
        public Color ColourLight4;
        public Color Colour1;
        public Color Colour2;
        public Color Colour3;
        public Color Colour4;
        public Color ColourHold;

    }
}