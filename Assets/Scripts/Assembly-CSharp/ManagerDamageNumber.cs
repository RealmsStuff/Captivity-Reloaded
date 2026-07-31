using UnityEngine;

public class ManagerDamageNumber : MonoBehaviour
{
	[SerializeField]
	private DamageNumber m_damageNumberDefault;

	public void CreateDamageNumber(float i_dmgNumber, Vector2 i_posSpawn)
	{
		DamageNumber damageNumber = Object.Instantiate(m_damageNumberDefault, base.transform);
		damageNumber.Initialize(i_dmgNumber, i_posSpawn);
		damageNumber.gameObject.SetActive(value: true);
	}
}
