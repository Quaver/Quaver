// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

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
        private string _name;

        /// <summary>
        /// The type of notification defined in the NotificationType enum
        /// </summary>
        private NotificationType _type;

        /// <summary>
        /// The action that will take place when a notification is clicked.
        /// </summary>
        private NotificationAction _action;

        /// <summary>
        /// The specific color of the notification for when it will be displayed to
        /// the user.
        /// </summary>
        private Color _color;

        /// <summary>
        /// The content or text of the notification.
        /// </summary>
        private string _content;

        /// <summary>
        /// The URL of the notification, if necessary.
        /// </summary>
        private string _URL;

        /// <summary>
        /// The physical notification object.
        /// </summary>
        private GameObject _notificationObject;

        // The constructor, set values for notification.
        public Notification(string name, NotificationType type, NotificationAction action, string content, string url = "")
        {
            _name = name;
            _type = type;
            _action = action;
            _content = content;
            _URL = url;

            // Define the notification color depending on the NotificationType that was passed.
            switch (_type)
            {
                case NotificationType.Alert:
                    _color = new Color(0, 1, 0, 1);
                    break;
                case NotificationType.Error:
                    _color = new Color(1, 0, 0, 1);
                    break;
                case NotificationType.Message:
                    _color = new Color(0, 0, 1, 1);
                    break;
                case NotificationType.Screenshot:
                    _color = new Color(1, 0, 1, 1);
                    break;
                case NotificationType.Warning:
                    _color = new Color(1, 0.92f, 0.016f, 1);
                    break;
            }

            this.DisplayNotification();
        }

        // Display the notification with the given details above.
        public void DisplayNotification()
        {
            // Load the NotificationObject prefab from the Resources folder.
            _notificationObject = (GameObject)Resources.Load("NotificationObject", typeof(GameObject));

            // Change the text of the notification
            Text notificationText = _notificationObject.GetComponentInChildren<Text>();
            notificationText.text = _content;

            // Change the color of the notification
            Color notificationColor = _notificationObject.GetComponentInChildren<RawImage>().color;
            notificationColor = _color;

            // Instantiate the new notification
            GameObject newNotification = GameObject.Instantiate(_notificationObject, GameObject.Find("Debugging Canvas").transform);

            // Put the new notification in a List<GameObjects> to be animated.
            Debug.Log(newNotification);

            // Leave a log of the new notification
            Debug.Log("<color=green>New Notification Received! Name: " + _name + " Content: " + _content + "URL: " + _URL + "</color>");
        }

        // Implement later, this'll be the method that defines what to do when a notification is clicked.
        public void ClickHandler()
        {
            switch (_action)
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