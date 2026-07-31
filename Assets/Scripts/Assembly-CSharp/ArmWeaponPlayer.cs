using UnityEngine;

public class ArmWeaponPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject m_pointHoldWeapon;

	public Vector2 GetPosHoldWeapon()
	{
		return m_pointHoldWeapon.transform.position;
	}

	public Transform GetTransformHoldWeapon()
	{
		return m_pointHoldWeapon.transform;
	}
}
