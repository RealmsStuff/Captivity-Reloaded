using UnityEngine;


public class ResourceContainer : MonoBehaviour
{
	private static ResourceContainer l_instance;

	public ParticleSystem m_particleBloodExplosion;

	public ParticleSystem m_particleBloodLeak;

	public ParticleSystem m_particleAndroidExplosion;

	public ParticleSystem m_particleAndroidExplosionSmoke;

	public ParticleSystem m_particleAndroidExplosionElectricity;

	public ParticleSystem m_particleAndroidExplosionElectricityLeak;

	public ParticleSystem m_particleMaggotSteppedOn;

	public ParticleSystem m_particleGunShot;

	public GameObject m_particleDash;

	public GameObject m_particleLedge;

	public GameObject m_particleInteractable;

	public GameObject m_particleEquippable;

	public GameObject m_particleAmmo;

	public GameObject m_particleKey;

	public GameObject m_particleUsable;

	public GameObject m_particleSmokeElectrocution;

	public GameObject m_particleFire;

	public GameObject m_objSpawn;

	public GameObject m_objEndStage;

	public UnityEngine.Rendering.Universal.Light2D m_lightMuzzleFlash;

	public SpriteRenderer m_lineShoot;

	public static ResourceContainer Resources
	{
		get
		{
			if (l_instance == null)
			{
				l_instance = Object.FindObjectOfType<ResourceContainer>();
				if (l_instance == null)
				{
					l_instance = new GameObject("ResourceContainer").AddComponent<ResourceContainer>();
				}
			}
			return l_instance;
		}
	}
}
