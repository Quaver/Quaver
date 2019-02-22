/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Input;

namespace Quaver.Shared.Helpers
{
    public static class XnaKeyHelper
    {
         /// <summary>
        ///     Gets a key from a given string.
        /// </summary>
        /// <param name="keyStr"></param>
        /// <returns></returns>
        internal static Keys GetKeyFromString(string keyStr)
        {
            Keys key;

            switch (keyStr.ToUpper())
            {
                // Letters
                case "Q":
                    key = Keys.Q;
                    return key;
                case "W":
                    key = Keys.W;
                    return key;
                case "E":
                    key = Keys.E;
                    return key;
                case "R":
                    key = Keys.R;
                    return key;
                case "T":
                    key = Keys.T;
                    return key;
                case "Y":
                    key = Keys.Y;
                    return key;
                case "U":
                    key = Keys.U;
                    return key;
                case "I":
                    key = Keys.I;
                    return key;
                case "O":
                    key = Keys.O;
                    return key;
                case "P":
                    key = Keys.P;
                    return key;
                case "A":
                    key = Keys.A;
                    return key;
                case "S":
                    key = Keys.S;
                    return key;
                case "D":
                    key = Keys.D;
                    return key;
                case "F":
                    key = Keys.F;
                    return key;
                case "G":
                    key = Keys.G;
                    return key;
                case "H":
                    key = Keys.H;
                    return key;
                case "J":
                    key = Keys.J;
                    return key;
                case "K":
                    key = Keys.K;
                    return key;
                case "L":
                    key = Keys.L;
                    return key;
                case "Z":
                    key = Keys.Z;
                    return key;
                case "X":
                    key = Keys.X;
                    return key;
                case "C":
                    key = Keys.C;
                    return key;
                case "V":
                    key = Keys.V;
                    return key;
                case "B":
                    key = Keys.B;
                    return key;
                case "N":
                    key = Keys.N;
                    return key;
                case "M":
                    key = Keys.M;
                    return key;

                // TOP ROW F# KEYS ETC.
                case "ESCAPE":
                    key = Keys.Escape;
                    return key;
                case "F1":
                    key = Keys.F1;
                    return key;
                case "F2":
                    key = Keys.F2;
                    return key;
                case "F3":
                    key = Keys.F3;
                    return key;
                case "F4":
                    key = Keys.F4;
                    return key;
                case "F5":
                    key = Keys.F5;
                    return key;
                case "F6":
                    key = Keys.F6;
                    return key;
                case "F7":
                    key = Keys.F7;
                    return key;
                case "F8":
                    key = Keys.F8;
                    return key;
                case "F9":
                    key = Keys.F9;
                    return key;
                case "F10":
                    key = Keys.F10;
                    return key;
                case "F11":
                    key = Keys.F11;
                    return key;
                case "F12":
                    key = Keys.F12;
                    return key;
                case "PRINTSCR":
                    key = Keys.PrintScreen;
                    return key;
                case "SCROLLLOCK":
                    key = Keys.Scroll;
                    return key;
                case "PAUSE":
                    key = Keys.Pause;
                    return key;

                // SECOND ROW NUMBERS, ETC
                case "`":
                    key = Keys.OemTilde;
                    return key;
                case "1":
                    key = Keys.D1;
                    return key;
                case "2":
                    key = Keys.D2;
                    return key;
                case "3":
                    key = Keys.D3;
                    return key;
                case "4":
                    key = Keys.D4;
                    return key;
                case "5":
                    key = Keys.D5;
                    return key;
                case "6":
                    key = Keys.D6;
                    return key;
                case "7":
                    key = Keys.D7;
                    return key;
                case "8":
                    key = Keys.D8;
                    return key;
                case "9":
                    key = Keys.D9;
                    return key;
                case "0":
                    key = Keys.D0;
                    return key;
                case "-":
                    key = Keys.OemMinus;
                    return key;
                case "+":
                    key = Keys.OemPlus;
                    return key;
                case "BACKSPACE":
                    key = Keys.Back;
                    return key;
                case "INSERT":
                    key = Keys.Insert;
                    return key;
                case "HOME":
                    key = Keys.Home;
                    return key;
                case "PGUP":
                    key = Keys.PageUp;
                    return key;
                case "NUMLOCK":
                    key = Keys.NumLock;
                    return key;
                case "/":
                    key = Keys.OemPipe; // <- NEED TO VERIFY
                    return key;
                case "*":
                    key = Keys.Multiply;
                    return key;
                case "TAB":
                    key = Keys.Tab;
                    return key;
                case "ENTER":
                    key = Keys.Enter;
                    return key;
                case "DELETE":
                    key = Keys.Delete;
                    return key;
                case "END":
                    key = Keys.End;
                    return key;
                case "PGDN":
                    key = Keys.PageDown;
                    return key;
                case "CAPS":
                    key = Keys.CapsLock;
                    return key;
                case "LSHIFT":
                    key = Keys.LeftShift;
                    return key;
                case "RSHIFT":
                    key = Keys.RightShift;
                    return key;
                case "LCTRL":
                    key = Keys.LeftControl;
                    return key;
                case "LALT":
                    key = Keys.LeftAlt;
                    return key;
                case "RALT":
                    key = Keys.RightAlt;
                    return key;
                case "RCTRL":
                    key = Keys.RightControl;
                    return key;
                case "SPACE":
                    key = Keys.Space;
                    return key;
                case "KP_INS":
                    key = Keys.NumPad0;
                    return key;
                case "KP_END":
                    key = Keys.NumPad1;
                    return key;
                case "KP_DOWNARROW":
                    key = Keys.NumPad2;
                    return key;
                case "KP_PGDN":
                    key = Keys.NumPad3;
                    return key;
                case "KP_LEFTARROW":
                    key = Keys.NumPad4;
                    return key;
                case "KP_5":
                    key = Keys.NumPad5;
                    return key;
                case "KP_RIGHTARROW":
                    key = Keys.NumPad6;
                    return key;
                case "KP_HOME":
                    key = Keys.NumPad7;
                    return key;
                case "KP_UPARROW":
                    key = Keys.NumPad8;
                    return key;
                case "KP_PGUP":
                    key = Keys.NumPad9;
                    return key;
                case "UP":
                    key = Keys.Up;
                    return key;
                case "DOWN":
                    key = Keys.Down;
                    return key;
                case "LEFT":
                    key = Keys.Left;
                    return key;
                case "RIGHT":
                    key = Keys.Right;
                    return key;
            }

            return Keys.OemQuestion; // <- RETURN QUESTION AS "NO SUCH KEY"
        }

        /// <summary>
        ///     Gets a string from a given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetStringFromKey(Keys key)
        {
            var keyStr = "?";

            switch (key)
            {
                // Letters
                case Keys.Q:
                    keyStr = "Q";
                    return keyStr;
                case Keys.W:
                    keyStr = "W";
                    return keyStr;
                case Keys.E:
                    keyStr = "E";
                    return keyStr;
                case Keys.R:
                    keyStr = "R";
                    return keyStr;
                case Keys.T:
                    keyStr = "T";
                    return keyStr;
                case Keys.Y:
                    keyStr = "Y";
                    return keyStr;
                case Keys.U:
                    keyStr = "U";
                    return keyStr;
                case Keys.I:
                    keyStr = "I";
                    return keyStr;
                case Keys.O:
                    keyStr = "O";
                    return keyStr;
                case Keys.P:
                    keyStr = "P";
                    return keyStr;
                case Keys.A:
                    keyStr = "A";
                    return keyStr;
                case Keys.S:
                    keyStr = "S";
                    return keyStr;
                case Keys.D:
                    keyStr = "D";
                    return keyStr;
                case Keys.F:
                    keyStr = "F";
                    return keyStr;
                case Keys.G:
                    keyStr = "G";
                    return keyStr;
                case Keys.H:
                    keyStr = "H";
                    return keyStr;
                case Keys.J:
                    keyStr = "J";
                    return keyStr;
                case Keys.K:
                    keyStr = "K";
                    return keyStr;
                case Keys.L:
                    keyStr = "L";
                    return keyStr;
                case Keys.Z:
                    keyStr = "Z";
                    return keyStr;
                case Keys.X:
                    keyStr = "X";
                    return keyStr;
                case Keys.C:
                    keyStr = "C";
                    return keyStr;
                case Keys.V:
                    keyStr = "V";
                    return keyStr;
                case Keys.B:
                    keyStr = "B";
                    return keyStr;
                case Keys.N:
                    keyStr = "N";
                    return keyStr;
                case Keys.M:
                    keyStr = "M";
                    return keyStr;

                // TOP ROW F# keyStrS ETC.
                case Keys.Escape:
                    keyStr = "Esc";
                    return keyStr;
                case Keys.F1:
                    keyStr = "F1";
                    return keyStr;
                case Keys.F2:
                    keyStr = "F2";
                    return keyStr;
                case Keys.F3:
                    keyStr = "F3";
                    return keyStr;
                case Keys.F4:
                    keyStr = "F4";
                    return keyStr;
                case Keys.F5:
                    keyStr = "F5";
                    return keyStr;
                case Keys.F6:
                    keyStr = "F6";
                    return keyStr;
                case Keys.F7:
                    keyStr = "F7";
                    return keyStr;
                case Keys.F8:
                    keyStr = "F8";
                    return keyStr;
                case Keys.F9:
                    keyStr = "F9";
                    return keyStr;
                case Keys.F10:
                    keyStr = "F10";
                    return keyStr;
                case Keys.F11:
                    keyStr = "F11";
                    return keyStr;
                case Keys.F12:
                    keyStr = "F12";
                    return keyStr;
                case Keys.PrintScreen:
                    keyStr = "PRTSCR";
                    return keyStr;
                case Keys.Scroll:
                    keyStr = "SCRLK";
                    return keyStr;
                case Keys.Pause:
                    keyStr = "PAUSE";
                    return keyStr;

                // SECOND ROW NUMBERS, ETC
                case Keys.OemTilde:
                    keyStr = "`";
                    return keyStr;
                case Keys.D1:
                    keyStr = "1";
                    return keyStr;
                case Keys.D2:
                    keyStr = "2";
                    return keyStr;
                case Keys.D3:
                    keyStr = "3";
                    return keyStr;
                case Keys.D4:
                    keyStr = "4";
                    return keyStr;
                case Keys.D5:
                    keyStr = "5";
                    return keyStr;
                case Keys.D6:
                    keyStr = "6";
                    return keyStr;
                case Keys.D7:
                    keyStr = "7";
                    return keyStr;
                case Keys.D8:
                    keyStr = "8";
                    return keyStr;
                case Keys.D9:
                    keyStr = "9";
                    return keyStr;
                case Keys.D0:
                    keyStr = "0";
                    return keyStr;
                case Keys.OemMinus:
                    keyStr = "-";
                    return keyStr;
                case Keys.OemPlus:
                    keyStr = "+";
                    return keyStr;
                case Keys.Back:
                    keyStr = "Back";
                    return keyStr;
                case Keys.Insert:
                    keyStr = "INSERT";
                    return keyStr;
                case Keys.Home:
                    keyStr = "HOME";
                    return keyStr;
                case Keys.PageUp:
                    keyStr = "PGUP";
                    return keyStr;
                case Keys.NumLock:
                    keyStr = "NMLK";
                    return keyStr;
                case Keys.OemPipe: // <- NEED TO VERIFY
                    keyStr = "/";
                    return keyStr;
                case Keys.Multiply:
                    keyStr = "*";
                    return keyStr;
                case Keys.Tab:
                    keyStr = "TAB";
                    return keyStr;
                case Keys.Enter:
                    keyStr = "ENT";
                    return keyStr;
                case Keys.Delete:
                    keyStr = "DEL";
                    return keyStr;
                case Keys.End:
                    keyStr = "END";
                    return keyStr;
                case Keys.PageDown:
                    keyStr = "PD";
                    return keyStr;
                case Keys.CapsLock:
                    keyStr = "CAPS";
                    return keyStr;
                case Keys.LeftShift:
                    keyStr = "LSHFT";
                    return keyStr;
                case Keys.RightShift:
                    keyStr = "RSHFT";
                    return keyStr;
                case Keys.LeftControl:
                    keyStr = "LCTRL";
                    return keyStr;
                case Keys.LeftAlt:
                    keyStr = "LALT";
                    return keyStr;
                case Keys.RightAlt:
                    keyStr = "RALT";
                    return keyStr;
                case Keys.RightControl:
                    keyStr = "RCTRL";
                    return keyStr;
                case Keys.Space:
                    keyStr = "Space";
                    return keyStr;
                case Keys.NumPad0:
                    keyStr = "KP_INS";
                    return keyStr;
                case Keys.NumPad1:
                    keyStr = "KP_END";
                    return keyStr;
                case Keys.NumPad2:
                    keyStr = "KP_DOWNARROW";
                    return keyStr;
                case Keys.NumPad3:
                    keyStr = "KP_PGDN";
                    return keyStr;
                case Keys.NumPad4:
                    keyStr = "KP_LEFTARROW";
                    return keyStr;
                case Keys.NumPad5:
                    keyStr = "KP_5";
                    return keyStr;
                case Keys.NumPad6:
                    keyStr = "KP_RIGHTARROW";
                    return keyStr;
                case Keys.NumPad7:
                    keyStr = "KP_HOME";
                    return keyStr;
                case Keys.NumPad8:
                    keyStr = "KP_UPARROW";
                    return keyStr;
                case Keys.NumPad9:
                    keyStr = "KP_PGUP";
                    return keyStr;
                case Keys.Up:
                    keyStr = "UP";
                    return keyStr;
                case Keys.Down:
                    keyStr = "DOWN";
                    return keyStr;
                case Keys.Left:
                    keyStr = "LEFT";
                    return keyStr;
                case Keys.Right:
                    keyStr = "RIGHT";
                    return keyStr;
                case Keys.OemPeriod:
                    return ".";
                case Keys.OemComma:
                    return ",";
                case Keys.OemSemicolon:
                    return ";";
                case Keys.OemQuotes:
                    return "'";
                case Keys.OemCloseBrackets:
                    return "]";
                case Keys.OemOpenBrackets:
                    return "[";
                case Keys.OemQuestion:
                    return "?";
                default:
                    return key.ToString();
            }
        }
    }
}
