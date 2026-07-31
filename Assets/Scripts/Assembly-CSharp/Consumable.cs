using UnityEngine;

public abstract class Consumable : PickUpable
{
	[SerializeField]
	private AudioClip m_audioConsume;

	protected float m_rangePickUp;

	protected override void Awake()
	{
		base.Awake();
		m_rangePickUp = 1.25f;
	}

	private void Update()
	{
		if (GetIsCloseEnoughToConsume())
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioConsume);
			Consume();
			Destroy();
		}
	}

	private bool GetIsCloseEnoughToConsume()
	{
		if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPosFeet(), base.transform.position) < m_rangePickUp)
		{
			return true;
		}
		return false;
	}

	public abstract void Consume();

	private void Destroy()
	{
		Object.Destroy(base.gameObject);
	}
}
