using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
				string screenshotPath = config.ScreenshotsDirectory + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
				Application.CaptureScreenshot(screenshotPath);

				string notifName = "Test";
				string notifContent = "Screenshot successfully saved at: " + screenshotPath;

				// Send new notification 
				new Notification(notifName, NotificationType.Screenshot, NotificationAction.OpenScreenshot, notifContent);
			}
		}
	}
}
