using System.Collections;
using CaptivityReloaded.Modding;
using UnityEngine;

// Runs after the default-order LateUpdates that pose the player, so the aim's lean of the upper
// body is laid over their result and the arms are solved from where the shoulder ends up.
[DefaultExecutionOrder(50)]
public class ManagerArmWeaponPlayer : MonoBehaviour
{
	// Off: the arms are posed by the fixed angles the game always used (no solver, no upper body). For comparing.
	public static bool UseAimSolver = true;

	// How long the aim takes to come in and go out when aiming starts or stops (reload, equip, exposing).
	private const float AimBlendSeconds = 0.1f;
	private const float PullSmoothSeconds = 0.06f;
	private const float LeanSmoothSeconds = 0.09f;
	// How much of the aim's pitch each bone of the upper body takes: about 12 degrees of torso and 27 of head when
	// the gun points straight up.
	public const float LeanSpine = 0.045f, LeanChest = 0.085f, LeanNeck = 0.06f, LeanHead = 0.11f;

	private sealed class LeanBone
	{
		public Transform Bone;
		public float Gain;
		public bool Written;
		public Quaternion Posed, Base;
	}

	private LeanBone[] m_leanBones;
	private bool m_isRigRead;
	private AimArmRig m_rig;
	private float m_aimWeight;
	private float m_aimAngle;
	private float m_pull, m_pullVelocity;
	private float m_leanPitch, m_leanVelocity;
	private AimSolution m_solution;
	private float m_maxPull;
	// How far along the aim a stick or touch aim looks for an enemy to draw the gun in for.
	private const float AutoPullRange = 3f;
	private bool m_hasSolution;

	[SerializeField]
	private GameObject m_rArmUpper;

	[SerializeField]
	private GameObject m_rArmLower;

	[SerializeField]
	private GameObject m_rHand;

	[SerializeField]
	private GameObject m_pointHoldWeaponR;

	[SerializeField]
	private GameObject m_lArmUpper;

	[SerializeField]
	private GameObject m_lArmLower;

	[SerializeField]
	private GameObject m_lHand;

	[SerializeField]
	private GameObject m_pointHoldWeaponL;

	private Player m_player;

	private bool m_isShooting;

	private Weapon m_weapon;

	private GunHoldType m_holdTypeCurrent;

	private float m_angleToAddToArmLower;

	private Coroutine m_coroutineShootGun;

	private float m_angleToAddToUpperArmOneHanded;

	private float m_angleToAddToLowerArmOneHanded;

	private float m_angleToAddToUpperArmRRifle;

	private float m_angleToAddToLowerArmRRifle;

	private float m_angleToAddToUpperArmLRifle;

	private float m_angleToAddToLowerArmLRifle;

	public void Start()
	{
		m_player = CommonReferences.Instance.GetPlayer();
	}

	// Takes last frame's lean back off before the Animator poses the new frame, so the lean can never pile up on a
	// bone the Animator leaves alone and everything else that reads the pose sees it clean.
	private void Update()
	{
		RemoveLean();
	}

	private void OnDisable()
	{
		RemoveLean();
		m_aimWeight = 0f;
		m_hasSolution = false;
	}

	private void LateUpdate()
	{
		bool isAiming = m_player.GetEquippableEquipped() != null && !CommonReferences.Instance.GetPlayerController().GetIsForceIgnoreInput() && m_player.GetStatePlayerCurrent() != StatePlayer.BeingRaped && m_player.GetStateActorCurrent() != StateActor.Ragdoll && m_player.GetStatePlayerCurrent() != StatePlayer.Dead && m_player.GetStatePlayerCurrent() != StatePlayer.BeingRaped && m_player.GetStatePlayerCurrent() != StatePlayer.Labor && !m_player.GetIsReloading() && !m_player.GetIsEquipping() && !m_player.IsExposing();
		if (!UseAimSolver)
		{
			m_aimWeight = 0f;
			m_hasSolution = false;
			if (isAiming) HandleAim();
			return;
		}
		Gun gun = m_player.GetEquippableEquipped() as Gun;
		// Ragdolled, held, in labour or dead the player is posed by something else entirely: nothing of the aim stays.
		if (IsPosedByOthers())
		{
			m_aimWeight = 0f;
			m_hasSolution = false;
			return;
		}
		bool isSolving = isAiming && gun != null;
		m_aimWeight = Mathf.MoveTowards(m_aimWeight, isSolving ? 1f : 0f, Time.deltaTime / AimBlendSeconds);
		// The upper body leans first, by the aim it had last frame, so the arms are solved from where that puts the
		// shoulder. Reloading, equipping and exposing take the arms back at once, as they always have; the lean
		// fades out under them, and arms and lean fade in together when the aim returns.
		if (m_hasSolution && m_aimWeight > 0f) ApplyLean(m_leanPitch * m_aimWeight);
		if (isSolving)
		{
			m_weapon = gun;
			m_holdTypeCurrent = gun.GetHoldTypeGun();
			SolveAim(gun);
			ApplyArms(m_aimWeight);
		}
		else if (m_aimWeight <= 0f) m_hasSolution = false;
	}

	private bool IsPosedByOthers()
	{
		return m_player.GetStatePlayerCurrent() == StatePlayer.BeingRaped || m_player.GetStateActorCurrent() == StateActor.Ragdoll
			|| m_player.GetStatePlayerCurrent() == StatePlayer.Dead || m_player.GetStatePlayerCurrent() == StatePlayer.Labor;
	}

	private void ReadRig()
	{
		if (m_isRigRead) return;
		m_isRigRead = true;
		m_rig = new AimArmRig
		{
			Elbow = m_rArmLower.transform.localPosition, Wrist = m_rHand.transform.localPosition,
			Hold = m_pointHoldWeaponR.transform.localPosition,
			HoldRotation = Mathf.DeltaAngle(0f, m_pointHoldWeaponR.transform.localEulerAngles.z)
		};
		SkeletonPlayer skeleton = m_player.GetSkeletonPlayer();
		m_leanBones = new[]
		{
			Lean(skeleton, BoneTypePlayer.Spine, LeanSpine), Lean(skeleton, BoneTypePlayer.Chest, LeanChest),
			Lean(skeleton, BoneTypePlayer.Neck, LeanNeck), Lean(skeleton, BoneTypePlayer.Head, LeanHead)
		};
	}

	private static LeanBone Lean(SkeletonPlayer i_skeleton, BoneTypePlayer i_type, float i_gain)
	{
		BonePlayer bone = i_skeleton == null ? null : i_skeleton.GetBone(i_type);
		return new LeanBone { Bone = bone == null ? null : bone.transform, Gain = i_gain };
	}

	public static AimHoldPose GetHoldPose(Gun i_gun)
	{
		if (i_gun.GetModHoldPose() != null) return AimHoldPose.FromDefinition(i_gun.GetModHoldPose());
		switch (i_gun.GetHoldTypeGun())
		{
		case GunHoldType.OneHanded: return AimHoldPose.OneHanded;
		case GunHoldType.TwoHanded1: return AimHoldPose.TwoHanded1;
		case GunHoldType.TwoHanded2: return AimHoldPose.TwoHanded2;
		case GunHoldType.TwoHanded3: return AimHoldPose.TwoHanded3;
		// PistolTwoHanded never had a pose of its own: every offset zero, both arms, as the old switch left it.
		default: return new AimHoldPose { UseLeftHand = true, Authored = true };
		}
	}

	// A gun without a stock has nothing braced against the shoulder, so it can be drawn in toward the body.
	// A pose a pack authored is kept as written whatever the gun says about a stock.
	public static bool CanPullIn(Gun i_gun)
	{
		return i_gun.GetModHoldPose() == null && !i_gun.HasStock();
	}

	// From the shoulder to the aim point in body space: x is ahead of the player whichever way they face.
	private Vector2 GetAimFromShoulder()
	{
		Vector3 shoulder = m_rArmUpper.transform.position;
		Camera camera = Camera.main;
		Vector3 onScreen = camera.WorldToScreenPoint(shoulder);
		Vector3 aim = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(shoulder);
		Vector3 world = camera.ScreenToWorldPoint(new Vector3(aim.x, aim.y, onScreen.z));
		return new Vector2((world.x - shoulder.x) * (m_player.GetIsFacingLeft() ? -1f : 1f), world.y - shoulder.y);
	}

	private void SolveAim(Gun i_gun)
	{
		ReadRig();
		Vector2 toAim = GetAimFromShoulder();
		bool wasAiming = m_hasSolution;
		m_aimAngle = PlayerAimSolver.AimAngle(toAim, wasAiming ? m_aimAngle : Mathf.Clamp(PlayerAimSolver.Angle(toAim), -PlayerAimSolver.MaxPitch, PlayerAimSolver.MaxPitch), Time.deltaTime);
		AimHoldPose pose = GetHoldPose(i_gun);
		bool hasMuzzle = i_gun.TryGetMuzzleInGunSpace(out Vector2 muzzle);
		float distance = toAim.magnitude;
		bool canPullIn = hasMuzzle && CanPullIn(i_gun);
		float pull = canPullIn ? PlayerAimSolver.DesiredPull(m_rig, pose, muzzle, GetPullDistance(toAim, distance)) : 0f;
		m_maxPull = canPullIn ? PlayerAimSolver.MaxPull(m_rig, pose) : 0f;
		float leanPitch = Mathf.Clamp(m_aimAngle, -90f, 90f);
		if (wasAiming)
		{
			m_pull = Mathf.SmoothDamp(m_pull, pull, ref m_pullVelocity, PullSmoothSeconds);
			m_leanPitch = Mathf.SmoothDamp(m_leanPitch, leanPitch, ref m_leanVelocity, LeanSmoothSeconds);
		}
		else
		{
			m_pull = pull;
			m_pullVelocity = 0f;
			m_leanPitch = leanPitch;
			m_leanVelocity = 0f;
		}
		m_solution = PlayerAimSolver.Solve(m_rig, pose, hasMuzzle, muzzle, m_aimAngle, distance, m_pull);
		m_hasSolution = true;
	}

	// How far away the thing being aimed at is, for deciding how far to draw the gun in. The mouse and the
	// keyboard's auto-aim put the cursor on it. A stick or a touch pad only gives a direction, with the cursor a
	// fixed way out: there the gun is drawn in for the nearest living enemy along the aim instead.
	private float GetPullDistance(Vector2 i_toAim, float i_distance)
	{
		ManagerInput input = CommonReferences.Instance.GetManagerInput();
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		bool hasCursorDistance = !input.IsControllerAiming() && (controller == null || !controller.GetIsMobileControlsEnabled());
		if (hasCursorDistance || i_distance < 1e-4f) return i_distance;
		Vector2 shoulder = m_rArmUpper.transform.position;
		Vector2 direction = new Vector2(i_toAim.x * (m_player.GetIsFacingLeft() ? -1f : 1f), i_toAim.y) / i_distance;
		float nearest = i_distance;
		foreach (RaycastHit2D hit in Physics2D.RaycastAll(shoulder, direction, AutoPullRange, LayerMask.GetMask("BodyPart")))
		{
			BodyPartActor part = hit.collider.GetComponent<BodyPartActor>();
			Actor owner = part == null ? null : part.GetOwner();
			if (owner == null || owner == m_player || owner.IsDead()) continue;
			nearest = Mathf.Min(nearest, hit.distance);
			break;
		}
		return nearest;
	}

	// Shots from a gun drawn in toward the body spread wider, up to PlayerAimSolver.PulledInSpread all the way in.
	public float GetSpreadMultiplier(Gun i_gun)
	{
		if (!UseAimSolver || !m_hasSolution || i_gun == null || i_gun != m_weapon) return 1f;
		return PlayerAimSolver.SpreadMultiplier(m_solution.Pull, m_maxPull);
	}

	private void ApplyArms(float i_weight)
	{
		float sign = m_player.GetIsFacingLeft() ? -1f : 1f;
		// The one-handed recoil kick, as it always was: half on the upper arm, all of it on the forearm and hand.
		float kick = m_holdTypeCurrent == GunHoldType.OneHanded ? m_angleToAddToArmLower : 0f;
		Pose(m_rArmUpper, m_solution.Right.Upper + kick / 2f, sign, i_weight);
		Pose(m_rArmLower, m_solution.Right.Lower + kick, sign, i_weight);
		Pose(m_rHand, m_solution.Right.Hand + kick, sign, i_weight);
		if (!m_solution.UseLeft) return;
		Pose(m_lArmUpper, m_solution.Left.Upper, sign, i_weight);
		Pose(m_lArmLower, m_solution.Left.Lower, sign, i_weight);
		Pose(m_lHand, m_solution.Left.Hand, sign, i_weight);
	}

	// Turns a bone to a body-space rotation: i_sign is -1 while the player faces left, where the whole body is mirrored.
	public static void Pose(GameObject i_bone, float i_bodyAngle, float i_sign, float i_weight)
	{
		Quaternion target = Quaternion.AngleAxis(i_sign * i_bodyAngle, Vector3.forward);
		i_bone.transform.rotation = i_weight >= 1f ? target : Quaternion.Slerp(i_bone.transform.rotation, target, i_weight);
	}

	// Leans spine, chest, neck and head with the aim, on top of whatever pose they hold this frame.
	private void ApplyLean(float i_pitch)
	{
		if (m_leanBones == null) return;
		foreach (LeanBone lean in m_leanBones)
		{
			if (lean.Bone == null) continue;
			float turn = lean.Gain * i_pitch;
			if (Mathf.Abs(turn) < 0.01f) continue;
			lean.Base = lean.Bone.localRotation;
			lean.Posed = lean.Base * Quaternion.Euler(0f, 0f, turn);
			lean.Bone.localRotation = lean.Posed;
			lean.Written = true;
		}
	}

	private void RemoveLean()
	{
		if (m_leanBones == null) return;
		foreach (LeanBone lean in m_leanBones)
		{
			if (!lean.Written) continue;
			lean.Written = false;
			// Only if the bone still holds what was written: a ragdoll or a finisher that took it over keeps its pose.
			if (lean.Bone != null && Quaternion.Angle(lean.Bone.localRotation, lean.Posed) < 0.01f) lean.Bone.localRotation = lean.Base;
		}
	}

	// Which way the barrel points in the world, without the recoil kick: where a shot from the muzzle goes when the
	// cursor is too close to aim at. False while the arms are not on the solved aim.
	public bool TryGetBarrelDirection(out Vector2 o_direction)
	{
		o_direction = Vector2.zero;
		if (!UseAimSolver || !m_hasSolution || m_aimWeight < 1f) return false;
		Vector2 body = PlayerAimSolver.Direction(m_solution.GunAngle);
		o_direction = new Vector2(m_player.GetIsFacingLeft() ? -body.x : body.x, body.y);
		return true;
	}

	// The pose the solver last gave the arms, for whoever needs to know where the gun really is.
	public bool TryGetAimSolution(out AimSolution o_solution)
	{
		o_solution = m_solution;
		return m_hasSolution && m_aimWeight > 0f;
	}

	private void HandleAim()
	{
		m_weapon = m_player.GetEquippableEquipped();
		if (m_weapon is Gun)
		{
			HandleHoldType();
		}
	}

	private void HandleHoldType()
	{
		Gun gun = (Gun)m_weapon;
		m_player = CommonReferences.Instance.GetPlayer();
		m_holdTypeCurrent = gun.GetHoldTypeGun();
		if (gun.GetModHoldPose() != null)
		{
			HandleCustomPose(gun.GetModHoldPose());
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		switch (m_holdTypeCurrent)
		{
		case GunHoldType.OneHanded:
			HandleHoldOneHanded();
			return;
		case GunHoldType.TwoHanded1:
			num = -45f;
			num2 = 65f;
			num3 = 90f;
			num4 = 60f;
			num5 = 75f;
			num6 = 80f;
			break;
		case GunHoldType.TwoHanded2:
			num = -65f;
			num2 = 60f;
			num3 = 90f;
			num4 = 60f;
			num5 = 75f;
			num6 = 80f;
			break;
		case GunHoldType.TwoHanded3:
			num = -65f;
			num2 = 100f;
			num3 = 90f;
			num4 = 65f;
			num5 = 100f;
			num6 = 90f;
			break;
		}
		Vector3 vector = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmUpper().transform.position) - Camera.main.WorldToScreenPoint(GetRArmUpper().transform.position);
		float num7 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num7 += num;
		float num8 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num8 += num2;
		float num9 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num9 += num3;
		if (m_player.GetIsFacingLeft())
		{
			GetRArmUpper().transform.rotation = Quaternion.AngleAxis(0f - num7, Vector3.forward);
			GetRArmLower().transform.rotation = Quaternion.AngleAxis(0f - num8, Vector3.forward);
			GetRHand().transform.rotation = Quaternion.AngleAxis(0f - num9, Vector3.forward);
		}
		else
		{
			GetRArmUpper().transform.rotation = Quaternion.AngleAxis(num7, Vector3.forward);
			GetRArmLower().transform.rotation = Quaternion.AngleAxis(num8, Vector3.forward);
			GetRHand().transform.rotation = Quaternion.AngleAxis(num9, Vector3.forward);
		}
		float num10 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num10 += num4;
		float num11 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num11 += num5;
		float num12 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num12 += num6;
		if (m_player.GetIsFacingLeft())
		{
			GetLArmUpper().transform.rotation = Quaternion.AngleAxis(0f - num10, Vector3.forward);
			GetLArmLower().transform.rotation = Quaternion.AngleAxis(0f - num11, Vector3.forward);
			GetLHand().transform.rotation = Quaternion.AngleAxis(0f - num12, Vector3.forward);
		}
		else
		{
			GetLArmUpper().transform.rotation = Quaternion.AngleAxis(num10, Vector3.forward);
			GetLArmLower().transform.rotation = Quaternion.AngleAxis(num11, Vector3.forward);
			GetLHand().transform.rotation = Quaternion.AngleAxis(num12, Vector3.forward);
		}
	}

	private void HandleCustomPose(CaptivityReloaded.Modding.WeaponHoldPoseDefinition i_pose)
	{
		Vector3 aim = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmUpper().transform.position)
			- Camera.main.WorldToScreenPoint(GetRArmUpper().transform.position);
		bool left = m_player.GetIsFacingLeft();
		float angle = Mathf.Atan2(aim.y, left ? -aim.x : aim.x) * Mathf.Rad2Deg;
		float sign = left ? -1f : 1f;
		GetRArmUpper().transform.rotation = Quaternion.AngleAxis(sign * (angle + i_pose.RightUpper), Vector3.forward);
		GetRArmLower().transform.rotation = Quaternion.AngleAxis(sign * (angle + i_pose.RightLower), Vector3.forward);
		GetRHand().transform.rotation = Quaternion.AngleAxis(sign * (angle + i_pose.RightHand), Vector3.forward);
		if (!i_pose.UseLeftHand) return;
		GetLArmUpper().transform.rotation = Quaternion.AngleAxis(sign * (angle + i_pose.LeftUpper), Vector3.forward);
		GetLArmLower().transform.rotation = Quaternion.AngleAxis(sign * (angle + i_pose.LeftLower), Vector3.forward);
		GetLHand().transform.rotation = Quaternion.AngleAxis(sign * (angle + i_pose.LeftHand), Vector3.forward);
	}

	private void HandleHoldOneHanded()
	{
		HandleArmOneHanded();
	}

	private void HandleArmOneHanded()
	{
		Vector3 vector = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmUpper().transform.position) - Camera.main.WorldToScreenPoint(GetRArmUpper().transform.position);
		float num = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num = ((!CommonReferences.Instance.GetPlayer().GetIsFacingLeft()) ? (num + m_angleToAddToArmLower / 2f) : (num + m_angleToAddToArmLower / 2f));
		num += 60f;
		float num2 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num2 = ((!CommonReferences.Instance.GetPlayer().GetIsFacingLeft()) ? (num2 + m_angleToAddToArmLower) : (num2 + m_angleToAddToArmLower));
		num2 += 90f;
		GetRHand().transform.up = GetRArmLower().transform.up;
		if (m_player.GetIsFacingLeft())
		{
			GetRArmUpper().transform.rotation = Quaternion.AngleAxis(0f - num, Vector3.forward);
			GetRArmLower().transform.rotation = Quaternion.AngleAxis(0f - num2, Vector3.forward);
		}
		else
		{
			GetRArmUpper().transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			GetRArmLower().transform.rotation = Quaternion.AngleAxis(num2, Vector3.forward);
		}
	}

	private void HandleHoldSmg()
	{
		HandleSmgHoldRightArm();
		HandleSmgHoldLeftArm();
	}

	private void HandleSmgHoldRightArm()
	{
		Vector3 vector = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmUpper().transform.position) - Camera.main.WorldToScreenPoint(GetRArmUpper().transform.position);
		vector.Normalize();
		float num = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num -= 45f;
		float num2 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num2 += 65f;
		float num3 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num3 += 90f;
		if (m_player.GetIsFacingLeft())
		{
			GetRArmUpper().transform.rotation = Quaternion.AngleAxis(0f - num, Vector3.forward);
			GetRArmLower().transform.rotation = Quaternion.AngleAxis(0f - num2, Vector3.forward);
			GetRHand().transform.rotation = Quaternion.AngleAxis(0f - num3, Vector3.forward);
		}
		else
		{
			GetRArmUpper().transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			GetRArmLower().transform.rotation = Quaternion.AngleAxis(num2, Vector3.forward);
			GetRHand().transform.rotation = Quaternion.AngleAxis(num3, Vector3.forward);
		}
	}

	private void HandleSmgHoldLeftArm()
	{
		Vector3 vector = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmUpper().transform.position) - Camera.main.WorldToScreenPoint(GetRArmUpper().transform.position);
		float num = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num += 60f;
		float num2 = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
		num2 += 75f;
		if (m_player.GetIsFacingLeft())
		{
			GetLArmUpper().transform.rotation = Quaternion.AngleAxis(0f - num, Vector3.forward);
			GetLArmLower().transform.rotation = Quaternion.AngleAxis(0f - num2, Vector3.forward);
		}
		else
		{
			GetLArmUpper().transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			GetLArmLower().transform.rotation = Quaternion.AngleAxis(num2, Vector3.forward);
		}
		GetLHand().transform.up = GetLArmLower().transform.up;
	}

	private void PointGunTowardCrosshair()
	{
		if (!m_isShooting)
		{
			Vector3 vector = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(m_weapon.transform.position) - Camera.main.WorldToScreenPoint(m_weapon.transform.position);
			float num = ((!m_player.GetIsFacingLeft()) ? (Mathf.Atan2(vector.y, vector.x) * 57.29578f) : (Mathf.Atan2(vector.y, 0f - vector.x) * 57.29578f));
			if (m_player.GetIsFacingLeft())
			{
				m_weapon.transform.rotation = Quaternion.AngleAxis(0f - num, Vector3.forward);
			}
			else
			{
				m_weapon.transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			}
		}
	}

	public void ShootGun(Gun i_gun)
	{
		ShootGun(i_gun, 1f);
	}

	public void ShootGun(Gun i_gun, float i_recoilMultiplier)
	{
		if (i_gun.GetHoldTypeGun() == GunHoldType.OneHanded)
		{
			if (m_coroutineShootGun != null)
			{
				StopCoroutine(m_coroutineShootGun);
			}
			m_angleToAddToArmLower = 0f;
			m_coroutineShootGun = StartCoroutine(CoroutineShootGun(i_gun, Mathf.Max(0f, i_recoilMultiplier)));
		}
	}

	private IEnumerator CoroutineShootGun(Gun i_gun, float i_recoilMultiplier)
	{
		m_isShooting = true;
		_ = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmUpper().transform.position) - Camera.main.WorldToScreenPoint(GetRArmUpper().transform.position);
		Vector3 vector = CommonReferences.Instance.GetManagerInput().GetAimScreenPosition(GetRArmLower().transform.position) - Camera.main.WorldToScreenPoint(GetRArmLower().transform.position);
		_ = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		float l_angleFrom = i_gun.GetDamage() * 3 * i_recoilMultiplier;
		float l_angleTo = 0f;
		float l_timeToMove = 0.15f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float angleToAddToArmLower = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_angleFrom, l_angleTo, i_time);
			m_angleToAddToArmLower = angleToAddToArmLower;
			yield return new WaitForFixedUpdate();
		}
		m_angleToAddToArmLower = 0f;
		m_isShooting = false;
	}

	public GameObject GetRArmUpper()
	{
		return m_rArmUpper;
	}

	public GameObject GetRArmLower()
	{
		return m_rArmLower;
	}

	public GameObject GetRHand()
	{
		return m_rHand;
	}

	public GameObject GetLArmUpper()
	{
		return m_lArmUpper;
	}

	public GameObject GetLArmLower()
	{
		return m_lArmLower;
	}

	public GameObject GetLHand()
	{
		return m_lHand;
	}

	public Vector2 GetPosHoldWeapon()
	{
		return m_pointHoldWeaponR.transform.position;
	}

	public void ShowCurrentArm()
	{
		m_rArmLower.gameObject.SetActive(value: true);
	}

	public GameObject GetPointHoldWeapon()
	{
		return m_pointHoldWeaponR;
	}
}
