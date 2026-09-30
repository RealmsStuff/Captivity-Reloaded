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
	private List<ClothingPiece> m_clothingPiecesConnected = new List<ClothingPiece>();

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
		ModClothingSway sway = GetComponent<ModClothingSway>();
		if (sway != null) sway.enabled = false;
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
		ModClothingSway sway = GetComponent<ModClothingSway>();
		if (sway != null) sway.enabled = false;
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

	public bool IsPlayRipSoundOnDropOrDestroy() { return m_isPlayRipSoundOnDropOrDestroy; }

	public void ConfigureConnectedPieces(IEnumerable<ClothingPiece> i_pieces)
	{
		m_clothingPiecesConnected.Clear();
		if (i_pieces != null) foreach (ClothingPiece piece in i_pieces)
			if (piece != null && piece != this && !m_clothingPiecesConnected.Contains(piece)) m_clothingPiecesConnected.Add(piece);
	}

	public void ConfigureModAttachment(CaptivityReloaded.Modding.ClothingAttachmentDefinition i_attachment)
	{
		if (i_attachment == null) return;
		if (!string.IsNullOrWhiteSpace(i_attachment.Bone)
			&& System.Enum.TryParse(i_attachment.Bone, true, out BoneTypePlayer bone)) m_boneToAttachTo = bone;
		if (i_attachment.OffsetX.HasValue) m_localPosition.x = i_attachment.OffsetX.Value;
		if (i_attachment.OffsetY.HasValue) m_localPosition.y = i_attachment.OffsetY.Value;
		if (i_attachment.Rotation.HasValue) m_localEulerAngleZ = i_attachment.Rotation.Value;
		if (i_attachment.SortingOffset.HasValue) m_sortingNumberClothingPiece = i_attachment.SortingOffset.Value;
		if (i_attachment.AttachToBone.HasValue) m_isAttachToBoneInsteadOfBodypart = i_attachment.AttachToBone.Value;
		if (i_attachment.HideBodyPart.HasValue) m_isHideBodyPartAttachedTo = i_attachment.HideBodyPart.Value;
		if (i_attachment.Droppable.HasValue) m_isDroppable = i_attachment.Droppable.Value;
		if (i_attachment.Destroyable.HasValue) m_isDestroyable = i_attachment.Destroyable.Value;
		if (i_attachment.DropOnOralThrust.HasValue) m_isDropOnOralThrust = i_attachment.DropOnOralThrust.Value;
		if (i_attachment.DestroyOnOralThrust.HasValue) m_isDestroyOnOralThrust = i_attachment.DestroyOnOralThrust.Value;
	}

	public void ConfigureRipSound(bool i_play) { m_isPlayRipSoundOnDropOrDestroy = i_play; }
}
