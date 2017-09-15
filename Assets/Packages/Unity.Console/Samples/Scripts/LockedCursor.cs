// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hide the default mouse cursor and show a centered pointer instead. Useful in conjunction with MouseLook and world space UI.
/// </summary>
public class LockedCursor : MonoBehaviour
{
    private Graphic _cursorGraphic;

    private void Start()
    {
        _cursorGraphic = GetComponent<Image>();
        _cursorGraphic.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockCursor();
        else if (Input.GetMouseButtonUp(0))
            LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _cursorGraphic.enabled = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _cursorGraphic.enabled = false;
    }
}
