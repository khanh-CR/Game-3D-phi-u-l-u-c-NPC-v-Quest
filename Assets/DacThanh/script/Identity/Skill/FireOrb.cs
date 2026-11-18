using System;
using UnityEngine;

public class FireOrb : MonoBehaviour
{
    bool wasDealingDamage = false;
    private GameObject Caster;

    public void SetCaster(GameObject caster)
    {
        this.Caster = caster;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            wasDealingDamage = true;
            if(wasDealingDamage) other.GetComponent<PlayerTest>().takeDamage(10);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player") wasDealingDamage = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") wasDealingDamage = false;
    }
}
