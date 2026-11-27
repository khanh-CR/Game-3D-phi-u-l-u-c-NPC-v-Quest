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
            loadScene();
        }
    }
//sdsd
    public void loadScene()
    {

        DontDestroyOnLoad(gameObject);
        if (target != null) DontDestroyOnLoad(target);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneIndex);
    }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    //     // target.GetComponent<TopDownPlayerController>().mainCamera =  Camera.main;
    //     DungeonManagers mgr = DungeonManagers.instance;
    //     mgr.player = target;
    //     if (mgr == null)
    //     {
    //         Debug.LogError("Tele: scene mới không có DungeonManagers");
    //         return;
    //     }
    //     mgr.CreateDungeon(data);
    //     Vector3? pos = mgr.GetPosOfRoomId(0, 5, 5);
    //     if (pos != null && target != null) TeleportTo(pos.Value);
    //     Destroy(gameObject);
    // }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Kiểm tra DungeonManagers
        DungeonManagers mgr = DungeonManagers.instance;

        if (mgr == null)
        {
            Debug.LogError("Tele: Scene mới không có DungeonManagers!");
            return;
        }

        // Kiểm tra Player
        if (target == null)
        {
            Debug.LogError("Tele: Target (Player) bị null!");
            return;
        }

        // Kiểm tra DungeonData
        if (data == null)
        {
            Debug.LogError("Tele: DungeonData chưa được gán!");
            return;
        }

        mgr.player = target;

        // Tạo Dungeon
        mgr.CreateDungeon(data);

        Vector3? pos = mgr.GetPosOfRoomId(0, 5, 5);

        if (pos.HasValue)
        {
            TeleportTo(pos.Value);
        }
        else
        {
            Debug.LogError("Tele: Không tìm thấy vị trí phòng (0, 5, 5)!");
        }

        Destroy(gameObject);
    }
    private void TeleportTo(Vector3 pos)
    {
        CharacterController cc = target.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        target.transform.position = pos + Vector3.up * yOffset;
        if (cc != null) cc.enabled = true;
    }
}