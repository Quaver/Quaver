using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Quaver.Config;

namespace Quaver.Main.Screenshots
{
	public static class ScreenshotService
	{
		public static void Capture(Cfg config)
		{
			if (Input.GetKeyDown(config.KeyScreenshot))
			{
				string screenshotPath = config.ScreenshotsDirectory + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
				Application.CaptureScreenshot(screenshotPath);
				Debug.Log("Screenshot successfully saved at: " + screenshotPath);
			}
		}
	}
}
