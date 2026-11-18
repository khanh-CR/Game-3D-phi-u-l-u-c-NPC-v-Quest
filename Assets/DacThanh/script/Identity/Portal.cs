using UnityEngine;

public class Portal : MonoBehaviour
{
    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        DungeonManagers.instance.NextFloor();
    }
}