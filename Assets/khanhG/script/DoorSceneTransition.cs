using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DoorSceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject interactUI;
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    private bool playerNear;
    private bool transitioning;

    void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;
    }

    void Update()
    {
        // New Input System
        if (playerNear &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !transitioning)
        {
            StartCoroutine(ChangeScene());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (interactUI != null)
                interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }

    IEnumerator ChangeScene()
    {
        transitioning = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            if (fadeCanvas != null)
                fadeCanvas.alpha = Mathf.Clamp01(time / fadeDuration);

            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}