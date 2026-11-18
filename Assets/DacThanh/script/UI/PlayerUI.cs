using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUI : MonoBehaviour
{
    public Gate currentGate;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && currentGate != null)
        {
            currentGate.OpenTheGate();   // Gate tự xác định đang ở trong hay ngoài
        }
    }
}