using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerTest : MonoBehaviour
{
    [SerializeField] public CharacterData data;
    [Header("Attitude")] 
    public int atk;
    public int def;
    public int hp;
    
    public int currentAtk;
    public int currentDef;
    public int currentHp;
    
    [Header("Movement")]
    public float moveSpeed = 4f;
    private float rotationSpeed = 0.1f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("References")]
    private CharacterController controller;
    private Animator animator;
    private bool isFirstPersonPerspective = true;
    private float verticalVelocity;

    [Header("Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    public Transform cameraTarget; // Object con của Player, kéo vào Tracking Target của CinemachineCamera
    private CinemachineThirdPersonFollow tpf;
    private float zoomSpeed = 20f;
    private float minZoom = 0f;
    private float maxZoom = 1.5f;

    private float cameraPitch = 0f;
    private float cameraDistance = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        tpf = cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
        animator = GetComponent<Animator>();
        setCam(isFirstPersonPerspective);
    }
    private void Update()
    {

        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            isFirstPersonPerspective = !isFirstPersonPerspective;
            setCam(isFirstPersonPerspective);
        }
        HandleZoom();
        HandleLook();
        HandleGravity();
        Move();
    }

    public void setCam(bool isFirstPersonPerspective)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Pivot đặt ngang tầm đầu (tương ứng TargetOffset y = 1 trước đây)
        cameraTarget.localPosition = new Vector3(0, 0f, 0);

        if (isFirstPersonPerspective)
        {
            cameraPitch = 0f;
            tpf.ShoulderOffset = new Vector3(0,1f,0);
            tpf.VerticalArmLength = 0f;
            tpf.CameraSide = 0.5f;
            tpf.CameraDistance = 0.1f;
        }
        else
        {
            // Tương ứng TargetOffset (0, 2, -1) trước đây, chỉnh lại cho vừa mắt
            cameraPitch = 50f;
            tpf.ShoulderOffset = new Vector3(0f, 2f, -2f);
            tpf.VerticalArmLength = 1f;
            tpf.CameraSide = 0.5f;
            tpf.CameraDistance = cameraDistance;
        }
    }

    private void HandleZoom()
    {
        if (isFirstPersonPerspective) return;
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        cameraDistance -= scroll * zoomSpeed * 0.01f;
        cameraDistance = Mathf.Clamp(cameraDistance, minZoom, maxZoom);
        tpf.CameraDistance = cameraDistance;
    }

    private void HandleLook()
    {
        if (Mouse.current == null) return;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Xoay ngang: luôn xoay Player
        transform.Rotate(Vector3.up * mouseDelta.x * rotationSpeed);
        // Góc thứ nhất: xoay dọc bình thường
        
        if(isFirstPersonPerspective) cameraPitch -= mouseDelta.y * rotationSpeed;
        // else cameraPitch = 50f;
        cameraPitch = Mathf.Clamp(cameraPitch, -35f, 70f);
        cameraTarget.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void Move()
    {
        Vector2 input = Vector2.zero;
        var cinemachineCD = GetComponent<CinemachineDeoccluder>();
        if (Keyboard.current != null || cinemachineCD.MinimumDistanceFromTarget < 0.5f)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
        }

        input = input.normalized;
        animator.SetFloat("Speed", input.magnitude);

        Vector3 movement = transform.right * input.x + transform.forward * input.y;
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    public void takeDamage(int damage)
    {
        Debug.Log("takeDamage");
    }
    public void Teleport(Vector3 pos)
    {
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
        verticalVelocity = 0f;
    }
}