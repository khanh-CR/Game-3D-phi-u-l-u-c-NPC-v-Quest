using System.Collections.Generic;
using UnityEngine;

public class DarkOrbDestruction : MonoBehaviour
{
    public Transform Caster;
    public List<GameObject> Orbs = new List<GameObject>();

    public float radius = 2f;
    public float speed = 200f;

    private float angle;

    private void Update()
    {
        if (Caster == null || Orbs.Count == 0)
            return;

        angle += speed * Time.deltaTime;

        for (int i = 0; i < Orbs.Count; i++)
        {
            if (Orbs[i] == null) continue;

            // Góc của từng Orb, cách đều nhau
            float orbAngle = angle + (360f / Orbs.Count * i);

            float rad = orbAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * radius,
                0f,
                Mathf.Sin(rad) * radius
            );

            // Tính vị trí dựa trên vị trí hiện tại của Player
            Orbs[i].transform.position = Caster.position + offset;
        }
    }

    public void addOrb(GameObject orb)
    {
        Orbs.Add(orb);
    }
}