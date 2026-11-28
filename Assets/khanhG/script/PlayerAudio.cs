using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Footstep")]
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.4f;
    [Range(0f, 1f)]
    public float footstepVolume = 0.8f;

    [Header("Attack")]
    public AudioClip attackSound;
    [Range(0f, 1f)]
    public float attackVolume = 1f;

    [Header("Movement Detection")]
    public float movementThreshold = 0.02f;

    private AudioSource footstepSource;
    private AudioSource attackSource;

    private Vector3 lastPosition;
    private float footstepTimer;

    void Awake()
    {
        // Tạo AudioSource riêng cho tiếng bước chân
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = 0f;

        // Tạo AudioSource riêng cho tiếng attack
        attackSource = gameObject.AddComponent<AudioSource>();
        attackSource.playOnAwake = false;
        attackSource.loop = false;
        attackSource.spatialBlend = 0f;

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        CheckMovement();
    }

    void CheckMovement()
    {
        // Kiểm tra vị trí thực tế sau khi script di chuyển đã chạy
        Vector3 currentPosition = transform.position;

        Vector3 delta = currentPosition - lastPosition;
        delta.y = 0f;

        float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        lastPosition = currentPosition;

        if (speed <= movementThreshold)
        {
            // Đứng yên: dừng âm thanh ngay
            footstepSource.Stop();
            footstepTimer = 0f;
            return;
        }

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            PlayFootstep();
            footstepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
            return;

        AudioClip clip = footstepSounds[
            Random.Range(0, footstepSounds.Length)
        ];

        if (clip == null)
            return;

        // Dừng tiếng bước chân cũ
        footstepSource.Stop();

        // Phát tiếng bước chân mới
        footstepSource.clip = clip;
        footstepSource.volume = footstepVolume;
        footstepSource.Play();
    }

    public void PlayAttackSound()
    {
        if (attackSound == null)
        {
            Debug.LogError("PlayerAudio: Chưa gán Attack Sound!");
            return;
        }

        attackSource.PlayOneShot(attackSound, attackVolume);

        Debug.Log("Đã phát âm thanh Attack!");
    }
}