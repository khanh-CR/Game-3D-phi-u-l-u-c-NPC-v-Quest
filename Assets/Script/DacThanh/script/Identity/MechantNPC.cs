using System;
using UnityEngine;

public class MechantNPC : MonoBehaviour
{
    public GameObject mechantShop;
    private void OnTriggerEnter(Collider other)
    {
        mechantShop.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        mechantShop.SetActive(false);
    }
}
