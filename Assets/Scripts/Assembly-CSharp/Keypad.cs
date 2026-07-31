using UnityEngine;

public class Keypad : Interactable
{
	[SerializeField]
	private string m_code;

	[SerializeField]
	private Interactable m_interactableToActivateOnPass;

	private bool m_isCompleted;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		CommonReferences.Instance.GetManagerHud().OpenKeypad(this);
	}

	public void SetCode(string i_code)
	{
		m_code = i_code;
	}

	public string GetCode()
	{
		return m_code;
	}

	public void Complete()
	{
		m_isCompleted = true;
		m_interactableToActivateOnPass.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
	}

	public bool GetIsCompleted()
	{
		return m_isCompleted;
	}
}
