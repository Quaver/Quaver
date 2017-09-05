using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hide the default mouse cursor and show a centered pointer instead. Useful in conjunction with MouseLook and world space UI.
/// </summary>
public class LockedCursor : MonoBehaviour
{
    private Graphic cursorGraphic;

    void Start()
    {
        cursorGraphic = GetComponent<Image>();
        cursorGraphic.enabled = false;
    }

    void Update()
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
        cursorGraphic.enabled = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorGraphic.enabled = false;
    }
}
