
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class MenuButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public TMP_Text buttonText;
    public float scaleMultiplier = 1.15f;
    public float animationSpeed = 10f;

    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (buttonText != null)
            buttonText.color = normalColor;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;

        if (buttonText != null)
            buttonText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;

        if (buttonText != null)
            buttonText.color = normalColor;
    }
}
