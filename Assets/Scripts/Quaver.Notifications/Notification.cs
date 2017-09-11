using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		private string Name;

		/// <summary>
		/// The type of notification defined in the NotificationType enum
		/// </summary>
		private NotificationType Type;
		
		/// <summary>
		/// The action that will take place when a notification is clicked.
		/// </summary>
		private NotificationAction Action;

		/// <summary>
		/// The specific color of the notification for when it will be displayed to
		/// the user.
		/// </summary>
		private Color Color;
		
		/// <summary>
		/// The content or text of the notification.
		/// </summary>
		private string Content;

		/// <summary>
		/// The URL of the notification, if necessary.
		/// </summary>
		private string URL;

		/// <summary>
		/// The physical notification object.
		/// </summary>
        private GameObject NotificationObject;

		// The constructor, set values for notification.
		public Notification(string name, NotificationType type, NotificationAction action, string content, string url ="")
		{
			this.Name = name;
			this.Type =  type;
			this.Action = action;
			this.Content = content;
			this.URL = url;

			// Define the notification color depending on the NotificationType that was passed.
			switch (this.Type)
			{
				case NotificationType.Alert:
					this.Color = new Color(0, 1, 0, 1);
					break;
				case NotificationType.Error:
					this.Color = new Color(1, 0, 0, 1);
					break;
				case NotificationType.Message:
					this.Color = new Color(0, 0, 1, 1);
					break;
				case NotificationType.Screenshot:
					this.Color = new Color(1, 0, 1, 1);
					break;
				case NotificationType.Warning:
					this.Color = new Color(1, 0.92f, 0.016f, 1);
					break;
				
			}

			this.DisplayNotification();
		}		

		// Display the notification with the given details above.
		public void DisplayNotification()
		{
			// Load the NotificationObject prefab from the Resources folder.
            NotificationObject = (GameObject)Resources.Load("NotificationObject", typeof(GameObject));

			// Change the text of the notification
			Text notificationText = NotificationObject.GetComponentInChildren<Text>();
			notificationText.text = this.Content;

			// Change the color of the notification
			Color notificationColor = NotificationObject.GetComponentInChildren<RawImage>().color;
			notificationColor = this.Color;

			// Instantiate the new notification
			GameObject newNotification = GameObject.Instantiate(NotificationObject, GameObject.Find("Debugging Canvas").transform);

			// Put the new notification in a List<GameObjects> to be animated.
			Debug.Log(newNotification);
			
			// Leave a log of the new notification
            Debug.Log("<color=green>New Notification Received! Name: " + this.Name + " Content: " + this.Content + "URL: " + this.URL + "</color>");
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