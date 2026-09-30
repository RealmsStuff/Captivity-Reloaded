using System.Collections;
using UnityEngine;

public class ManagerArmWeaponPlayer : MonoBehaviour
{
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

	private void LateUpdate()
	{
		if (m_player.GetEquippableEquipped() != null && !CommonReferences.Instance.GetPlayerController().GetIsForceIgnoreInput() && m_player.GetStatePlayerCurrent() != StatePlayer.BeingRaped && m_player.GetStateActorCurrent() != StateActor.Ragdoll && m_player.GetStatePlayerCurrent() != StatePlayer.Dead && m_player.GetStatePlayerCurrent() != StatePlayer.BeingRaped && m_player.GetStatePlayerCurrent() != StatePlayer.Labor && !m_player.GetIsReloading() && !m_player.GetIsEquipping() && !m_player.IsExposing())
		{
			HandleAim();
		}
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
