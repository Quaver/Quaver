// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using IniParser;
using IniParser.Model;

namespace Quaver.Skin
{
    public class SkinConfigLoader
    {
        // Responsible for return back a parsed skin.ini file.
        // I would seriously suggest reading the example to learn how to use
        // this - https://github.com/rickyah/ini-parser
        public static Skin Load(string path)
        {
            var parser = new FileIniDataParser();

            IniData data = parser.ReadFile(path);

            Skin skin = new Skin();

            // [General]
            skin.Name = String.IsNullOrEmpty(data["General"]["Name"]) ? SkinDefault.Name : data["General"]["Name"];
            skin.Author = String.IsNullOrEmpty(data["General"]["Author"]) ? SkinDefault.Author : data["General"]["Author"];
            skin.Version = String.IsNullOrEmpty(data["General"]["Version"]) ? SkinDefault.Version : data["General"]["Version"];

            // [Menu]
            try 
            {
                skin.CustomBackground = String.IsNullOrEmpty(data["Menu"]["CustomBackground"]) ? SkinDefault.CustomBackground : Boolean.Parse(data["Menu"]["CustomBackground"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.CustomBackground = SkinDefault.CustomBackground;
            }

            // [Cursor]

            // CursorRotate
            try 
            {
                skin.CursorRotate = String.IsNullOrEmpty(data["Cursor"]["CursorRotate"]) ? SkinDefault.CursorRotate : Boolean.Parse(data["Cursor"]["CursorRotate"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.CursorRotate = SkinDefault.CursorRotate;
            }            

            // CursorTrailRotate
            try 
            {
                skin.CursorTrailRotate = String.IsNullOrEmpty(data["Cursor"]["CursorTrailRotate"]) ? SkinDefault.CursorTrailRotate : Boolean.Parse(data["Cursor"]["CursorTrailRotate"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.CursorTrailRotate = SkinDefault.CursorTrailRotate;
            }  

            // CursorExpand
            try 
            {
                skin.CursorExpand = String.IsNullOrEmpty(data["Cursor"]["CursorExpand"]) ? SkinDefault.CursorExpand : Boolean.Parse(data["Cursor"]["CursorExpand"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.CursorExpand = SkinDefault.CursorExpand;
            }  

            // CursorCentre
            try 
            {
                skin.CursorCentre = String.IsNullOrEmpty(data["Cursor"]["CursorCentre"]) ? SkinDefault.CursorCentre : Boolean.Parse(data["Cursor"]["CursorCentre"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.CursorCentre = SkinDefault.CursorCentre;
            }       

            // [Gameplay] 

            // BgMaskBufferSize                 
            try 
            {
                skin.BgMaskBufferSize = String.IsNullOrEmpty(data["Gameplay"]["BgMaskBufferSize"]) ? SkinDefault.BgMaskBufferSize : Int32.Parse(data["Gameplay"]["BgMaskBufferSize"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.BgMaskBufferSize = SkinDefault.BgMaskBufferSize;
            }      

            // NoteBufferSpacing
            try 
            {
                skin.NoteBufferSpacing = String.IsNullOrEmpty(data["Gameplay"]["NoteBufferSpacing"]) ? SkinDefault.NoteBufferSpacing : Int32.Parse(data["Gameplay"]["NoteBufferSpacing"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.NoteBufferSpacing = SkinDefault.NoteBufferSpacing;
            }      

            // TimingBarPixelSize
            try 
            {
                skin.TimingBarPixelSize = String.IsNullOrEmpty(data["Gameplay"]["TimingBarPixelSize"]) ? SkinDefault.TimingBarPixelSize : Int32.Parse(data["Gameplay"]["TimingBarPixelSize"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.TimingBarPixelSize = SkinDefault.TimingBarPixelSize;
            }   

            // HitLightingScale
            try 
            {
                skin.HitLightingScale = String.IsNullOrEmpty(data["Gameplay"]["HitLightingScale"]) ? SkinDefault.HitLightingScale : float.Parse(data["Gameplay"]["HitLightingScale"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.HitLightingScale = SkinDefault.HitLightingScale;
            }  

            // ColumnSize
            try 
            {
                skin.ColumnSize = String.IsNullOrEmpty(data["Gameplay"]["ColumnSize"]) ? SkinDefault.ColumnSize : Int32.Parse(data["Gameplay"]["ColumnSize"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ColumnSize = SkinDefault.ColumnSize;
            }   

            // ReceptorYOffset
            try 
            {
                skin.ReceptorYOffset = String.IsNullOrEmpty(data["Gameplay"]["ReceptorYOffset"]) ? SkinDefault.ReceptorYOffset : Int32.Parse(data["Gameplay"]["ReceptorYOffset"]);
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ReceptorYOffset = SkinDefault.ReceptorYOffset;
            }   

            // [Colours]

            // ColourLight1
            try
            {
                skin.ColourLight1 = String.IsNullOrEmpty(data["Colours"]["ColourLight1"]) ? SkinDefault.ColourLight1 : data["Colours"]["ColourLight1"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ColourLight1 = SkinDefault.ColourLight1;
            }

            // ColourLight2
            try
            {
                skin.ColourLight2 = String.IsNullOrEmpty(data["Colours"]["ColourLight2"]) ? SkinDefault.ColourLight2 : data["Colours"]["ColourLight2"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ColourLight2 = SkinDefault.ColourLight2;
            }

            // ColourLight3
            try
            {
                skin.ColourLight3 = String.IsNullOrEmpty(data["Colours"]["ColourLight3"]) ? SkinDefault.ColourLight3 : data["Colours"]["ColourLight3"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ColourLight3 = SkinDefault.ColourLight3;
            }

            // ColourLight4
            try
            {
                skin.ColourLight4 = String.IsNullOrEmpty(data["Colours"]["ColourLight4"]) ? SkinDefault.ColourLight4 : data["Colours"]["ColourLight4"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ColourLight4 = SkinDefault.ColourLight4;
            }

            // Colour1
            try
            {
                skin.Colour1 = String.IsNullOrEmpty(data["Colours"]["Colour1"]) ? SkinDefault.Colour1 : data["Colours"]["Colour1"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.Colour1 = SkinDefault.Colour1;
            }

            // Colour2
            try
            {
                skin.Colour2 = String.IsNullOrEmpty(data["Colours"]["Colour2"]) ? SkinDefault.Colour2 : data["Colours"]["Colour2"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.Colour2 = SkinDefault.Colour2;
            }

            // Colour3
            try
            {
                skin.Colour3 = String.IsNullOrEmpty(data["Colours"]["Colour3"]) ? SkinDefault.Colour3 : data["Colours"]["Colour3"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.Colour3 = SkinDefault.Colour3;
            }

            // Colour4
            try
            {
                skin.Colour4 = String.IsNullOrEmpty(data["Colours"]["Colour4"]) ? SkinDefault.Colour4 : data["Colours"]["Colour4"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.Colour4 = SkinDefault.Colour4;
            }

            // ColourHold
            try
            {
                skin.ColourHold = String.IsNullOrEmpty(data["Colours"]["ColourHold"]) ? SkinDefault.ColourHold : data["Colours"]["ColourHold"].ToColor();
            } 
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                skin.ColourHold = SkinDefault.ColourHold;
            }

            return skin;
        }
  
    }
}