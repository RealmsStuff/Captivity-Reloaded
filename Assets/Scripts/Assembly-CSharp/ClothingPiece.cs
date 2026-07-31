using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothingPiece : MonoBehaviour
{
	[SerializeField]
	private int m_idClothing;

	[SerializeField]
	private BoneTypePlayer m_boneToAttachTo;

	[SerializeField]
	private bool m_isHideBodyPartAttachedTo;

	[SerializeField]
	private bool m_isAttachToBoneInsteadOfBodypart;

	[SerializeField]
	private int m_sortingNumberClothingPiece;

	[SerializeField]
	private Vector2 m_localPosition;

	[SerializeField]
	private float m_localEulerAngleZ;

	[SerializeField]
	private bool m_isDroppable;

	[SerializeField]
	private bool m_isDestroyable;

	[SerializeField]
	private bool m_isPlayRipSoundOnDropOrDestroy;

	[SerializeField]
	private List<ClothingPiece> m_clothingPiecesConnected;

	[SerializeField]
	private bool m_isDropOnOralThrust;

	[SerializeField]
	private bool m_isDestroyOnOralThrust;

	private int m_sortingOrderDifference;

	private Coroutine m_coroutineApplyBodyPartSortingGroup;

	private bool m_isDropped;

	private bool m_isDestroyed;

	public void SetId(int i_id)
	{
		m_idClothing = i_id;
	}

	public BoneTypePlayer GetBoneToAttachTo()
	{
		return m_boneToAttachTo;
	}

	private void OnEnable()
	{
		if (m_isAttachToBoneInsteadOfBodypart)
		{
			if (m_coroutineApplyBodyPartSortingGroup != null)
			{
				StopCoroutine(m_coroutineApplyBodyPartSortingGroup);
			}
			m_coroutineApplyBodyPartSortingGroup = StartCoroutine(CoroutineApplyBodyPartSortingGroup());
		}
	}

	private IEnumerator CoroutineApplyBodyPartSortingGroup()
	{
		SpriteRenderer l_renderer = GetComponent<SpriteRenderer>();
		while (!m_isDropped && !m_isDestroyed)
		{
			yield return new WaitForEndOfFrame();
			l_renderer.sortingOrder = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(m_boneToAttachTo)
				.GetBodyPartPlayer()
				.GetSortingOrder() + m_sortingNumberClothingPiece + 1;
		}
		m_coroutineApplyBodyPartSortingGroup = null;
	}

	public void DropOrDestroy(bool i_isIgnoreAudipRip)
	{
		if ((!m_isDroppable && !m_isDestroyable) || m_isDropped || m_isDestroyed)
		{
			return;
		}
		if (m_isDroppable)
		{
			Drop();
		}
		else
		{
			Destroy();
		}
		if (m_isHideBodyPartAttachedTo)
		{
			CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(GetBoneToAttachTo())
				.GetBodyPart()
				.GetComponent<SpriteRenderer>()
				.enabled = true;
		}
		if (m_isPlayRipSoundOnDropOrDestroy && !i_isIgnoreAudipRip)
		{
			AudioClip[] array = new AudioClip[3]
			{
				Resources.Load<AudioClip>("Audio/ClothingRip1"),
				Resources.Load<AudioClip>("Audio/ClothingRip2"),
				Resources.Load<AudioClip>("Audio/ClothingRip3")
			};
			int num = Random.Range(0, 3);
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(array[num]);
		}
		foreach (ClothingPiece item in m_clothingPiecesConnected)
		{
			if (i_isIgnoreAudipRip)
			{
				item.DropOrDestroy(i_isIgnoreAudipRip: true);
			}
			else
			{
				item.DropOrDestroy(m_isPlayRipSoundOnDropOrDestroy);
			}
		}
	}

	public void Drop()
	{
		base.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		if (!GetComponent<Rigidbody2D>())
		{
			base.gameObject.AddComponent<Rigidbody2D>();
		}
		GetComponent<Rigidbody2D>().isKinematic = false;
		if (!GetComponent<Collider2D>())
		{
			base.gameObject.AddComponent<PolygonCollider2D>();
		}
		GetComponent<Collider2D>().enabled = true;
		GetComponent<Collider2D>().isTrigger = false;
		m_isDropped = true;
		Object.Destroy(base.gameObject, 10f);
	}

	public void Destroy()
	{
		m_isDestroyed = true;
		Object.Destroy(base.gameObject);
	}

	public Vector2 GetLocalPosition()
	{
		return m_localPosition;
	}

	public float GetLocalEulerAngleZ()
	{
		return m_localEulerAngleZ;
	}

	public int GetIdClothing()
	{
		return m_idClothing;
	}

	public int GetSortingNumberClothingPiece()
	{
		return m_sortingNumberClothingPiece;
	}

	public bool IsDroppable()
	{
		return m_isDroppable;
	}

	public bool IsDestroyable()
	{
		return m_isDestroyable;
	}

	public void Show()
	{
		GetComponent<SpriteRenderer>().enabled = true;
	}

	public void Hide()
	{
		GetComponent<SpriteRenderer>().enabled = false;
	}

	public bool IsHideBodyPartAttachedTo()
	{
		return m_isHideBodyPartAttachedTo;
	}

	public List<ClothingPiece> GetClothingPiecesConnected()
	{
		return m_clothingPiecesConnected;
	}

	public bool IsAttachToBoneInsteadOfBodyPart()
	{
		return m_isAttachToBoneInsteadOfBodypart;
	}

	public bool IsDropOnOralThrust()
	{
		return m_isDropOnOralThrust;
	}

	public bool IsDestroyOnOralThrust()
	{
		return m_isDestroyOnOralThrust;
	}
}
