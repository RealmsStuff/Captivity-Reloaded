using UnityEngine;
using UnityEngine.Rendering;

public class BodyPartPlayer : BodyPart
{
	[SerializeField]
	private Sprite m_sprPale;

	[SerializeField]
	private Sprite m_sprWhite;

	[SerializeField]
	private Sprite m_sprTan;

	[SerializeField]
	private Sprite m_sprBlack;

	[SerializeField]
	private int m_sortingOrder;

	private SortingGroup m_sortingGroup;

	private AudioSource m_audioSourceImpact;

	protected override void Awake()
	{
		base.Awake();
		m_sortingGroup = GetComponent<SortingGroup>();
		m_sortingOrder = m_sortingGroup.sortingOrder;
		m_audioSourceImpact = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
		m_audioSourceImpact.name = "AudioSourceImpact";
	}

	private void LateUpdate()
	{
		m_sortingGroup.sortingOrder = m_sortingOrder;
	}

	public void SetSkinColor(SkinColor i_skinColor)
	{
		switch (i_skinColor)
		{
		case SkinColor.Pale:
			GetComponent<SpriteRenderer>().sprite = m_sprPale;
			break;
		case SkinColor.White:
			GetComponent<SpriteRenderer>().sprite = m_sprWhite;
			break;
		case SkinColor.Tan:
			GetComponent<SpriteRenderer>().sprite = m_sprTan;
			break;
		case SkinColor.Black:
			GetComponent<SpriteRenderer>().sprite = m_sprBlack;
			break;
		}
	}

	public Sprite GetSkinSprite(SkinColor i_skinColor)
	{
		switch (i_skinColor)
		{
		case SkinColor.Pale:
			return m_sprPale;
		case SkinColor.White:
			return m_sprWhite;
		case SkinColor.Tan:
			return m_sprTan;
		case SkinColor.Black:
			return m_sprBlack;
		default:
			return null;
		}
	}

	public void SetSkinSprite(SkinColor i_skinColor, Sprite i_sprite)
	{
		Sprite previous = GetSkinSprite(i_skinColor);
		switch (i_skinColor)
		{
		case SkinColor.Pale:
			m_sprPale = i_sprite;
			break;
		case SkinColor.White:
			m_sprWhite = i_sprite;
			break;
		case SkinColor.Tan:
			m_sprTan = i_sprite;
			break;
		case SkinColor.Black:
			m_sprBlack = i_sprite;
			break;
		}
		if (GetComponent<SpriteRenderer>().sprite == previous)
			GetComponent<SpriteRenderer>().sprite = i_sprite;
	}

	public override void Explode()
	{
		base.Explode();
		foreach (ClothingPiece item in CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetClothingPiecesAttached())
		{
			if (item.transform.parent == base.transform.parent)
			{
				item.Destroy();
			}
		}
	}

	public SortingGroup GetSortingGroup()
	{
		return m_sortingGroup;
	}

	public void SetSortingOrder(int i_sortingOrder)
	{
		m_sortingOrder = i_sortingOrder;
	}

	public int GetSortingOrder()
	{
		return m_sortingOrder;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Bone componentInParent = GetComponentInParent<Bone>();
		if (!componentInParent.GetRigidbody2D())
		{
			return;
		}
		float num = 0.4f;
		float num2 = 5f;
		float num3 = 9f;
		float magnitude = componentInParent.GetRigidbody2D().velocity.magnitude;
		if (!(magnitude < num))
		{
			AudioClip clip = Resources.Load<AudioClip>("Audio/Impact/ImpactLow");
			if (magnitude >= num2)
			{
				clip = Resources.Load<AudioClip>("Audio/Impact/ImpactMid");
			}
			if (magnitude >= num3)
			{
				clip = Resources.Load<AudioClip>("Audio/Impact/ImpactHigh");
			}
			m_audioSourceImpact.PlayOneShot(clip);
		}
	}
}
