using UnityEngine;

public class Note : Interactable
{
	[TextArea(3, 10)]
	[SerializeField]
	private string m_text;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		CommonReferences.Instance.GetManagerHud().ShowNote(this);
	}

	public string GetText()
	{
		return m_text;
	}

	public void SetText(string i_text)
	{
		m_text = i_text;
	}
}
