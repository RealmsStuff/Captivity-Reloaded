using UnityEngine;

public class InspectionObject : Interactable
{
	[SerializeField]
	private Sprite m_sprInspection;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		CommonReferences.Instance.GetManagerHud().InspectObject(this);
	}

	public Sprite GetSprite()
	{
		return m_sprInspection;
	}
}
