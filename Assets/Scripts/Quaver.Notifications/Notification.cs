using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Notifications
{
    /// <summary>
    /// This class handles everything to do with notifications
	/// It will display a notification and handle its click event.
	///</summary>
	public class Notification
	{
		/// <summary>
		///	The name of the notification.
		/// </summary>
		public string Name;

		/// <summary>
		/// The type of notification defined in the NotificationType enum
		/// </summary>
		public NotificationType Type;
		
		/// <summary>
		/// The action that will take place when a notification is clicked.
		/// </summary>
		public NotificationAction Action;

		/// <summary>
		/// The specific color of the notification for when it will be displayed to
		/// the user.
		/// </summary>
		public Color Color;
		
		/// <summary>
		/// The content or text of the notification.
		/// </summary>
		public string Content;

		/// <summary>
		/// The URL of the notification, if necessary.
		/// </summary>
		public string URL;

		// The constructor, set values for notification.
		public Notification(string name, NotificationType type, NotificationAction action, Color color, string content, string url = "")
		{
			this.Name = name;
			this.Type =  type;
			this.Action = action;
			this.Color = color;
			this.Content = content;
			this.URL = url;
		}		

		// Display the notification with the given details above.
		public void DisplayNotification()
		{
			Debug.Log("<color=green>New Notification Received! Content: " + this.Content + "</color>");
		}

		// Implement later, this'll be the method that defines what to do when a notification is clicked.
		public void ClickHandler()
		{
			switch (this.Action)
			{
				case NotificationAction.Disappear:
					// Make notification disappear
					break;
				case NotificationAction.OpenChat:
					// Open Chat and make notification disappear
					break;
				case NotificationAction.OpenLink:
					// Open a URL
					break;
				case NotificationAction.OpenLog:
					// Open the logs folder
					break;
				case NotificationAction.OpenReplays:
					// Open the replays directory
					break;
				case NotificationAction.OpenScreenshot:
					// Open the screenshots directory
					break;
				default: 
					// Just make it disapepar.
					break;
			}
		}
	}

}