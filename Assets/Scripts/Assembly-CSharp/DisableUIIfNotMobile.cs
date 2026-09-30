using UnityEngine;

public class DisableUIIfNotMobile : MonoBehaviour
{
    private PlayerController m_playerController;
    private CanvasGroup m_canvasGroup;

    private void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        if (m_canvasGroup == null) m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        m_playerController = FindObjectOfType<PlayerController>();
		InputGlyphLibrary.StyleMobileCanvas(transform);
        RefreshVisibility();
    }

    private void Update()
    {
        if (m_playerController == null) m_playerController = FindObjectOfType<PlayerController>();
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        bool visible = m_playerController != null && m_playerController.ShouldShowMobileControls();
        if (!visible && m_playerController != null) m_playerController.CancelMobileHeldInputs();
        m_canvasGroup.alpha = visible ? 1f : 0f;
        m_canvasGroup.interactable = visible;
        m_canvasGroup.blocksRaycasts = visible;
    }
}
