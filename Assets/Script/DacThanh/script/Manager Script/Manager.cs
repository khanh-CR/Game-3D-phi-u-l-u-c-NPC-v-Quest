using System;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager instance;
    public DungeonManagers dungeonMgr;
    public GameObject player;
    private void Awake()
    {
        instance = this;
        dungeonMgr = DungeonManagers.instance;
    }

    public void SetPlayerToDungeonManager()
    {
        player = dungeonMgr.player;
    }
}
