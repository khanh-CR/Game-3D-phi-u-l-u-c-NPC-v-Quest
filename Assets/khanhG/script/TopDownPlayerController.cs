using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 15f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("References")]
    public Camera mainCamera;

    private CharacterController controller;
    private Animator animator;

    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        Move();
        RotateToMouse();
        HandleAttack();
    }

    // =========================================
    // DI CHUYỂN + ANIMATION
    // =========================================
    void Move()
    {
        if (mainCamera == null)
            return;

        Vector2 input = Vector2.zero;

        // WASD
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                input.y += 1f;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1f;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1f;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1f;
        }

        input = input.normalized;

        // =====================================
        // ANIMATION IDLE / WALK
        // =====================================

        animator.SetFloat("Speed", input.magnitude);

        // =====================================
        // DI CHUYỂN THEO CAMERA
        // =====================================

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move =
            cameraForward * input.y +
            cameraRight * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // =====================================
        // GRAVITY
        // =====================================

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    // =========================================
    // XOAY NHÂN VẬT VỀ CHUỘT
    // =========================================
    void RotateToMouse()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);

        Plane groundPlane =
            new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition =
                ray.GetPoint(distance);

            Vector3 direction =
                mouseWorldPosition - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }
    }

    // =========================================
    // ATTACK - CHUỘT TRÁI
    // =========================================
    void HandleAttack()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetTrigger("Attack");
        }
    }
}