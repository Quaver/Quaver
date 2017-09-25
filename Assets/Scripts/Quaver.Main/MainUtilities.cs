using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Quaver.Main
{
	public class MainUtilities
	{
		[DllImport("user32.dll", EntryPoint = "SetWindowText")]
		public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
		
		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

        /// <summary>
        /// Changes the title of the window. This'll be used when changing songs.
        /// </summary>
        /// <param name="newWindowTitle">The window title to switch to.</param>
		public static void ChangeWindowTitle(string newWindowTitle)
		{
			// Only switch if on a Windows Machine. TODO: Figure this out for OS X/Linux
			if (Application.platform == RuntimePlatform.WindowsPlayer)
			{
				// Get the window handle.
				System.IntPtr windowPtr = FindWindow(null, GameStateManager.WindowTitle);

				GameStateManager.WindowTitle = newWindowTitle;

				//Set the title text using the window handle.
				SetWindowText(windowPtr, GameStateManager.WindowTitle);
			}		
		}
	}
}
