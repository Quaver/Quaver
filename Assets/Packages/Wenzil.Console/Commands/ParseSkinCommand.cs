
using UnityEngine;
using System;
using Quaver.Skin;
using IniParser.Model;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// PING Command. A test command to ping the console.
    /// </summary>
    public static class ParseSkinCommand
    {
        public static readonly string name = "SKIN";
        public static readonly string description = "Parses an .ini file used for a skin. | Args: (name)";
        public static readonly string usage = "SKIN";

        public static string Execute(params string[] args)
        {
            if (args.Length > 0)
            {
                string path = String.Join(" ", args);
                Skin skin = SkinConfigLoader.Load(path);

                string skinLog = "[General] \n" +
                                "Name: " + skin.Name + "\n" +
                                "Author: " + skin.Author + "\n" +
                                "Version: " + skin.Version + "\n\n" + 

                                "CustomBackground: " + skin.CustomBackground + "\n\n" + 

                                "CursorRotate: " + skin.CursorRotate + "\n" + 
                                "CursorTrailRotate: " + skin.CursorTrailRotate + "\n" + 
                                "CursorExpand: " + skin.CursorExpand + "\n" +
                                "CursorCentre: " + skin.CursorCentre + "\n\n" +

                                "BgMaskBufferSize: " + skin.BgMaskBufferSize + "\n" + 
                                "NoteBufferSpacing: " + skin.NoteBufferSpacing + "\n" + 
                                "TimingBarPixelSize: " + skin.TimingBarPixelSize + "\n" + 
                                "HitLightingScale: " + skin.HitLightingScale + "\n" + 
                                "ColumnSize: " + skin.ColumnSize + "\n" + 
                                "ReceptorYOffset: " + skin.ReceptorYOffset + "\n\n" +

                                "ColourLight1: " + skin.ColourLight1.ToString() + "\n" +
                                "ColourLight2: " + skin.ColourLight2.ToString() + "\n" +
                                "ColourLight3: " + skin.ColourLight3.ToString() + "\n" +
                                "ColourLight4: " + skin.ColourLight4.ToString() + "\n" +   
                                "Colour1: " + skin.Colour1.ToString() + "\n" + 
                                "Colour2: " + skin.Colour2.ToString() + "\n" + 
                                "Colour3: " + skin.Colour3.ToString() + "\n" + 
                                "Colour4: " + skin.Colour4.ToString() + "\n" +
                                "ColourHold: " + skin.ColourHold.ToString() + "\n";                                                                                                                                                                                                                            
                return skinLog;
            }

            return "Dude, give me the path of a skin.ini please. You're really chopping my eggplant here.";
        }
    }
}