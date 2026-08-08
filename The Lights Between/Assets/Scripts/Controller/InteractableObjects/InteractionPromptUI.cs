using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 8f;

    private Transform _mainCameraTransform;
    private bool _isVisible;

    private void Awake()
    {
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void LateUpdate()
    {
        if (_isVisible && _mainCameraTransform != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - _mainCameraTransform.position);
        }

        if (canvasGroup != null)
        {
            float targetAlpha = _isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    public void Show(string prompt)
    {
        if (promptText != null)
        {
            promptText.text = prompt;
        }

        _isVisible = true;
    }

    public void Hide()
    {
        _isVisible = false;
    }
}