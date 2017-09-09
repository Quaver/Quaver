using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Quaver.Config;
using Quaver.Notifications;

namespace Quaver.Screenshot
{
	public static class ScreenshotService
	{
		public static void Capture(Cfg config)
		{
			if (Input.GetKeyDown(config.KeyScreenshot))
			{
				string[] files = Directory.GetFiles(ConfigDefault.ScreenshotsDirectory, "*.png", SearchOption.AllDirectories);

				string screenshotPath = config.ScreenshotsDirectory + DateTime.Now.ToString("yyyy-MM-dd") + " Screenshot" + (files.Length + 1) + ".png";
				ScreenCapture.CaptureScreenshot(screenshotPath);

				string notifName = "Test";
				string notifContent = "Screenshot successfully saved at: " + screenshotPath;

				// Send new notification 
				new Notification(notifName, NotificationType.Screenshot, NotificationAction.OpenScreenshot, notifContent);
			}
		}
	}
}
