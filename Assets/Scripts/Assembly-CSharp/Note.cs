using UnityEngine;

public class Note : Interactable
{
	[TextArea(3, 10)]
	[SerializeField]
	private string m_text;

	[SerializeField]
	private int m_fontSize;

	public void ConfigureModNote(string i_text, int i_fontSize)
	{
		ResetModActivationLinks();
		m_text = i_text;
		m_fontSize = i_fontSize;
		m_priceToActivate = 0;
		m_isSingleUse = false;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = true;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = false;
		m_isCanBeActivatedByNPC = false;
		m_isHideNotificationInteract = false;
		m_isUnInteractable = false;
	}

	public bool IsUsableModTemplate()
	{
		return GetComponent<SpriteRenderer>() != null;
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		CommonReferences.Instance.GetManagerHud().ShowNote(this);
	}

	public string GetText()
	{
		return m_text;
	}

	public int GetFontSize()
	{
		return m_fontSize;
	}

	public void SetText(string i_text)
	{
		m_text = i_text;
	}
}
