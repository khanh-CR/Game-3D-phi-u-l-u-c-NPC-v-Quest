using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerTest : MonoBehaviour, ISkillable
{
    [SerializeField] public CharacterData data;
    [Header("Attitude")] 
    public int atk;
    public int armor;
    public int hp;
    
    public int currentAtk;
    public int currentArmor;
    public int currentHp;

    public void SetUp()
    {
        
        atk = data.atk;
        hp = data.hp;
        armor = data.armor;
        
        currentAtk = atk;
        currentArmor = armor;
        currentHp = hp;
    }
    [Header("Level")]
    public int level = 1;
    public int maxLevel = 20;
    public int exp;
    public int baseExp = 100;          
    public float expGrowth = 1.2f;     
    public int levelsPerSlot = 5;      
    public int atkPerLevel = 2, defPerLevel = 1, hpPerLevel = 10;
    public int ExpToNext => Mathf.Max(1, Mathf.RoundToInt(baseExp * Mathf.Pow(expGrowth, level - 1)));
    [Header("Skills")]
    public SkillBook skillBook;
    
    [Header("Movement")]
    public float moveSpeed = 4f;
    private float rotationSpeed = 0.1f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("References")] 
    public GameObject AtkArea;
    private CharacterController controller;
    private Animator animator;
    private bool isFirstPersonPerspective = true;
    private float verticalVelocity;

    [Header("Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    public Transform cameraTarget;
    private CinemachineThirdPersonFollow tpf;
    private float zoomSpeed = 40f;
    private float minZoom = 0f;
    private float maxZoom = 1.5f;

    private float cameraPitch = 0f;
    private float cameraDistance = 0f;

    // Alt cursor control
    private bool isAltPressed = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        tpf = cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
        animator = GetComponent<Animator>();
        skillBook.Init(this);
        UpdateSkillSlots();
        setCam(isFirstPersonPerspective);
        SetCursorState(false);
        SetUp();
    }
    private void Update()
    {
        HandleCursor();

        // Chỉ điều khiển camera khi chuột đang bị khóa
        if (!isAltPressed)
        {
            if (Keyboard.current != null &&
                Keyboard.current.vKey.wasPressedThisFrame)
            {
                isFirstPersonPerspective = !isFirstPersonPerspective;
                setCam(isFirstPersonPerspective);
            }

            HandleZoom();
            HandleLook();
        }

        HandleGravity();
        Move();
        skillBook.Tick();
    }

    // ================= CURSOR =================

    private void HandleCursor()
    {
        if (Keyboard.current == null) return;

        bool altPressed =
            Keyboard.current.leftAltKey.isPressed ||
            Keyboard.current.rightAltKey.isPressed;

        if (altPressed != isAltPressed)
        {
            isAltPressed = altPressed;
            SetCursorState(isAltPressed);
        }
    }

    private void SetCursorState(bool showCursor)
    {
        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // ================= CAMERA =================

    public void setCam(bool isFirstPersonPerspective)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraTarget.localPosition = new Vector3(0, 0f, 0);

        if (isFirstPersonPerspective)
        {
            cameraPitch = 0f;
            tpf.ShoulderOffset = new Vector3(0, 1f, 0);
            tpf.VerticalArmLength = 0f;
            tpf.CameraSide = 0.5f;
            tpf.CameraDistance = 0.1f;
        }
        else
        {
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

        transform.Rotate(Vector3.up * mouseDelta.x * rotationSpeed);

        if (isFirstPersonPerspective)
            cameraPitch -= mouseDelta.y * rotationSpeed;

        cameraPitch = Mathf.Clamp(cameraPitch, -35f, 70f);

        cameraTarget.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    // ================= MOVEMENT =================

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

        if (animator != null)
            animator.SetFloat("Speed", input.magnitude);

        Vector3 movement = transform.right * input.x + transform.forward * input.y;
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }
    // ================= UPGRADE STATS =================

    public void UpgradeAtk(int amount)
    {
        atk += amount;
        currentAtk += amount;

        Debug.Log("ATK upgraded: " + currentAtk);
    }

    public void UpgradeDef(int amount)
    {
        currentArmor += amount;
        currentArmor += amount;

        Debug.Log("DEF upgraded: " + currentArmor);
    }

    public void UpgradeHp(int amount)
    {
        hp += amount;
        currentHp += amount;

        Debug.Log("HP upgraded: " + currentHp);
    }
    public void TakeDamage(int amount)
    {
        currentHp = Mathf.Max(0, currentHp - Mathf.RoundToInt(amount));
    }
    public void takeDamage(int damage)
    {
        int actualDamage = Mathf.Max(0, damage - currentArmor);

        currentHp -= actualDamage;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log("Player took damage: " + actualDamage);
        Debug.Log("Current HP: " + currentHp);
    }
    public void Heal(int amount)
    {
        currentHp = Mathf.Min(hp, currentHp + Mathf.RoundToInt(amount));
    }
    public void AddAtk(int amount) => currentAtk += amount;
    public void Teleport(Vector3 pos)
    {
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;

        verticalVelocity = 0f;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #region Skill
    public void AddExp(int amount)
    {
        if (level >= maxLevel) return;

        exp += amount;
        int ups = 0;
        while (level < maxLevel && exp >= ExpToNext) { exp -= ExpToNext; level++; ups++; }
        if (level >= maxLevel) exp = 0;      // đạt cấp tối đa thì không tích exp nữa
        if (ups == 0) return;

        atk += atkPerLevel * ups;  currentAtk += atkPerLevel * ups;
        armor += defPerLevel * ups;  currentArmor += defPerLevel * ups;
        hp  += hpPerLevel * ups;   currentHp  += hpPerLevel * ups;

        UpdateSkillSlots();                  // lên level chỉ tăng stats và mở khóa slot
    }
    private void UpdateSkillSlots() => skillBook.SetUnlocked(1 + level / Mathf.Max(1, levelsPerSlot));

    #endregion
}
