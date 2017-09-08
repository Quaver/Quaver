﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Quaver.Config;
using Quaver.Notifications;

namespace Quaver.Main.Screenshots
{
	public static class ScreenshotService
	{
		public static void Capture(Cfg config)
		{
			if (Input.GetKeyDown(config.KeyScreenshot))
			{
				string screenshotPath = config.ScreenshotsDirectory + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
				ScreenCapture.CaptureScreenshot(screenshotPath);

				string notifName = "Test";
				string notifContent = "Screenshot successfully saved at: " + screenshotPath;

				// Send new notification 
				Notification notif = new Notification(notifName, NotificationType.Screenshot, NotificationAction.OpenScreenshot, notifContent);
			}
		}
	}
}
