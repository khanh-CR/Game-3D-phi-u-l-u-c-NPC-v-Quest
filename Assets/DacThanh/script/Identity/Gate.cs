using System;
using Unity.VisualScripting;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public Transform HangPoint;
    [SerializeField] private float rotateSpeed = 80f;
    private bool isOpening = false;
    public bool isOpened = false;
    private Quaternion targetRotation;
    public DungeonRoom dungeonRoom;
    bool roomActive =false;
    private void Update()
    {
        if (!isOpening || HangPoint == null) return;
        HangPoint.rotation = Quaternion.RotateTowards(
            HangPoint.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
        if (Quaternion.Angle(targetRotation, HangPoint.rotation) < 0.1f)
        {
            HangPoint.rotation = targetRotation;
            isOpening = false;
        }

        if (roomActive)
        {
            DungeonManagers.instance.SpawnMonstersInRoom(dungeonRoom);
            roomActive = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {   
        if (other.CompareTag("Player"))
        {
            var playerUI = other.GetComponent<PlayerUI>();
            if (playerUI == null)
            {
                Debug.LogError("Không tìm thấy Component PlayerUI trên GameObject Player!");
                return;
            }
            playerUI.playerUi.SetActive(true);
            playerUI.currentGate = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerUI = other.GetComponent<PlayerUI>();
            if (playerUI == null)
            {
                Debug.LogError("Không tìm thấy Component PlayerUI trên GameObject Player!");
                return;
            }
            playerUI.playerUi.SetActive(false);
            if (playerUI.currentGate == this)
            {
                playerUI.currentGate = null;
            }
        }
    }

    public void OpenTheGate()
    {
        if (HangPoint == null)
        {
            Debug.Log("Chưa gán hangPoint");
            return;
        }
        if (isOpening) return;
        targetRotation = Quaternion.Euler(0f,0f,0f);
        isOpening = true;
        isOpened = true;
        Debug.Log("bắt đầu mở cửa");
        dungeonRoom.CheckRoomWave();
        roomActive = true;
    }

    public void CloseTheGate()
    {
        if (HangPoint == null)
        {
            Debug.Log("Chưa gán hangPoint");
            return;
        }
        if (isOpening) return;
        targetRotation = Quaternion.Euler(0f,90f,0f);
        isOpening = true;
        isOpened = false;
        Debug.Log("bắt đầu mở cửa");
    }
}
