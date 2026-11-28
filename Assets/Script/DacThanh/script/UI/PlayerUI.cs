using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUI : MonoBehaviour
{
    public Gate currentGate;
    public TextMeshProUGUI tmp;
    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame && currentGate != null)
        {
            currentGate.OpenTheGate();   
        }
    }

    public void UpdateText(string text)
    {
        tmp.text = text;
    }
}