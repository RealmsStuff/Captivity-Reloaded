using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SkeletonPlayer : Skeleton
{
	[SerializeField]
	private SortingGroup m_sortingGroupHoldWeapon;

	private ManagerFacePlayer m_managerFacePlayer;

	private int m_sortingOrderRArmUpper;

	private int m_sortingOrderLArmUpper;

	private int m_sortingOrderRArmLower;

	private int m_sortingOrderLArmLower;

	private int m_sortingOrderRHand;

	private int m_sortingOrderLHand;

	private List<Clothing> m_clothesEquipped = new List<Clothing>();

	private SkinColor m_skinColor;

	private EyeColor m_eyeColor;

	private readonly List<SpriteRenderer> m_auxiliarySkinRenderers = new List<SpriteRenderer>();

	private Color m_auxiliarySkinTint = Color.white;

	protected override void Awake()
	{
		base.Awake();
		m_managerFacePlayer = GetComponentInChildren<ManagerFacePlayer>();
		RememberArmSortOrder();
		m_skinColor = SkinColor.White;
	}

	private void RememberArmSortOrder()
	{
		BonePlayer bone = GetBone(BoneTypePlayer.rArmUpper);
		BonePlayer bone2 = GetBone(BoneTypePlayer.rArmLower);
		BonePlayer bone3 = GetBone(BoneTypePlayer.rHand);
		BonePlayer bone4 = GetBone(BoneTypePlayer.lArmUpper);
		BonePlayer bone5 = GetBone(BoneTypePlayer.lArmLower);
		BonePlayer bone6 = GetBone(BoneTypePlayer.lHand);
		m_sortingOrderRArmUpper = bone.GetBodyPart().GetComponent<SortingGroup>().sortingOrder;
		m_sortingOrderLArmUpper = bone4.GetBodyPart().GetComponent<SortingGroup>().sortingOrder;
		m_sortingOrderRArmLower = bone2.GetBodyPart().GetComponent<SortingGroup>().sortingOrder;
		m_sortingOrderLArmLower = bone5.GetBodyPart().GetComponent<SortingGroup>().sortingOrder;
		m_sortingOrderRHand = bone3.GetBodyPart().GetComponent<SortingGroup>().sortingOrder;
		m_sortingOrderLHand = bone6.GetBodyPart().GetComponent<SortingGroup>().sortingOrder;
	}

	protected override void HandleEnableRagdoll()
	{
		foreach (BonePlayer bone in m_bones)
		{
			if ((bool)bone.GetRigidbody2D())
			{
				bone.GetRigidbody2D().isKinematic = false;
				bone.GetRigidbody2D().velocity = CommonReferences.Instance.GetPlayer().GetVelocity();
			}
		}
	}

	protected override void CenterSkeleton()
	{
		GetBone(BoneTypePlayer.Hips).transform.localPosition = Vector3.zero;
	}

	public BonePlayer GetBone(BoneTypePlayer i_boneType)
	{
		foreach (BonePlayer bone in m_bones)
		{
			if (bone.GetBoneType() == i_boneType)
			{
				return bone;
			}
		}
		return null;
	}

	public override void SetInvulnerable(bool i_isInvulnerable)
	{
		base.SetInvulnerable(i_isInvulnerable);
		if (i_isInvulnerable)
		{
			foreach (ClothingPiece item in GetClothingPiecesAttached())
			{
				Color color = item.GetComponent<SpriteRenderer>().color;
				color.a = 0.5f;
				item.GetComponent<SpriteRenderer>().color = color;
			}
			return;
		}
		foreach (ClothingPiece item2 in GetClothingPiecesAttached())
		{
			Color color2 = item2.GetComponent<SpriteRenderer>().color;
			color2.a = 1f;
			item2.GetComponent<SpriteRenderer>().color = color2;
		}
	}

	public override void SetIsFacingLeft(bool i_isFacingLeft)
	{
		if (m_isFacingLeft != i_isFacingLeft)
		{
			CommonReferences.Instance.GetPlayer().GetIsBeingRaped();
			base.SetIsFacingLeft(i_isFacingLeft);
			BonePlayer bone = GetBone(BoneTypePlayer.rArmUpper);
			BonePlayer bone2 = GetBone(BoneTypePlayer.rArmLower);
			BonePlayer bone3 = GetBone(BoneTypePlayer.rHand);
			BonePlayer bone4 = GetBone(BoneTypePlayer.lArmUpper);
			BonePlayer bone5 = GetBone(BoneTypePlayer.lArmLower);
			BonePlayer bone6 = GetBone(BoneTypePlayer.lHand);
			if (GetIsTrueFacingLeft())
			{
				bone.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLArmUpper);
				bone2.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLArmLower);
				bone3.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLHand);
				bone4.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRArmUpper);
				bone5.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRArmLower);
				bone6.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRHand);
				m_sortingGroupHoldWeapon.sortingOrder = m_sortingOrderLHand + 4;
			}
			else
			{
				bone.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRArmUpper);
				bone2.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRArmLower);
				bone3.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRHand);
				bone4.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLArmUpper);
				bone5.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLArmLower);
				bone6.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLHand);
				m_sortingGroupHoldWeapon.sortingOrder = m_sortingOrderRHand - 4;
			}
			PlaceClothesOnTopOfBodyParts();
		}
	}

	public bool GetIsTrueFacingLeft()
	{
		if (GetBone(BoneTypePlayer.Hips).transform.localScale.x == -1f)
		{
			if (m_isFacingLeft)
			{
				return false;
			}
			return true;
		}
		if (m_isFacingLeft)
		{
			return true;
		}
		return false;
	}

	public void ResetArmSorting()
	{
		BonePlayer bone = GetBone(BoneTypePlayer.rArmUpper);
		BonePlayer bone2 = GetBone(BoneTypePlayer.rArmLower);
		BonePlayer bone3 = GetBone(BoneTypePlayer.rHand);
		BonePlayer bone4 = GetBone(BoneTypePlayer.lArmUpper);
		BonePlayer bone5 = GetBone(BoneTypePlayer.lArmLower);
		BonePlayer bone6 = GetBone(BoneTypePlayer.lHand);
		bone.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRArmUpper);
		bone2.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRArmLower);
		bone3.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderRHand);
		bone4.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLArmUpper);
		bone5.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLArmLower);
		bone6.GetBodyPartPlayer().SetSortingOrder(m_sortingOrderLHand);
		m_sortingGroupHoldWeapon.sortingOrder = m_sortingOrderRHand - 4;
	}

	public void PlaceEquippableOnHand(Weapon i_equippable)
	{
		SortingGroup componentInChildren = GetBone(BoneTypePlayer.rHand).GetComponentInChildren<SortingGroup>();
		if (m_isFacingLeft)
		{
			componentInChildren.sortingOrder = m_sortingOrderLHand + 3;
		}
		else
		{
			componentInChildren.sortingOrder = m_sortingOrderRHand - 3;
		}
	}

	public ManagerFacePlayer GetManagerFacePlayer()
	{
		return m_managerFacePlayer;
	}

	private void AnimEventUseUsable()
	{
		if ((bool)CommonReferences.Instance.GetPlayer().GetEquippableEquipped() && CommonReferences.Instance.GetPlayer().GetEquippableEquipped() is Usable && !((Usable)CommonReferences.Instance.GetPlayer().GetEquippableEquipped()).IsUsed())
		{
			CommonReferences.Instance.GetPlayer().UseEquippedUsable();
		}
	}

	private void AnimEventDropUsable()
	{
		CommonReferences.Instance.GetPlayer().DropEquippedEquippable(base.transform.forward * 5f);
	}

	private void AnimEventPlayAudioFootstep()
	{
		CommonReferences.Instance.GetPlayer().AudioPlayRandomFootstep();
	}

	private void AnimEventReloadOneBullet()
	{
		Gun gun = (Gun)CommonReferences.Instance.GetPlayer().GetEquippableEquipped();
		if (gun == null)
		{
			return;
		}
		if (gun.GetAmmoLeft() <= 0 && !gun.GetIsAmmoInfinite())
		{
			CommonReferences.Instance.GetPlayer().InterruptReload();
			return;
		}
		gun.ReloadOneBullet();
		if (gun.GetAmmoMagazineLeft() == gun.GetAmmoMagazineMax() || gun.GetAmmoLeft() <= 0)
		{
			CommonReferences.Instance.GetPlayer().InterruptReload();
		}
	}

	public void PlaceClothesOnTopOfBodyParts()
	{
		foreach (ClothingPiece item in GetClothingPiecesAttached())
		{
			UpdateClothingPieceSortingOrder(item);
		}
	}

	private void UpdateClothingPieceSortingOrder(ClothingPiece i_clothingPiece)
	{
		int sortingOrder = 1 + i_clothingPiece.GetSortingNumberClothingPiece();
		if (i_clothingPiece.IsAttachToBoneInsteadOfBodyPart())
		{
			sortingOrder += GetBone(i_clothingPiece.GetBoneToAttachTo()).GetBodyPartPlayer().GetSortingGroup().sortingOrder;
		}
		i_clothingPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
	}

	public List<BonePlayer> GetAllBonesPlayer()
	{
		List<BonePlayer> list = new List<BonePlayer>();
		foreach (Bone bone in m_bones)
		{
			list.Add((BonePlayer)bone);
		}
		return list;
	}

	private List<BodyPartPlayer> GetAllBodyPartsPlayer()
	{
		List<BodyPartPlayer> list = new List<BodyPartPlayer>();
		foreach (Bone bone in m_bones)
		{
			list.Add((BodyPartPlayer)bone.GetBodyPart());
		}
		return list;
	}

	public void EquipClothing(Clothing i_clothing)
	{
		if (IsClothingEquipped(i_clothing))
		{
			return;
		}
		m_clothesEquipped.Add(i_clothing);
		i_clothing.ApplyModEffects(CommonReferences.Instance.GetPlayer());
		Clothing clothing = Object.Instantiate(i_clothing);
		clothing.Initialize();
		foreach (ClothingPiece clothingPiece in clothing.GetClothingPieces())
		{
			if (clothingPiece.IsAttachToBoneInsteadOfBodyPart())
			{
				clothingPiece.transform.SetParent(GetBone(clothingPiece.GetBoneToAttachTo()).transform);
			}
			else
			{
				clothingPiece.transform.SetParent(GetBone(clothingPiece.GetBoneToAttachTo()).GetBodyPart().transform);
			}
			if (clothingPiece.IsHideBodyPartAttachedTo())
			{
				GetBone(clothingPiece.GetBoneToAttachTo()).GetBodyPart().GetComponent<SpriteRenderer>().enabled = false;
			}
			clothingPiece.transform.localPosition = clothingPiece.GetLocalPosition();
			clothingPiece.transform.localEulerAngles = new Vector3(0f, 0f, clothingPiece.GetLocalEulerAngleZ());
			clothingPiece.transform.localScale = Vector3.one;
			UpdateClothingPieceSortingOrder(clothingPiece);
			clothingPiece.gameObject.SetActive(value: true);
		}
		HandleHat();
		Object.Destroy(clothing.gameObject);
	}

	public void RemoveClothing(Clothing i_clothing)
	{
		foreach (ClothingPiece item in GetClothingPiecesAttached())
		{
			if (item.GetIdClothing() == i_clothing.GetId())
			{
				if (item.IsHideBodyPartAttachedTo())
				{
					GetBone(item.GetBoneToAttachTo()).GetBodyPart().GetComponent<SpriteRenderer>().enabled = true;
				}
				Object.Destroy(item.gameObject);
			}
		}
		m_clothesEquipped.Remove(i_clothing);
		i_clothing.RemoveModEffects(CommonReferences.Instance.GetPlayer());
		StartCoroutine(CoroutineWaitForHandleHat());
	}

	private IEnumerator CoroutineWaitForHandleHat()
	{
		yield return new WaitForEndOfFrame();
		HandleHat();
	}

	public void RemoveAllClothing()
	{
		List<Clothing> list = new List<Clothing>();
		foreach (Clothing item in m_clothesEquipped)
		{
			list.Add(item);
		}
		foreach (Clothing item2 in list)
		{
			RemoveClothing(item2);
		}
	}

	public List<ClothingPiece> GetClothingPiecesAttached()
	{
		List<ClothingPiece> list = new List<ClothingPiece>();
		ClothingPiece[] componentsInChildren = GetComponentsInChildren<ClothingPiece>();
		foreach (ClothingPiece item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public List<ClothingPiece> GetClothingPiecesAttachedFromClothing(Clothing i_clothing)
	{
		List<ClothingPiece> list = new List<ClothingPiece>();
		foreach (ClothingPiece item in GetClothingPiecesAttached())
		{
			if (item.GetIdClothing() == i_clothing.GetId())
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<Clothing> GetClothesEquipped()
	{
		return m_clothesEquipped;
	}

	public int GetMissingClothingPieceCount()
	{
		int missing = 0;
		foreach (Clothing clothing in m_clothesEquipped)
		{
			int attached = GetClothingPiecesAttachedFromClothing(clothing).Count;
			missing += Mathf.Max(0, clothing.GetClothingPieces().Count - attached);
		}
		return missing;
	}

	public int GetClothingRepairCost()
	{
		int cost = 0;
		foreach (Clothing clothing in m_clothesEquipped)
		{
			int missing = Mathf.Max(0, clothing.GetClothingPieces().Count - GetClothingPiecesAttachedFromClothing(clothing).Count);
			cost += missing * GetRepairCostPerPiece(clothing.GetCatergoryClothing());
		}
		return cost;
	}

	public int RepairEquippedClothing()
	{
		List<Clothing> damaged = new List<Clothing>();
		int repairedPieces = 0;
		foreach (Clothing clothing in m_clothesEquipped)
		{
			int missing = Mathf.Max(0, clothing.GetClothingPieces().Count - GetClothingPiecesAttachedFromClothing(clothing).Count);
			if (missing <= 0) continue;
			damaged.Add(clothing);
			repairedPieces += missing;
		}
		foreach (Clothing clothing in damaged)
		{
			RemoveClothing(clothing);
			EquipClothing(clothing);
		}
		return repairedPieces;
	}

	private static int GetRepairCostPerPiece(ClothingCategory i_category)
	{
		switch (i_category)
		{
		case ClothingCategory.Upper: return 40;
		case ClothingCategory.Lower: return 50;
		case ClothingCategory.Stockings: return 25;
		case ClothingCategory.Sleeves:
		case ClothingCategory.Shoes: return 15;
		case ClothingCategory.Hair:
		case ClothingCategory.Hat: return 0;
		default: return 25;
		}
	}

	public bool IsClothingEquipped(Clothing i_clothingToCheck)
	{
		if (m_clothesEquipped.Contains(i_clothingToCheck))
		{
			return true;
		}
		return false;
	}

	public void UpdateClothesEquippedAlready()
	{
		ClothingPiece[] componentsInChildren = GetComponentsInChildren<ClothingPiece>();
		foreach (ClothingPiece clothingPiece in componentsInChildren)
		{
			Clothing clothing = Library.Instance.Clothes.GetClothing(clothingPiece.GetIdClothing());
			if (!IsClothingEquipped(clothing))
			{
				m_clothesEquipped.Add(clothing);
				clothing.ApplyModEffects(CommonReferences.Instance.GetPlayer());
			}
		}
		HandleHat();
	}

	private void HandleHat()
	{
		bool flag = false;
		foreach (ClothingPiece item in GetClothingPiecesAttached())
		{
			if (item is ClothingPieceHat)
			{
				flag |= ((ClothingPieceHat)item).IsHidesHair();
			}
		}
		foreach (ClothingPiece item2 in GetClothingPiecesAttached())
		{
			if (Library.Instance.Clothes.GetClothing(item2.GetIdClothing()).GetCatergoryClothing() == ClothingCategory.Hair)
			{
				if (flag)
				{
					item2.Hide();
				}
				else
				{
					item2.Show();
				}
			}
		}
	}

	public void DropOrDestroyRandomClothingPiece()
	{
		if (GetClothingPiecesAttached().Count != 0)
		{
			int index = Random.Range(0, GetClothingPiecesAttached().Count);
			GetClothingPiecesAttached()[index].DropOrDestroy(i_isIgnoreAudipRip: false);
			HandleHat();
		}
	}

	public void HandleOralThrust()
	{
		List<ClothingPiece> clothingPiecesAttached = GetClothingPiecesAttached();
		for (int i = 0; i < clothingPiecesAttached.Count; i++)
		{
			if (clothingPiecesAttached[i].IsDropOnOralThrust())
			{
				clothingPiecesAttached[i].Drop();
			}
			if (clothingPiecesAttached[i].IsDestroyOnOralThrust())
			{
				clothingPiecesAttached[i].Destroy();
			}
		}
	}

	public void SetSkinColor(SkinColor i_skinColor)
	{
		foreach (BodyPartPlayer item in GetAllBodyPartsPlayer())
		{
			item.SetSkinColor(i_skinColor);
		}
		m_skinColor = i_skinColor;
		ApplyAuxiliarySkinTint(i_skinColor);
		foreach (ModPersistentPlayerAttachment attachment in GetComponentsInChildren<ModPersistentPlayerAttachment>(includeInactive: true))
			attachment.RefreshForSkinTone();
	}

	private void ApplyAuxiliarySkinTint(SkinColor i_skinColor)
	{
		m_auxiliarySkinTint = SkinTonePalette.GetRendererTint(i_skinColor);
		m_auxiliarySkinRenderers.Clear();
		foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
		{
			if (renderer.name != "bp_ear" && renderer.name != "eyelidLowerHideEye"
				&& renderer.name != "face_eyelidLowerHideEye") continue;
			m_auxiliarySkinRenderers.Add(renderer);
		}
		RefreshAuxiliarySkinTint();
	}

	private void LateUpdate()
	{
		// Facial animations write these renderer colors. Reapply only RGB after the
		// Animator so eyelid/blush alpha animation remains untouched.
		RefreshAuxiliarySkinTint();
	}

	private void RefreshAuxiliarySkinTint()
	{
		foreach (SpriteRenderer renderer in m_auxiliarySkinRenderers)
		{
			if (renderer == null) continue;
			Color color = renderer.color;
			color.r = m_auxiliarySkinTint.r;
			color.g = m_auxiliarySkinTint.g;
			color.b = m_auxiliarySkinTint.b;
			renderer.color = color;
		}
	}

	public SkinColor GetSkinColor()
	{
		return m_skinColor;
	}

	public void SetEyeColor(EyeColor i_eyeColor)
	{
		GetManagerFacePlayer().SetColorIris(i_eyeColor);
		m_eyeColor = i_eyeColor;
	}

	public EyeColor GetEyeColor()
	{
		return m_eyeColor;
	}
}
