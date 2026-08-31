using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gun : Weapon
{
	[Header("---Gun---")]
	[SerializeField]
	private GunHoldType m_gunHoldType;

	[SerializeField]
	private GunReloadType m_gunReloadType;

	[SerializeField]
	private bool m_isHideShootLine;

	[SerializeField]
	private float m_shootDistance01;

	[SerializeField]
	private int m_damage;

	[SerializeField]
	private int m_bulletsExtraPerShot;

	[SerializeField]
	protected float m_knockbackX;

	[SerializeField]
	protected float m_knockbackY;

	[SerializeField]
	protected int m_ammoMax;

	private int m_ammoLeft;

	[SerializeField]
	private int m_ammoMagazineMax;

	private int m_ammoLeftInMagazine;

	[SerializeField]
	private int m_penetration;

	[SerializeField]
	private bool m_isAmmoInfinite;

	[SerializeField]
	private bool m_isSemiFire;

	[SerializeField]
	private float m_delayBetweenShots;

	private float m_durationReload;

	[SerializeField]
	private List<GameObject> m_objectsToDropOnReload = new List<GameObject>();

	[SerializeField]
	private Vector2 m_forceObjectsDropOnReload;

	[SerializeField]
	private List<GameObject> m_objectsToDropOnShoot = new List<GameObject>();

	[SerializeField]
	private Vector2 m_forceObjectsDropOnShoot;

	[SerializeField]
	private float m_recoilBase01;

	[SerializeField]
	private float m_recoilFactorMovement01;

	[SerializeField]
	protected GameObject m_bulletSpawnPointDefault;

	[SerializeField]
	private List<Sprite> m_spritesMuzzleFlash;

	[SerializeField]
	private List<AudioClip> m_audiosShot = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosReload = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosUnique = new List<AudioClip>();

	[SerializeField]
	private bool m_isShakesCamera;

	[SerializeField]
	private bool m_isKnockbacksShooterFire;

	[SerializeField]
	private bool m_isKnockbacksShooterAltFire;

	private List<GameObject> m_objMuzzleFlashes = new List<GameObject>();

	private bool m_isOwnerPlayer;

	private bool m_isHasMuzzleFlash;

	private float m_distanceShootMax = 18.75f;

	protected Color m_colorLineShoot;

	protected int m_thicknessLineShoot;

	protected int m_framesLineShoot;

	protected override void Awake()
	{
		base.Awake();
		if (m_spritesMuzzleFlash.Count > 0)
		{
			m_isHasMuzzleFlash = true;
			CreateMuzzleFlashObject();
		}
		m_ammoLeftInMagazine = m_ammoMagazineMax;
		m_ammoLeft = m_ammoMax - m_ammoMagazineMax;
		switch (m_weaponType)
		{
		case WeaponType.Pistol:
			m_durationEquip = 0.5f;
			m_durationReload = 1f;
			break;
		case WeaponType.Smg:
			if (m_gunHoldType == GunHoldType.OneHanded)
			{
				m_durationEquip = 0.5f;
				m_durationReload = 1f;
			}
			else
			{
				m_durationEquip = 0.75f;
				m_durationReload = 1.5f;
			}
			break;
		case WeaponType.Shotgun:
			if (m_gunHoldType == GunHoldType.OneHanded)
			{
				m_durationEquip = 0.5f;
				m_durationReload = 1f;
			}
			else
			{
				m_durationEquip = 1f;
				m_durationReload = 2f;
			}
			break;
		case WeaponType.Rifle:
			m_durationEquip = 1f;
			m_durationReload = 2.5f;
			break;
		}
		if (m_gunReloadType == GunReloadType.SingleBarrel)
		{
			m_durationReload = 2f;
		}
		m_colorLineShoot = Color.white;
		m_thicknessLineShoot = 1;
		m_framesLineShoot = 1;
	}

	public override void Equip()
	{
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().Play("Idle");
		}
	}

	protected override bool HandleUse(bool i_isAltFire)
	{
		return true;
	}

	private void CreateMuzzleFlashObject()
	{
		int num = 0;
		foreach (Sprite item in m_spritesMuzzleFlash)
		{
			_ = item;
			GameObject gameObject = new GameObject("MuzzleFlash");
			gameObject.AddComponent<SpriteRenderer>();
			gameObject.GetComponent<SpriteRenderer>().sprite = m_spritesMuzzleFlash[num];
			gameObject.GetComponent<SpriteRenderer>().sortingOrder = 49;
			gameObject.transform.SetParent(m_bulletSpawnPointDefault.transform);
			gameObject.transform.position = m_bulletSpawnPointDefault.transform.position;
			gameObject.transform.localScale = m_bulletSpawnPointDefault.transform.localScale;
			gameObject.SetActive(value: false);
			m_objMuzzleFlashes.Add(gameObject);
			num++;
		}
	}

	protected virtual void OnEnable()
	{
		if (m_isHasMuzzleFlash)
		{
			foreach (GameObject objMuzzleFlash in m_objMuzzleFlashes)
			{
				objMuzzleFlash.SetActive(value: false);
			}
		}
		if (m_delayBetweenShots == 0f)
		{
			m_delayBetweenShots = 0.02f;
		}
	}

	public override void PickUp(Actor i_owner)
	{
		base.PickUp(i_owner);
		if (m_owner is Player)
		{
			m_isOwnerPlayer = true;
			base.OnDrop += HandleDrop;
		}
		else
		{
			m_isOwnerPlayer = false;
			m_ammoLeftInMagazine = m_ammoMagazineMax;
		}
	}

	public override void Drop(float i_powerDrop01)
	{
		base.Drop(i_powerDrop01);
		m_isOwnerPlayer = false;
	}

	public override void Drop(Vector2 i_forceDrop)
	{
		base.Drop(i_forceDrop);
		m_isOwnerPlayer = false;
	}

	private void OnPickup(PickUpable i_pickUpable)
	{
		if (m_isAmmoInfinite && i_pickUpable == this)
		{
			m_ammoLeftInMagazine = m_ammoMagazineMax;
		}
	}

	private void HandleDrop()
	{
		base.OnDrop -= HandleDrop;
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().Play("Idle");
		}
	}

	public List<Bullet> Shoot()
	{
		if (m_audiosShot.Count > 0)
		{
			m_audioSourceSFX.PlayOneShot(m_audiosShot[Random.Range(0, m_audiosShot.Count)]);
		}
		m_ammoLeftInMagazine--;
		if (m_isHasMuzzleFlash)
		{
			StartCoroutine(CoroutineMuzzleFlash());
		}
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().Play("Shoot", 0, 0f);
		}
		if (m_isShakesCamera)
		{
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().Shake(m_knockbackX / 5f + m_knockbackY / 5f, 0.05f);
		}
		return ShootBullets();
	}

	private List<Bullet> ShootBullets()
	{
		List<Bullet> list = new List<Bullet>();
		int mask = LayerMask.GetMask("Actor", "BodyPart", "Platform", "InvisibleWall", "Interactable", "Trap");
		Vector2 vector = m_owner.GetSkeleton().GetBone("rArmUpper").transform.position;
		Vector2 vector2 = CommonReferences.Instance.GetUtilityTools().GetPosMousePerspectiveCamera();
		float valueTotal = CommonReferences.Instance.GetPlayer().GetStat("DamageMultiplierGun").GetValueTotal();
		for (int i = 0; i < m_bulletsExtraPerShot + 1; i++)
		{
			Vector2 vector3 = vector2 - vector;
			vector3.Normalize();
			vector3 *= m_distanceShootMax * m_shootDistance01;
			float num = m_recoilBase01;
			if (CommonReferences.Instance.GetPlayer().IsStatusEffectAppliedAlready("BPrec"))
			{
				num *= 0.5f;
			}
			vector3 = Quaternion.AngleAxis(Random.Range((0f - num) * 20f, num * 20f), m_bulletSpawnPointDefault.transform.forward) * vector3;
			RaycastHit2D[] i_hits = Physics2D.RaycastAll(vector, vector3, m_distanceShootMax * m_shootDistance01, mask);
			Bullet bullet = new Bullet(this, m_owner, vector3, i_hits, (float)m_damage * valueTotal, m_knockbackX, m_knockbackY, m_penetration);
			bullet.HandleHits();
			list.Add(bullet);
		}
		return list;
	}

	public virtual void HandleActorHit(BodyPartActor i_bodyPart, Bullet i_bullet)
	{
	}

	public void LineShoot(Vector2 i_direction, Vector2 i_posEnd)
	{
		if (!m_isHideShootLine)
		{
			Vector2 i_posOrigin = m_owner.GetSkeleton().GetBone("rArmUpper").transform.position;
			StartCoroutine(CoroutineLineShoot(i_posOrigin, i_direction, i_posEnd));
		}
	}

	private IEnumerator CoroutineLineShoot(Vector2 i_posOrigin, Vector2 i_direction, Vector2 i_pointHit)
	{
		SpriteRenderer l_lineShoot = Object.Instantiate(ResourceContainer.Resources.m_lineShoot, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		l_lineShoot.transform.position = i_posOrigin;
		l_lineShoot.transform.right = i_direction;
		if (i_pointHit == Vector2.zero)
		{
			l_lineShoot.transform.localScale = new Vector3(m_distanceShootMax * m_shootDistance01 * 32f, 1f, 1f);
		}
		else
		{
			l_lineShoot.transform.localScale = new Vector3(Vector2.Distance(i_posOrigin, i_pointHit) * 32f, 1f, 1f);
		}
		l_lineShoot.transform.localScale = new Vector3(l_lineShoot.transform.localScale.x, m_thicknessLineShoot, 1f);
		l_lineShoot.gameObject.SetActive(value: true);
		l_lineShoot.enabled = true;
		l_lineShoot.color = new Color(m_colorLineShoot.r, m_colorLineShoot.g, m_colorLineShoot.b, 1f);
		Object.Destroy(l_lineShoot.gameObject, 0.5f);
		for (int l_index = 0; l_index < m_framesLineShoot; l_index++)
		{
			yield return new WaitForEndOfFrame();
			l_lineShoot.color = new Color(l_lineShoot.color.r, l_lineShoot.color.g, l_lineShoot.color.b, 1f - (float)(l_index / m_framesLineShoot));
		}
		Object.Destroy(l_lineShoot.gameObject);
	}

	private IEnumerator CoroutineMuzzleFlash()
	{
		StartCoroutine(CoroutineMuzzleFlashLight());
		int l_rndRoll = Random.Range(0, m_objMuzzleFlashes.Count);
		m_objMuzzleFlashes[l_rndRoll].SetActive(value: true);
		yield return new WaitForEndOfFrame();
		m_objMuzzleFlashes[l_rndRoll].SetActive(value: false);
	}

	private IEnumerator CoroutineMuzzleFlashLight()
	{
		UnityEngine.Rendering.Universal.Light2D l_lightFlash = Object.Instantiate(ResourceContainer.Resources.m_lightMuzzleFlash, base.transform);
		Vector3 position = l_lightFlash.transform.position;
		position.x = m_bulletSpawnPointDefault.transform.position.x;
		position.y = m_bulletSpawnPointDefault.transform.position.y;
		l_lightFlash.transform.position = position;
		l_lightFlash.gameObject.SetActive(value: true);
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(l_lightFlash.gameObject, 0.1f);
		float l_tranparencyFrom = l_lightFlash.intensity;
		if (ManagerDB.IsReduceGunFlash())
		{
			l_tranparencyFrom *= 0.25f;
		}
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			l_lightFlash.intensity = intensity;
			yield return new WaitForFixedUpdate();
		}
	}

	public void AnimateReload()
	{
		if (m_gunReloadType != GunReloadType.PumpAction)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFXRandom(m_audiosReload, 100f);
		}
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().Play("Reload");
		}
	}

	private void DropObjectsReload()
	{
		if (!m_isPickedUp || m_objectsToDropOnReload.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in m_objectsToDropOnReload)
		{
			GameObject obj = Object.Instantiate(item, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
			obj.transform.position = item.transform.position;
			obj.transform.rotation = item.transform.rotation;
			obj.transform.localScale = item.transform.lossyScale;
			Vector2 forceObjectsDropOnReload = m_forceObjectsDropOnReload;
			if (m_owner.GetIsFacingLeft())
			{
				forceObjectsDropOnReload.x *= -1f;
			}
			obj.SetActive(value: true);
			obj.GetComponent<Rigidbody2D>().AddForce(forceObjectsDropOnReload, ForceMode2D.Impulse);
			Object.Destroy(obj, 10f);
		}
	}

	private void DropObjectsShoot()
	{
		if (!m_isPickedUp || m_objectsToDropOnShoot.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in m_objectsToDropOnShoot)
		{
			GameObject obj = Object.Instantiate(item, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
			obj.transform.position = item.transform.position;
			obj.transform.rotation = item.transform.rotation;
			obj.transform.localScale = item.transform.lossyScale;
			Vector2 forceObjectsDropOnShoot = m_forceObjectsDropOnShoot;
			if (m_owner.GetIsFacingLeft())
			{
				forceObjectsDropOnShoot.x *= -1f;
			}
			obj.SetActive(value: true);
			obj.GetComponent<Rigidbody2D>().AddForce(forceObjectsDropOnShoot, ForceMode2D.Impulse);
			Object.Destroy(obj, 3f);
		}
	}

	public void Reload()
	{
		if (!m_isOwnerPlayer)
		{
			return;
		}
		_ = (Player)m_owner;
		if (m_isAmmoInfinite && m_ammoLeftInMagazine < m_ammoMagazineMax)
		{
			m_ammoLeftInMagazine = m_ammoMagazineMax;
			m_ammoLeft = m_ammoMax;
		}
		else if (m_ammoLeft > 0 && m_ammoLeftInMagazine < m_ammoMagazineMax)
		{
			int num = m_ammoMagazineMax - m_ammoLeftInMagazine;
			if (m_ammoLeft <= num)
			{
				num = m_ammoLeft;
			}
			DepleteAmmo(num);
			m_ammoLeftInMagazine += num;
		}
	}

	private IEnumerator CoroutineReload(int i_amountToReload)
	{
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFXRandom(m_audiosReload, 100f);
		yield return new WaitForSeconds(m_durationReload);
		if (!m_isAmmoInfinite)
		{
			DepleteAmmo(i_amountToReload);
		}
		m_ammoLeftInMagazine += i_amountToReload;
	}

	public void ReloadInterrupt()
	{
		StopCoroutine("CoroutineReload");
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().Play("Idle");
		}
	}

	private void DepleteAmmo(int i_amount)
	{
		if (m_owner is Player)
		{
			m_ammoLeft -= i_amount;
			if (m_ammoLeft < 0)
			{
				m_ammoLeft = 0;
			}
		}
	}

	public void AddAmmo(int i_amount)
	{
		m_ammoLeft += i_amount;
		if (m_ammoLeft > GetAmmoMaxTotal())
		{
			m_ammoLeft = GetAmmoMaxTotal();
		}
	}

	public void AddAmmoIncludingMagazine(int i_amount)
	{
		int num = 0;
		if (m_ammoLeftInMagazine < m_ammoMagazineMax)
		{
			m_ammoLeftInMagazine += i_amount;
			num = m_ammoLeftInMagazine - m_ammoMagazineMax;
		}
		else
		{
			num = i_amount;
		}
		if (num > 0)
		{
			m_ammoLeftInMagazine = m_ammoMagazineMax;
			AddAmmo(num);
		}
	}

	public void FillEntireGun()
	{
		m_ammoLeft = m_ammoMax;
		m_ammoLeftInMagazine = m_ammoMagazineMax;
		m_ammoLeft -= m_ammoMagazineMax;
	}

	public void ReloadOneBullet()
	{
		if (m_isOwnerPlayer)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFXRandom(m_audiosReload, 100f);
			_ = (Player)m_owner;
			if (m_isAmmoInfinite && m_ammoLeftInMagazine < m_ammoMagazineMax)
			{
				m_ammoLeftInMagazine++;
				m_ammoLeft = m_ammoMax;
			}
			else if (m_ammoLeft >= 1)
			{
				m_ammoLeftInMagazine++;
				m_ammoLeft--;
			}
		}
	}

	public int GetAmmoMax()
	{
		return m_ammoMax;
	}

	public void ConfigureModWeaponStats(CaptivityReloaded.Modding.WeaponStatsDefinition i_stats)
	{
		if (i_stats == null) return;
		if (i_stats.Damage.HasValue) m_damage = i_stats.Damage.Value;
		if (i_stats.BulletsPerShot.HasValue) m_bulletsExtraPerShot = i_stats.BulletsPerShot.Value - 1;
		if (i_stats.Penetration.HasValue) m_penetration = i_stats.Penetration.Value;
		if (i_stats.RangeMultiplier.HasValue) m_shootDistance01 = i_stats.RangeMultiplier.Value;
		if (i_stats.FireIntervalSeconds.HasValue) m_delayBetweenShots = i_stats.FireIntervalSeconds.Value;
		if (i_stats.Recoil.HasValue) m_recoilBase01 = i_stats.Recoil.Value;
		if (i_stats.MovementRecoil.HasValue) m_recoilFactorMovement01 = i_stats.MovementRecoil.Value;
		if (i_stats.KnockbackX.HasValue) m_knockbackX = i_stats.KnockbackX.Value;
		if (i_stats.KnockbackY.HasValue) m_knockbackY = i_stats.KnockbackY.Value;
		if (i_stats.AmmoMax.HasValue && i_stats.MagazineSize.HasValue)
		{
			m_ammoMax = i_stats.AmmoMax.Value;
			m_ammoMagazineMax = i_stats.MagazineSize.Value;
			FillEntireGun();
		}
	}

	public int GetAmmoMaxTotal()
	{
		return m_ammoMax - GetAmmoMagazineMax();
	}

	public int GetAmmoLeft()
	{
		return m_ammoLeft;
	}

	public int GetAmmoLeftTotal()
	{
		return GetAmmoLeft() + GetAmmoMagazineLeft();
	}

	public int GetAmmoMagazineMax()
	{
		return m_ammoMagazineMax;
	}

	public int GetAmmoMagazineLeft()
	{
		return m_ammoLeftInMagazine;
	}

	public float GetDurationReload()
	{
		return m_durationReload;
	}

	public float GetDelayBetweenShots()
	{
		return m_delayBetweenShots;
	}

	public int GetDamage()
	{
		return m_damage;
	}

	public float GetKockbackX()
	{
		return m_knockbackX;
	}

	public float GetKockbackY()
	{
		return m_knockbackY;
	}

	public float GetRecoilBase()
	{
		return m_recoilBase01;
	}

	public float GetRecoilFactorMovement01()
	{
		return m_recoilFactorMovement01;
	}

	public bool GetIsPenetratingAmmo()
	{
		if (m_penetration > 0)
		{
			return true;
		}
		return false;
	}

	public int GetPenetration()
	{
		return m_penetration;
	}

	public bool GetIsKnocksbackShooterFire()
	{
		return m_isKnockbacksShooterFire;
	}

	public bool GetIsKnocksbackShooterAltFire()
	{
		return m_isKnockbacksShooterAltFire;
	}

	public Transform GetTransformPointSpawnBullet()
	{
		return m_bulletSpawnPointDefault.transform;
	}

	public int GetBulletsPerShot()
	{
		return m_bulletsExtraPerShot + 1;
	}

	public bool GetIsAmmoInfinite()
	{
		return m_isAmmoInfinite;
	}

	public bool GetIsSemiFire()
	{
		return m_isSemiFire;
	}

	public GunHoldType GetHoldTypeGun()
	{
		return m_gunHoldType;
	}

	public GunReloadType GetReloadTypeGun()
	{
		return m_gunReloadType;
	}

	public bool IsReloadCancelable()
	{
		if (m_gunReloadType == GunReloadType.PumpAction)
		{
			return true;
		}
		return false;
	}

	public void Destroy()
	{
		Object.Destroy(base.gameObject);
	}

	private void AnimEventPlayAudioUnique(int i_numAudioUnique)
	{
		m_audioSourceSFX.PlayOneShot(m_audiosUnique[i_numAudioUnique - 1]);
	}
}
