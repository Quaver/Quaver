// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System;
using Quaver.Notifications;

namespace Wenzil.Console.Commands
{
    /// <summary>
    /// NOTIF command. Send a test notification.
    /// </summary>
    public static class NotificationCommand
    {
        public static readonly string name = "NOTIF";
        public static readonly string description = "Send a notification | Args: (content)";
        public static readonly string usage = "NOTIF";

        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return "You must provide content for the notification";
            }

            string notifName = "Test";
            string notifContent = String.Join(" ", args);

            new Notification(notifName, NotificationType.Message, NotificationAction.Disappear, notifContent);

            return "Notification successfully sent.";
        }
    }
}