using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Tele : MonoBehaviour
{
    public GameObject target;   // Player (root object)
    public DungeonData data;
    public int sceneIndex = 2;
    public float yOffset = 2f;

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // loadScene();
        }
    }
//sdsd
    
}