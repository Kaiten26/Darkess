using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // Disable the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
