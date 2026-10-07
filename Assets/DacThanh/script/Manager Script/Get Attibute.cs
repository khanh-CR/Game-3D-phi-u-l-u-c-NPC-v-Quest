using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetAttibute : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public int id, x, y;
    public GameObject t;
    public void GetRoomById(int id)
    {
        DungeonManagers.instance.GetRoomById(id);
        
        _text.text = $"Dungeon Room ID: {id}\n" +
                     $"Size width: {DungeonManagers.instance.GetRoomById(id).Width}\n" +
                     $"Size Height: {DungeonManagers.instance.GetRoomById(id).Height}\n" +
                     $"Start Postion: {DungeonManagers.instance.GetRoomById(id).Min}\n";
    }

    public void GetRoomByIdssss()
    {
        Vector3? pos = DungeonManagers.instance.GetPosOfRoomId(id, x, y);
        if(!pos.HasValue) return;
            var obj = Instantiate(t,pos.Value + new Vector3(0,0.5f,0),Quaternion.identity);
    }
}
