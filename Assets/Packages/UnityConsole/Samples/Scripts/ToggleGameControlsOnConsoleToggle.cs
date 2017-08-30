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

    void OnEnable()
    {
        console.onToggleConsole += ToggleMouseLook;
        ToggleMouseLook(console.isConsoleOpen);
    }

    void OnDisable()
    {
        console.onToggleConsole -= ToggleMouseLook;
        ToggleMouseLook(false);
    }

    private void ToggleMouseLook(bool isConsoleOpen)
    {
        if(mouseLook != null)
            mouseLook.enabled = !isConsoleOpen;

        if(wasdMovement != null)
            wasdMovement.enabled = !isConsoleOpen;
    }
}