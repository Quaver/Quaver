// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using Wenzil.Console;

/// <summary>
/// A special utility class that revokes user controls whenever the console is open. Very game-specific.
/// </summary>
public class ToggleGameControlsOnConsoleToggle : MonoBehaviour
{
    public ConsoleUI console;
    public MouseLook mouseLook;
    public WASDMovement wasdMovement;

    private void OnEnable()
    {
        console.onToggleConsole += ToggleMouseLook;
        ToggleMouseLook(console.isConsoleOpen);
    }

    private void OnDisable()
    {
        console.onToggleConsole -= ToggleMouseLook;
        ToggleMouseLook(false);
    }

    private void ToggleMouseLook(bool isConsoleOpen)
    {
        if (mouseLook != null)
            mouseLook.enabled = !isConsoleOpen;

        if (wasdMovement != null)
            wasdMovement.enabled = !isConsoleOpen;
    }
}