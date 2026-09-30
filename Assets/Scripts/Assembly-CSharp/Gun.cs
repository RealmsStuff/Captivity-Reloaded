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

	[SerializeField]
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

	[SerializeField] private int m_modBurstCount = 1;
	[SerializeField] private float m_modBurstInterval = 0.1f;
	[SerializeField] private float m_modSoundVolume = 1f;
	[SerializeField] private float m_modShootCasingLifetime = 3f;
	[SerializeField] private float m_modReloadCasingLifetime = 10f;
	private bool m_modReloadAudioOverride;
	private bool m_modPlayShootSound = true;
	private bool m_modShowMuzzleFlash = true;
	private CaptivityReloaded.Modding.WeaponProjectileDefinition m_modProjectile;
	private CaptivityReloaded.Modding.WeaponMeleeDefinition m_modMelee;
	private CaptivityReloaded.Modding.WeaponChargeDefinition m_modCharge;
	private CaptivityReloaded.Modding.WeaponBeamDefinition m_modBeam;
	private CaptivityReloaded.Modding.WeaponThrowableDefinition m_modThrowable;
	private CaptivityReloaded.Modding.WeaponAlternateFireDefinition m_modAlternateFire;
	private CaptivityReloaded.Modding.WeaponAmmunitionDefinition m_modAmmunition;
	private string m_modIdleAnimation = "Idle";
	private string m_modFireAnimation = "Shoot";
	private string m_modReloadAnimation = "Reload";
	private float m_modFireAnimationSpeed = 1f;
	private float m_modReloadAnimationSpeed = 1f;
	private bool m_modIsCharging;
	private float m_modChargeStarted;
	private bool m_modIsAimingThrowable;
	private float m_modThrowableAimStarted;
	[SerializeField] private List<AudioClip> m_modThrowableImpactSounds = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_modThrowableTickSounds = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_modThrowableExplosionSounds = new List<AudioClip>();
	private float m_modNextAlternateTime;
	private Notification m_modChargeNotification;
	private int m_modChargePercentShown = -1;
	private readonly Dictionary<SpriteRenderer, Color> m_modChargeRendererColors = new Dictionary<SpriteRenderer, Color>();
	private float m_modMuzzleFlashDuration = 0.04f;
	private float m_modMuzzleLightIntensityMultiplier = 1f;
	private float m_modCasingAngularVelocity;
	[SerializeField] private Sprite m_modProjectileSprite;
	[SerializeField] private List<Sprite> m_modImpactSprites = new List<Sprite>();
	[SerializeField] private string m_modStatsJson;
	[SerializeField] private string m_modBehaviorJson;
	[SerializeField] private bool m_isOriginalRuntimeWeapon;
	private bool m_modStatsRestored;

	private float GetRuleProfileRange()
	{
		return ExternalRuleProfileFactory.ApplyWeaponRange(m_distanceShootMax);
	}

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
		ModWeaponSpriteAnimator spriteAnimator = GetComponent<ModWeaponSpriteAnimator>();
		if (spriteAnimator != null) spriteAnimator.Play("equip");
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().Play(m_modIdleAnimation);
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
			SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
			renderer.sprite = m_spritesMuzzleFlash[num];
			// Runtime-created renderers otherwise use the Default sorting layer, which is
			// behind the stage and makes every muzzle flash effectively invisible.
			renderer.sortingLayerName = "Projectile";
			renderer.sortingOrder = 49;
			gameObject.transform.SetParent(m_bulletSpawnPointDefault.transform, false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: false);
			m_objMuzzleFlashes.Add(gameObject);
			num++;
		}
	}

	protected virtual void OnEnable()
	{
		RestoreModStats();
		RestoreModBehavior();
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
		CancelCharge();
		CancelThrowableAim();
		base.OnDrop -= HandleDrop;
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().speed = 1f;
			GetComponent<Animator>().Play(m_modIdleAnimation);
		}
	}

	public List<Bullet> Shoot()
	{
		List<Bullet> result = ShootOnce();
		// A lethal first shot can synchronously advance Gun Game and deactivate this
		// weapon before ShootOnce returns. Do not host the remaining burst on an
		// inactive clone.
		if (m_modBurstCount > 1 && isActiveAndEnabled && gameObject.activeInHierarchy)
			StartCoroutine(CoroutineModBurst());
		return result;
	}

	public override void Show()
	{
		base.Show();
		ModHiddenWeaponSlots hidden = GetComponent<ModHiddenWeaponSlots>();
		if (hidden != null) hidden.Apply();
	}

	public override void Hide()
	{
		CancelCharge();
		CancelThrowableAim();
		base.Hide();
	}

	private List<Bullet> ShootOnce(float i_damageMultiplier = 1f, int i_ammoCost = 1, bool i_alternate = false)
	{
		PlayAttackPresentation(i_alternate);
		if (m_isOriginalRuntimeWeapon) DropObjectsShoot();
		m_ammoLeftInMagazine = Mathf.Max(0, m_ammoLeftInMagazine - i_ammoCost);
		return ShootBullets(i_damageMultiplier);
	}

	private void PlayAttackPresentation(bool i_alternate = false)
	{
		ModWeaponSpriteAnimator spriteAnimator = GetComponent<ModWeaponSpriteAnimator>();
		if (spriteAnimator != null) spriteAnimator.Play(i_alternate ? "alternate" : "fire", "fire");
		if (m_modPlayShootSound && m_audiosShot.Count > 0)
		{
			m_audioSourceSFX.PlayOneShot(m_audiosShot[Random.Range(0, m_audiosShot.Count)], m_modSoundVolume);
		}
		if (m_modShowMuzzleFlash && m_isHasMuzzleFlash)
		{
			StartCoroutine(CoroutineMuzzleFlash());
		}
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().speed = m_modFireAnimationSpeed;
			GetComponent<Animator>().Play(m_modFireAnimation, 0, 0f);
			StartCoroutine(CoroutineResetAnimatorSpeed(Mathf.Max(0.02f, m_delayBetweenShots)));
		}
		if (m_isShakesCamera)
		{
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().Shake(m_knockbackX / 5f + m_knockbackY / 5f, 0.05f);
		}
	}

	private IEnumerator CoroutineModBurst()
	{
		for (int shot = 1; shot < m_modBurstCount; shot++)
		{
			yield return new WaitForSeconds(m_modBurstInterval);
			if (!m_isPickedUp || m_ammoLeftInMagazine < 1) yield break;
			if (m_isOwnerPlayer) CommonReferences.Instance.GetPlayer().PresentModWeaponRecoil(this);
			ShootOnce();
		}
	}

	internal IReadOnlyList<Sprite> GetCoreMuzzleFlashSprites()
	{
		return m_spritesMuzzleFlash;
	}

	internal void SetCoreMuzzleFlashSprite(int i_index, Sprite i_replacement)
	{
		if (i_replacement == null || m_spritesMuzzleFlash == null || i_index < 0 || i_index >= m_spritesMuzzleFlash.Count) return;
		m_spritesMuzzleFlash[i_index] = i_replacement;
		if (i_index < m_objMuzzleFlashes.Count && m_objMuzzleFlashes[i_index] != null)
		{
			SpriteRenderer renderer = m_objMuzzleFlashes[i_index].GetComponent<SpriteRenderer>();
			if (renderer != null) renderer.sprite = i_replacement;
		}
	}

	private List<Bullet> ShootBullets(float i_damageMultiplier = 1f)
	{
		List<Bullet> list = new List<Bullet>();
		int mask = LayerMask.GetMask("Actor", "BodyPart", "Platform", "InvisibleWall", "Interactable", "Trap");
		Vector2 vector = GetShotOrigin();
		Vector2 vector2 = CommonReferences.Instance.GetUtilityTools().GetPosMousePerspectiveCamera();
		float valueTotal = ExternalRuleProfileFactory.ApplyGunDamageMultiplier(CommonReferences.Instance.GetPlayer().GetStat("DamageMultiplierGun").GetValueTotal());
		for (int i = 0; i < m_bulletsExtraPerShot + 1; i++)
		{
			Vector2 vector3 = vector2 - vector;
			vector3.Normalize();
			vector3 *= GetRuleProfileRange() * m_shootDistance01;
			float num = m_recoilBase01;
			if (CommonReferences.Instance.GetPlayer().IsStatusEffectAppliedAlready("BPrec"))
			{
				num *= 0.5f;
			}
			vector3 = Quaternion.AngleAxis(Random.Range((0f - num) * 20f, num * 20f), m_bulletSpawnPointDefault.transform.forward) * vector3;
			if (m_modProjectile != null && m_modProjectile.Mode == "physical")
			{
				SpawnModProjectile(vector3.normalized, (float)m_damage * valueTotal * i_damageMultiplier * GetAmmunitionDamageMultiplier());
				continue;
			}
			RaycastHit2D[] i_hits = Physics2D.RaycastAll(vector, vector3, GetRuleProfileRange() * m_shootDistance01, mask);
			Bullet bullet = new Bullet(this, m_owner, vector3, i_hits,
				(float)m_damage * valueTotal * i_damageMultiplier * GetAmmunitionDamageMultiplier(),
				m_knockbackX, m_knockbackY, m_penetration + GetAmmunitionPenetrationBonus());
			bullet.HandleHits();
			list.Add(bullet);
		}
		return list;
	}

	public virtual void HandleActorHit(BodyPartActor i_bodyPart, Bullet i_bullet)
	{
		if (m_modAmmunition == null || i_bodyPart == null || i_bodyPart.GetOwner() == null
			|| i_bullet == null || !i_bullet.IsActorHit(i_bodyPart.GetOwner())) return;
		if (m_modAmmunition.DamageOverTime.GetValueOrDefault() <= 0f
			&& m_modAmmunition.SlowMultiplier.GetValueOrDefault(1f) >= 1f) return;
		i_bodyPart.GetOwner().gameObject.AddComponent<ModAmmunitionEffect>().Configure(i_bodyPart.GetOwner(), m_modAmmunition);
	}

	public float GetAmmunitionWeakpointMultiplier(BodyPartActor i_bodyPart)
	{
		return m_modAmmunition != null && i_bodyPart != null && i_bodyPart.GetDamageMultiplierAmountBodyPart() > 1f
			? m_modAmmunition.WeakpointMultiplier ?? 1f : 1f;
	}

	private float GetAmmunitionDamageMultiplier() { return m_modAmmunition?.DamageMultiplier ?? 1f; }
	private int GetAmmunitionPenetrationBonus() { return m_modAmmunition?.PenetrationBonus ?? 0; }

	public void LineShoot(Vector2 i_direction, Vector2 i_posEnd)
	{
		// A hit can synchronously advance a weapon-progression rule. That removes and
		// disables this gun before Bullet.HandleHits returns, so it is no longer a
		// valid coroutine host for the tracer belonging to the final shot.
		if (!m_isHideShootLine && isActiveAndEnabled && gameObject.activeInHierarchy)
		{
			Vector2 i_posOrigin = GetShotOrigin();
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
			l_lineShoot.transform.localScale = new Vector3(GetRuleProfileRange() * m_shootDistance01 * 32f, 1f, 1f);
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
		// A single rendered frame was easy to miss at high refresh rates.
		yield return new WaitForSeconds(m_modMuzzleFlashDuration);
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
		l_tranparencyFrom *= m_modMuzzleLightIntensityMultiplier;
		l_lightFlash.intensity = l_tranparencyFrom;
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
		ModWeaponSpriteAnimator spriteAnimator = GetComponent<ModWeaponSpriteAnimator>();
		if (spriteAnimator != null) spriteAnimator.Play("reload");
		if (m_gunReloadType != GunReloadType.PumpAction)
		{
			PlayReloadSound();
		}
		if (m_isOriginalRuntimeWeapon) DropObjectsReload();
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().speed = m_modReloadAnimationSpeed;
			GetComponent<Animator>().Play(m_modReloadAnimation);
			StartCoroutine(CoroutineResetAnimatorSpeed(Mathf.Max(0.02f, m_durationReload)));
		}
	}

	private IEnumerator CoroutineResetAnimatorSpeed(float i_delay)
	{
		yield return new WaitForSeconds(i_delay);
		Animator animator = GetComponent<Animator>();
		if (animator != null) animator.speed = 1f;
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
			Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
			body.AddForce(forceObjectsDropOnReload, ForceMode2D.Impulse);
			body.angularVelocity = m_owner.GetIsFacingLeft() ? -m_modCasingAngularVelocity : m_modCasingAngularVelocity;
			Object.Destroy(obj, m_modReloadCasingLifetime);
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
			Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
			body.AddForce(forceObjectsDropOnShoot, ForceMode2D.Impulse);
			body.angularVelocity = m_owner.GetIsFacingLeft() ? -m_modCasingAngularVelocity : m_modCasingAngularVelocity;
			Object.Destroy(obj, m_modShootCasingLifetime);
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
		PlayReloadSound();
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
			PlayReloadSound();
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
		m_modStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(i_stats);
		if (i_stats.Damage.HasValue) m_damage = i_stats.Damage.Value;
		if (i_stats.BulletsPerShot.HasValue) m_bulletsExtraPerShot = i_stats.BulletsPerShot.Value - 1;
		if (i_stats.Penetration.HasValue) m_penetration = i_stats.Penetration.Value;
		if (i_stats.RangeMultiplier.HasValue) m_shootDistance01 = i_stats.RangeMultiplier.Value;
		if (i_stats.FireIntervalSeconds.HasValue) m_delayBetweenShots = i_stats.FireIntervalSeconds.Value;
		if (i_stats.Recoil.HasValue) m_recoilBase01 = i_stats.Recoil.Value;
		if (i_stats.MovementRecoil.HasValue) m_recoilFactorMovement01 = i_stats.MovementRecoil.Value;
		if (i_stats.KnockbackX.HasValue) m_knockbackX = i_stats.KnockbackX.Value;
		if (i_stats.KnockbackY.HasValue) m_knockbackY = i_stats.KnockbackY.Value;
		if (i_stats.ReloadSeconds.HasValue) m_durationReload = i_stats.ReloadSeconds.Value;
		if (i_stats.InfiniteAmmo.HasValue) m_isAmmoInfinite = i_stats.InfiniteAmmo.Value;
		if (i_stats.SemiAutomatic.HasValue) m_isSemiFire = i_stats.SemiAutomatic.Value;
		if (!string.IsNullOrWhiteSpace(i_stats.HoldType) && System.Enum.TryParse(i_stats.HoldType, true, out GunHoldType holdType)) m_gunHoldType = holdType;
		if (!string.IsNullOrWhiteSpace(i_stats.ReloadType) && System.Enum.TryParse(i_stats.ReloadType, true, out GunReloadType reloadType)) m_gunReloadType = reloadType;
		if (!string.IsNullOrWhiteSpace(i_stats.WeaponType) && System.Enum.TryParse(i_stats.WeaponType, true, out WeaponType weaponType)) m_weaponType = weaponType;
		ConfigureModWeaponBase(i_stats.EquipSeconds, i_stats.Marketable);
		ConfigureModEconomy(i_stats.Weight, i_stats.Value);
		if (i_stats.AmmoMax.HasValue && i_stats.MagazineSize.HasValue)
		{
			m_ammoMax = i_stats.AmmoMax.Value;
			m_ammoMagazineMax = i_stats.MagazineSize.Value;
			FillEntireGun();
		}
	}

	public void ConfigureOriginalWeaponRuntime(GameObject i_muzzlePoint, Sprite i_icon,
		CaptivityReloaded.Modding.WeaponStatsDefinition i_stats)
	{
		m_isOriginalRuntimeWeapon = true;
		m_bulletSpawnPointDefault = i_muzzlePoint;
		m_isHideShootLine = false;
		m_shootDistance01 = 1f;
		m_damage = 1;
		m_bulletsExtraPerShot = 0;
		m_penetration = 0;
		m_delayBetweenShots = 0.25f;
		m_recoilBase01 = 0.05f;
		m_recoilFactorMovement01 = 0.05f;
		m_ammoMax = 12;
		m_ammoMagazineMax = 12;
		m_isSemiFire = true;
		m_gunHoldType = GunHoldType.OneHanded;
		m_gunReloadType = GunReloadType.Magazine;
		m_weaponType = WeaponType.Pistol;
		ConfigureModPickup(i_canDrop: true);
		SetModItemIcon(i_icon);
		ConfigureModWeaponStats(i_stats);
	}

	private Vector2 GetShotOrigin()
	{
		if (m_isOriginalRuntimeWeapon && m_bulletSpawnPointDefault != null) return m_bulletSpawnPointDefault.transform.position;
		return m_owner.GetSkeleton().GetBone("rArmUpper").transform.position;
	}

	public void ConfigureModWeaponBehavior(CaptivityReloaded.Modding.WeaponBehaviorDefinition i_behavior,
		IEnumerable<Sprite> i_muzzleFlashes, Sprite i_casingSprite, Sprite i_projectileSprite,
		IEnumerable<Sprite> i_impactSprites)
	{
		if (i_behavior == null) return;
		m_modBehaviorJson = Newtonsoft.Json.JsonConvert.SerializeObject(i_behavior);
		m_modBurstCount = i_behavior.BurstCount ?? 1;
		m_modBurstInterval = i_behavior.BurstIntervalSeconds ?? 0.1f;
		if (m_modBurstCount > 1)
			m_delayBetweenShots = Mathf.Max(m_delayBetweenShots, m_modBurstInterval * (m_modBurstCount - 1) + 0.02f);
		m_modSoundVolume = i_behavior.SoundVolume ?? 1f;
		m_modPlayShootSound = i_behavior.PlayShootSound ?? true;
		m_modShowMuzzleFlash = i_behavior.ShowMuzzleFlash ?? true;
		m_modMuzzleFlashDuration = i_behavior.MuzzleFlashDurationSeconds ?? 0.04f;
		m_modMuzzleLightIntensityMultiplier = i_behavior.MuzzleLightIntensityMultiplier ?? 1f;
		if (i_behavior.CameraShake.HasValue) m_isShakesCamera = i_behavior.CameraShake.Value;
		if (i_behavior.ShooterKnockback.HasValue) m_isKnockbacksShooterFire = i_behavior.ShooterKnockback.Value;
		if (!string.IsNullOrWhiteSpace(i_behavior.TracerColor)
			&& ColorUtility.TryParseHtmlString(i_behavior.TracerColor, out Color tracerColor)) m_colorLineShoot = tracerColor;
		if (i_behavior.TracerThickness.HasValue) m_thicknessLineShoot = i_behavior.TracerThickness.Value;
		if (i_behavior.TracerFrames.HasValue) m_framesLineShoot = i_behavior.TracerFrames.Value;
		if (i_muzzleFlashes != null)
		{
			foreach (GameObject flash in m_objMuzzleFlashes) if (flash != null) Object.Destroy(flash);
			m_objMuzzleFlashes.Clear();
			m_spritesMuzzleFlash = new List<Sprite>(i_muzzleFlashes);
			m_isHasMuzzleFlash = m_spritesMuzzleFlash.Count > 0;
			if (m_isHasMuzzleFlash) CreateMuzzleFlashObject();
		}
		if (i_casingSprite != null)
		{
			GameObject casing = new GameObject("ModCasingTemplate");
			casing.layer = LayerMask.NameToLayer("Item");
			casing.transform.SetParent(m_bulletSpawnPointDefault.transform, false);
			casing.transform.localPosition = new Vector3(i_behavior.CasingOffsetX ?? 0f, i_behavior.CasingOffsetY ?? 0f, 0f);
			SpriteRenderer casingRenderer = casing.AddComponent<SpriteRenderer>();
			casingRenderer.sprite = i_casingSprite;
			casingRenderer.sortingLayerName = "Item";
			BoxCollider2D casingCollider = casing.AddComponent<BoxCollider2D>();
			ConfigureOpaqueSpriteCollider(casingCollider, i_casingSprite);
			Rigidbody2D casingBody = casing.AddComponent<Rigidbody2D>();
			casingBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			casingBody.gravityScale = i_behavior.CasingGravityScale ?? 1f;
			casing.SetActive(false);
			string casingOn = string.IsNullOrWhiteSpace(i_behavior.CasingOn) ? "shoot" : i_behavior.CasingOn;
			if (casingOn == "shoot" || casingOn == "both") { m_objectsToDropOnShoot.Clear(); m_objectsToDropOnShoot.Add(casing); }
			if (casingOn == "reload" || casingOn == "both") { m_objectsToDropOnReload.Clear(); m_objectsToDropOnReload.Add(casing); }
			m_forceObjectsDropOnShoot = new Vector2(i_behavior.CasingForceX ?? 1f, i_behavior.CasingForceY ?? 2f);
			m_forceObjectsDropOnReload = m_forceObjectsDropOnShoot;
			m_modShootCasingLifetime = i_behavior.CasingLifetimeSeconds ?? 3f;
			m_modReloadCasingLifetime = i_behavior.CasingLifetimeSeconds ?? 10f;
			m_modCasingAngularVelocity = i_behavior.CasingAngularVelocity ?? 0f;
		}
		foreach (GameObject flash in m_objMuzzleFlashes)
			if (flash != null) flash.transform.localPosition = new Vector3(i_behavior.MuzzleFlashOffsetX ?? 0f, i_behavior.MuzzleFlashOffsetY ?? 0f, 0f);
		m_modProjectile = i_behavior.Projectile;
		m_modMelee = i_behavior.Melee;
		m_modCharge = i_behavior.Charge;
		m_modBeam = i_behavior.Beam;
		m_modThrowable = i_behavior.Throwable;
		m_modAlternateFire = i_behavior.AlternateFire;
		m_modAmmunition = i_behavior.Ammunition;
		if (m_modBeam != null)
		{
			if (!string.IsNullOrWhiteSpace(m_modBeam.Color) && ColorUtility.TryParseHtmlString(m_modBeam.Color, out Color beamColor)) m_colorLineShoot = beamColor;
			if (m_modBeam.Thickness.HasValue) m_thicknessLineShoot = m_modBeam.Thickness.Value;
			m_framesLineShoot = Mathf.Max(m_framesLineShoot, 2);
		}
		if (i_behavior.Animations != null)
		{
			m_modIdleAnimation = ResolveAnimationState(i_behavior.Animations.IdleState, "Idle");
			m_modFireAnimation = ResolveAnimationState(i_behavior.Animations.FireState, "Shoot");
			m_modReloadAnimation = ResolveAnimationState(i_behavior.Animations.ReloadState, "Reload");
			m_modFireAnimationSpeed = i_behavior.Animations.FireSpeedMultiplier ?? 1f;
			m_modReloadAnimationSpeed = i_behavior.Animations.ReloadSpeedMultiplier ?? 1f;
		}
		m_modProjectileSprite = i_projectileSprite;
		m_modImpactSprites = i_impactSprites == null ? new List<Sprite>() : new List<Sprite>(i_impactSprites);
	}

	private static void ConfigureOpaqueSpriteCollider(BoxCollider2D i_collider, Sprite i_sprite)
	{
		Rect rect = i_sprite.rect;
		int minX = Mathf.CeilToInt(rect.width);
		int minY = Mathf.CeilToInt(rect.height);
		int maxX = -1;
		int maxY = -1;
		try
		{
			Color32[] pixels = i_sprite.texture.GetPixels32();
			int textureWidth = i_sprite.texture.width;
			int startX = Mathf.RoundToInt(rect.x);
			int startY = Mathf.RoundToInt(rect.y);
			int width = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					if (pixels[(startY + y) * textureWidth + startX + x].a > 8)
					{
						minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
						minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
					}
		}
		catch (UnityException)
		{
			// Runtime-loaded mod PNGs are readable. Retain a conservative fallback for other sprites.
		}
		if (maxX < minX || maxY < minY)
		{
			Vector2 fallback = i_sprite.bounds.size;
			i_collider.size = new Vector2(Mathf.Max(0.02f, fallback.x * 0.8f), Mathf.Max(0.02f, fallback.y * 0.8f));
			return;
		}
		float pixelsPerUnit = i_sprite.pixelsPerUnit;
		i_collider.size = new Vector2(Mathf.Max(0.02f, (maxX - minX + 1) / pixelsPerUnit * 0.9f),
			Mathf.Max(0.02f, (maxY - minY + 1) / pixelsPerUnit * 0.9f));
		Vector2 opaqueCenter = new Vector2((minX + maxX + 1) * 0.5f, (minY + maxY + 1) * 0.5f);
		i_collider.offset = (opaqueCenter - i_sprite.pivot) / pixelsPerUnit;
	}

	private void RestoreModBehavior()
	{
		if (string.IsNullOrWhiteSpace(m_modBehaviorJson)) return;
		CaptivityReloaded.Modding.WeaponBehaviorDefinition behavior;
		try { behavior = Newtonsoft.Json.JsonConvert.DeserializeObject<CaptivityReloaded.Modding.WeaponBehaviorDefinition>(m_modBehaviorJson); }
		catch (Newtonsoft.Json.JsonException exception)
		{
			CaptivityReloaded.Modding.ModLoaderRuntime.LastReport.Add(CaptivityReloaded.Modding.ValidationSeverity.Error,
				"weapon.behavior-restore", "Could not restore weapon behavior: " + exception.Message, gameObject.name);
			return;
		}
		if (behavior == null) return;
		m_modProjectile = behavior.Projectile;
		m_modMelee = behavior.Melee;
		m_modCharge = behavior.Charge;
		m_modBeam = behavior.Beam;
		m_modThrowable = behavior.Throwable;
		m_modAlternateFire = behavior.AlternateFire;
		m_modAmmunition = behavior.Ammunition;
		m_modPlayShootSound = behavior.PlayShootSound ?? true;
		m_modShowMuzzleFlash = behavior.ShowMuzzleFlash ?? true;
		if (behavior.Animations != null)
		{
			m_modIdleAnimation = ResolveAnimationState(behavior.Animations.IdleState, "Idle");
			m_modFireAnimation = ResolveAnimationState(behavior.Animations.FireState, "Shoot");
			m_modReloadAnimation = ResolveAnimationState(behavior.Animations.ReloadState, "Reload");
			m_modFireAnimationSpeed = behavior.Animations.FireSpeedMultiplier ?? 1f;
			m_modReloadAnimationSpeed = behavior.Animations.ReloadSpeedMultiplier ?? 1f;
		}
		if (m_modBeam != null)
		{
			if (!string.IsNullOrWhiteSpace(m_modBeam.Color) && ColorUtility.TryParseHtmlString(m_modBeam.Color, out Color color)) m_colorLineShoot = color;
			if (m_modBeam.Thickness.HasValue) m_thicknessLineShoot = m_modBeam.Thickness.Value;
		}
	}

	private void RestoreModStats()
	{
		if (m_modStatsRestored || string.IsNullOrWhiteSpace(m_modStatsJson)) return;
		m_modStatsRestored = true;
		try
		{
			CaptivityReloaded.Modding.WeaponStatsDefinition stats = Newtonsoft.Json.JsonConvert.DeserializeObject<CaptivityReloaded.Modding.WeaponStatsDefinition>(m_modStatsJson);
			if (stats != null) ConfigureModWeaponStats(stats);
		}
		catch (Newtonsoft.Json.JsonException exception)
		{
			CaptivityReloaded.Modding.ModLoaderRuntime.LastReport.Add(CaptivityReloaded.Modding.ValidationSeverity.Error,
				"weapon.stats-restore", "Could not restore weapon statistics: " + exception.Message, gameObject.name);
		}
	}

	private string ResolveAnimationState(string i_requested, string i_fallback)
	{
		if (string.IsNullOrWhiteSpace(i_requested)) return i_fallback;
		Animator animator = GetComponent<Animator>();
		if (animator == null || animator.HasState(0, Animator.StringToHash(i_requested))
			|| animator.HasState(0, Animator.StringToHash("Base Layer." + i_requested))) return i_requested;
		Debug.LogWarning("[ModLoader] Weapon " + gameObject.name + " has no animation state '" + i_requested + "'; using '" + i_fallback + "'.");
		return i_fallback;
	}

	public bool IsPrimaryMelee()
	{
		return m_modMelee != null && (string.IsNullOrWhiteSpace(m_modMelee.Input) || m_modMelee.Input == "primary");
	}

	public List<Bullet> MeleeAttack() { return MeleeAttack(false, 1f); }

	private List<Bullet> MeleeAttack(bool i_alternate, float i_damageMultiplier)
	{
		List<Bullet> attacks = new List<Bullet>();
		bool isAlternateMelee = m_modMelee != null && m_modMelee.Input == "alternate";
		bool forcedAlternate = i_alternate && m_modAlternateFire != null && m_modAlternateFire.Mode == "melee";
		if (m_modMelee == null || m_owner == null || (!forcedAlternate && i_alternate != isAlternateMelee)) return attacks;
		PlayAttackPresentation(i_alternate);
		Vector2 origin = GetShotOrigin();
		Vector2 aim = CommonReferences.Instance.GetUtilityTools().GetPosMousePerspectiveCamera();
		Vector2 direction = (aim - origin).normalized;
		float range = m_modMelee.Range ?? 1.5f;
		float radius = m_modMelee.Radius ?? 0.75f;
		Vector2 center = origin + direction * range;
		float damage = m_damage * (m_modMelee.DamageMultiplier ?? 1f) * i_damageMultiplier * GetAmmunitionDamageMultiplier()
			* ExternalRuleProfileFactory.ApplyGunDamageMultiplier(CommonReferences.Instance.GetPlayer().GetStat("DamageMultiplierGun").GetValueTotal());
		float knockbackX = m_modMelee.KnockbackX ?? m_knockbackX;
		float knockbackY = m_modMelee.KnockbackY ?? m_knockbackY;
		int maxTargets = m_modMelee.MaxTargets ?? 1;
		HashSet<Actor> actors = new HashSet<Actor>();
		HashSet<Interactable> interactables = new HashSet<Interactable>();
		foreach (Collider2D collider in Physics2D.OverlapCircleAll(center, radius,
			LayerMask.GetMask("Actor", "BodyPart", "Interactable", "Trap")))
		{
			BodyPartActor body = collider.GetComponent<BodyPartActor>();
			if (body != null && body.GetOwner() != null && body.GetOwner() != m_owner && !actors.Contains(body.GetOwner()) && actors.Count < maxTargets)
			{
				Bullet strike = new Bullet(this, m_owner, direction, new RaycastHit2D[0], damage, knockbackX, knockbackY, 0, false);
				strike.HitBodyPartDirect(body, body.transform.position);
				actors.Add(body.GetOwner());
				attacks.Add(strike);
			}
			Interactable interactable = collider.GetComponent<Interactable>() ?? collider.GetComponentInParent<Interactable>();
			if (interactable != null && interactables.Add(interactable))
			{
				Bullet strike = attacks.Count > 0 ? attacks[0] : new Bullet(this, m_owner, direction, new RaycastHit2D[0], damage, knockbackX, knockbackY, 0, false);
				strike.HitInteractable(interactable);
			}
		}
		return attacks;
	}

	public bool HasChargeAttack() { return m_modCharge != null && !IsPrimaryMelee() && !IsPrimaryBeam(); }
	public bool HasThrowableAttack() { return m_modThrowable != null; }
	public bool IsAimingThrowable() { return m_modIsAimingThrowable; }

	public void BeginThrowableAim()
	{
		if (!HasThrowableAttack() || m_modIsAimingThrowable || m_ammoLeftInMagazine < 1) return;
		m_modIsAimingThrowable = true;
		m_modThrowableAimStarted = Time.time;
		ModWeaponSpriteAnimator spriteAnimator = GetComponent<ModWeaponSpriteAnimator>();
		if (spriteAnimator != null) spriteAnimator.Play("charge");
	}

	public bool ReleaseThrowable()
	{
		if (!m_modIsAimingThrowable || m_modThrowable == null || m_ammoLeftInMagazine < 1 || m_owner == null) return false;
		float heldSeconds = Mathf.Max(0f, Time.time - m_modThrowableAimStarted);
		m_modIsAimingThrowable = false;
		PlayAttackPresentation();
		m_ammoLeftInMagazine = Mathf.Max(0, m_ammoLeftInMagazine - 1);
		SpawnModThrowable(heldSeconds);
		return true;
	}

	private void CancelThrowableAim()
	{
		m_modIsAimingThrowable = false;
		m_modThrowableAimStarted = 0f;
	}

	private void SpawnModThrowable(float i_heldSeconds)
	{
		Vector2 origin = GetShotOrigin();
		Vector2 aim = CommonReferences.Instance.GetUtilityTools().GetPosMousePerspectiveCamera();
		Vector2 direction = (aim - origin).normalized;
		if (direction.sqrMagnitude < 0.01f) direction = m_owner.GetIsFacingLeft() ? Vector2.left : Vector2.right;

		Sprite sprite = m_modProjectileSprite;
		if (sprite == null)
		{
			foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (renderer.sprite == null || renderer.gameObject.name == "MuzzleFlash" || renderer.gameObject.name == "ModCasingTemplate") continue;
				sprite = renderer.sprite;
				break;
			}
		}
		if (sprite == null) return;

		GameObject projectile = new GameObject(gameObject.name + " Throwable");
		Stage stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		if (stage != null) projectile.transform.SetParent(stage.transform, true);
		projectile.transform.position = origin;
		int itemLayer = LayerMask.NameToLayer("Item");
		if (itemLayer >= 0) projectile.layer = itemLayer;
		SpriteRenderer projectileRenderer = projectile.AddComponent<SpriteRenderer>();
		projectileRenderer.sprite = sprite;
		projectileRenderer.sortingLayerName = "Item";
		projectileRenderer.sortingOrder = 20;
		Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
		body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		body.gravityScale = m_modThrowable.Gravity ?? 1f;
		BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
		ConfigureOpaqueSpriteCollider(collider, sprite);
		AudioSource audio = projectile.AddComponent<AudioSource>();
		audio.outputAudioMixerGroup = m_audioSourceSFX == null ? null : m_audioSourceSFX.outputAudioMixerGroup;
		projectile.AddComponent<ModThrownWeaponProjectile>().Configure(this, m_owner, body, collider, projectileRenderer, audio,
			direction, i_heldSeconds, m_modThrowable, m_modThrowableImpactSounds, m_modThrowableTickSounds,
			m_modThrowableExplosionSounds);
	}

	public void AddModThrowableAudioClip(string i_role, AudioClip i_clip)
	{
		if (i_clip == null) return;
		if (i_role == "impact") m_modThrowableImpactSounds.Add(i_clip);
		else if (i_role == "tick") m_modThrowableTickSounds.Add(i_clip);
		else if (i_role == "explosion") m_modThrowableExplosionSounds.Add(i_clip);
	}

	public bool IsCharging() { return m_modIsCharging; }
	public float GetChargeProgress01()
	{
		if (!m_modIsCharging || m_modCharge == null) return 0f;
		return Mathf.Clamp01((Time.time - m_modChargeStarted) / (m_modCharge.Seconds ?? 1f));
	}
	public void BeginCharge()
	{
		if (!HasChargeAttack() || m_modIsCharging || m_ammoLeftInMagazine < 1) return;
		m_modIsCharging = true;
		m_modChargeStarted = Time.time;
		m_modChargePercentShown = -1;
		m_modChargeRendererColors.Clear();
		foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
			if (renderer != null && renderer.gameObject.name != "MuzzleFlash" && renderer.gameObject.name != "ModCasingTemplate")
				m_modChargeRendererColors[renderer] = renderer.color;
		ModWeaponSpriteAnimator spriteAnimator = GetComponent<ModWeaponSpriteAnimator>();
		if (spriteAnimator != null) spriteAnimator.Play("charge");
		ManagerHud hud = CommonReferences.Instance.GetManagerHud();
		if (m_isOwnerPlayer && hud != null && hud.GetManagerNotification() != null)
			m_modChargeNotification = hud.GetManagerNotification().CreateNotification(
				(string.IsNullOrWhiteSpace(m_modCharge.ChargingText) ? "CHARGING" : m_modCharge.ChargingText) + ": 0%",
				ColorTextNotification.Ammo, i_isContinues: true);
		UpdateChargeFeedback();
	}
	public List<Bullet> ReleaseCharge()
	{
		if (!m_modIsCharging) return new List<Bullet>();
		float charge01 = GetChargeProgress01();
		m_modIsCharging = false;
		DestroyChargeNotification();
		float multiplier = Mathf.Lerp(m_modCharge.MinimumDamageMultiplier ?? 0.25f,
			m_modCharge.MaximumDamageMultiplier ?? 2f, charge01);
		return ShootOnce(multiplier);
	}

	private void Update()
	{
		if (m_modIsCharging) UpdateChargeFeedback();
	}

	private void UpdateChargeFeedback()
	{
		int percent = Mathf.RoundToInt(GetChargeProgress01() * 100f);
		if (percent != m_modChargePercentShown)
		{
			m_modChargePercentShown = percent;
			if (m_modChargeNotification != null)
				m_modChargeNotification.SetText(percent >= 100
					? (string.IsNullOrWhiteSpace(m_modCharge.ReadyText) ? "CHARGE READY" : m_modCharge.ReadyText)
					: (string.IsNullOrWhiteSpace(m_modCharge.ChargingText) ? "CHARGING" : m_modCharge.ChargingText) + ": " + percent + "%");
		}
		Color charging = new Color(1f, 0.65f, 0.15f, 1f);
		Color ready = new Color(0.35f, 1f, 0.45f, 1f);
		if (!string.IsNullOrWhiteSpace(m_modCharge.ChargingColor)) ColorUtility.TryParseHtmlString(m_modCharge.ChargingColor, out charging);
		if (!string.IsNullOrWhiteSpace(m_modCharge.ReadyColor)) ColorUtility.TryParseHtmlString(m_modCharge.ReadyColor, out ready);
		float progress = GetChargeProgress01();
		float strength = (m_modCharge.TintStrength ?? 0.45f) * progress;
		if (progress >= 1f) strength *= 0.75f + Mathf.PingPong(Time.time * 4f, 0.25f);
		foreach (KeyValuePair<SpriteRenderer, Color> entry in m_modChargeRendererColors)
			if (entry.Key != null) entry.Key.color = Color.Lerp(entry.Value, progress >= 1f ? ready : charging, strength);
	}

	private void CancelCharge()
	{
		m_modIsCharging = false;
		DestroyChargeNotification();
	}

	private void DestroyChargeNotification()
	{
		foreach (KeyValuePair<SpriteRenderer, Color> entry in m_modChargeRendererColors)
			if (entry.Key != null) entry.Key.color = entry.Value;
		m_modChargeRendererColors.Clear();
		if (m_modChargeNotification == null) return;
		ManagerHud hud = CommonReferences.Instance.GetManagerHud();
		if (hud != null && hud.GetManagerNotification() != null)
			hud.GetManagerNotification().DestroyNotification(m_modChargeNotification);
		m_modChargeNotification = null;
		m_modChargePercentShown = -1;
	}

	public bool IsPrimaryBeam()
	{
		return m_modBeam != null && (string.IsNullOrWhiteSpace(m_modBeam.Input) || m_modBeam.Input == "primary");
	}

	public bool HasAlternateAttack()
	{
		return m_modAlternateFire != null || (m_modMelee != null && m_modMelee.Input == "alternate")
			|| (m_modBeam != null && m_modBeam.Input == "alternate");
	}

	public bool CanAlternateAttack()
	{
		if (!HasAlternateAttack() || Time.time < m_modNextAlternateTime) return false;
		int ammoCost = m_modAlternateFire == null ? 0 : (m_modAlternateFire.AmmoCost ?? 1);
		return ammoCost <= m_ammoLeftInMagazine;
	}

	public bool ShouldAnimateAlternateRecoil()
	{
		return GetAlternateMode() != "melee";
	}

	private string GetAlternateMode()
	{
		return m_modAlternateFire == null
			? (m_modMelee != null && m_modMelee.Input == "alternate" ? "melee" : "beam")
			: m_modAlternateFire.Mode;
	}

	public List<Bullet> BeginBeam(bool i_alternate)
	{
		bool alternateBeam = m_modBeam != null && m_modBeam.Input == "alternate";
		bool forcedAlternate = i_alternate && m_modAlternateFire != null && m_modAlternateFire.Mode == "beam";
		if (m_modBeam == null || (!forcedAlternate && alternateBeam != i_alternate)) return new List<Bullet>();
		StartCoroutine(CoroutineModBeam(i_alternate));
		return new List<Bullet>();
	}

	private IEnumerator CoroutineModBeam(bool i_alternate)
	{
		PlayAttackPresentation(i_alternate);
		float duration = m_modBeam.DurationSeconds ?? 0.5f;
		float interval = m_modBeam.TickIntervalSeconds ?? 0.1f;
		int ammo = m_modBeam.AmmoPerTick ?? 1;
		float end = Time.time + duration;
		while (Time.time < end && m_isPickedUp && (ammo == 0 || m_ammoLeftInMagazine >= ammo))
		{
			if (ammo > 0) m_ammoLeftInMagazine -= ammo;
			ShootBeamTick(m_modBeam.DamageMultiplier ?? 0.25f);
			yield return new WaitForSeconds(interval);
		}
	}

	private void ShootBeamTick(float i_damageMultiplier)
	{
		int mask = LayerMask.GetMask("Actor", "BodyPart", "Platform", "InvisibleWall", "Interactable", "Trap");
		Vector2 origin = GetShotOrigin();
		Vector2 aim = CommonReferences.Instance.GetUtilityTools().GetPosMousePerspectiveCamera();
		Vector2 direction = (aim - origin).normalized;
		RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, GetRuleProfileRange() * m_shootDistance01, mask);
		float damage = m_damage * i_damageMultiplier * GetAmmunitionDamageMultiplier()
			* ExternalRuleProfileFactory.ApplyGunDamageMultiplier(CommonReferences.Instance.GetPlayer().GetStat("DamageMultiplierGun").GetValueTotal());
		new Bullet(this, m_owner, direction, hits, damage, m_knockbackX, m_knockbackY,
			m_penetration + GetAmmunitionPenetrationBonus()).HandleHits();
	}

	public List<Bullet> AlternateAttack()
	{
		List<Bullet> result = new List<Bullet>();
		if (!CanAlternateAttack()) return result;
		float multiplier = m_modAlternateFire == null ? 1f : (m_modAlternateFire.DamageMultiplier ?? 1f);
		int ammoCost = m_modAlternateFire == null ? 0 : (m_modAlternateFire.AmmoCost ?? 1);
		if (ammoCost > m_ammoLeftInMagazine) return result;
		string mode = GetAlternateMode();
		m_modNextAlternateTime = Time.time + (m_modAlternateFire == null ? m_delayBetweenShots : (m_modAlternateFire.CooldownSeconds ?? m_delayBetweenShots));
		if (mode == "melee") result = MeleeAttack(true, multiplier);
		else if (mode == "beam") result = BeginBeam(true);
		else
		{
			result = ShootOnce(multiplier, ammoCost, true);
			if ((m_modAlternateFire?.BurstCount ?? 1) > 1)
				StartCoroutine(CoroutineAlternateBurst(multiplier, ammoCost));
		}
		if ((mode == "melee" || mode == "beam") && ammoCost > 0) m_ammoLeftInMagazine = Mathf.Max(0, m_ammoLeftInMagazine - ammoCost);
		return result;
	}

	private IEnumerator CoroutineAlternateBurst(float i_damageMultiplier, int i_ammoCost)
	{
		int count = m_modAlternateFire?.BurstCount ?? 1;
		float interval = m_modAlternateFire?.BurstIntervalSeconds ?? 0.1f;
		for (int shot = 1; shot < count; shot++)
		{
			yield return new WaitForSeconds(interval);
			if (!m_isPickedUp || m_ammoLeftInMagazine < i_ammoCost) yield break;
			if (m_isOwnerPlayer) CommonReferences.Instance.GetPlayer().PresentModWeaponRecoil(this, GetAlternateRecoilMultiplier());
			ShootOnce(i_damageMultiplier, i_ammoCost, true);
		}
	}

	public float GetAlternateRecoilMultiplier()
	{
		return m_modAlternateFire?.RecoilMultiplier ?? 1f;
	}

	private void SpawnModProjectile(Vector2 i_direction, float i_damage)
	{
		GameObject projectile = new GameObject(gameObject.name + " Projectile");
		Transform stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform;
		projectile.transform.SetParent(stage, true);
		projectile.transform.position = m_bulletSpawnPointDefault != null
			? m_bulletSpawnPointDefault.transform.position : m_owner.GetSkeleton().GetBone("rArmUpper").transform.position;
		projectile.transform.right = i_direction;
		if (m_modProjectileSprite != null)
		{
			SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
			renderer.sprite = m_modProjectileSprite;
			renderer.sortingOrder = 20;
		}
		projectile.AddComponent<ModPhysicalProjectile>().Configure(this, m_owner, i_direction, i_damage,
			m_knockbackX, m_knockbackY, m_penetration + GetAmmunitionPenetrationBonus(), m_modProjectile, m_modImpactSprites);
	}

	public void BeginModAudioOverride(bool i_shoot, bool i_reload, float i_volume)
	{
		m_modSoundVolume = i_volume;
		if (i_shoot) m_audiosShot.Clear();
		if (i_reload) { m_audiosReload.Clear(); m_modReloadAudioOverride = true; }
	}

	private void PlayReloadSound()
	{
		if (m_audiosReload.Count == 0) return;
		if (m_modReloadAudioOverride)
			m_audioSourceSFX.PlayOneShot(m_audiosReload[Random.Range(0, m_audiosReload.Count)], m_modSoundVolume);
		else CommonReferences.Instance.GetManagerAudio().PlayAudioSFXRandom(m_audiosReload, 100f);
	}

	public void AddModAudioClip(bool i_shoot, AudioClip i_clip)
	{
		if (i_clip == null) return;
		if (i_shoot) m_audiosShot.Add(i_clip); else m_audiosReload.Add(i_clip);
	}

	public int GetAmmoMaxTotal()
	{
		return ExternalRuleProfileFactory.ApplyWeaponAmmoCapacity(this, m_ammoMax - GetAmmoMagazineMax());
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
		return ExternalRuleProfileFactory.ApplyWeaponReloadSeconds(this, m_durationReload);
	}

	public float GetDelayBetweenShots()
	{
		return ExternalRuleProfileFactory.ApplyMinimumShotDelay(m_delayBetweenShots);
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
		return m_isAmmoInfinite || DeveloperToolkitPanel.InfiniteAmmoEnabled;
	}

	public bool GetIsSemiFire()
	{
		return !ExternalRuleProfileFactory.ShouldForceAutomaticWeapons() && m_isSemiFire;
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

public sealed class ModPhysicalProjectile : MonoBehaviour
{
	private Gun m_gun;
	private Actor m_owner;
	private Vector2 m_velocity;
	private float m_damage;
	private float m_knockbackX;
	private float m_knockbackY;
	private float m_gravity;
	private float m_explosionRadius;
	private float m_explosionDamageMultiplier;
	private float m_impactLifetime;
	private int m_remainingPenetration;
	private readonly HashSet<Actor> m_hitActors = new HashSet<Actor>();
	private List<Sprite> m_impactSprites;
	private float m_dieAt;

	public void Configure(Gun i_gun, Actor i_owner, Vector2 i_direction, float i_damage, float i_knockbackX,
		float i_knockbackY, int i_penetration, CaptivityReloaded.Modding.WeaponProjectileDefinition i_definition,
		IEnumerable<Sprite> i_impactSprites)
	{
		m_gun = i_gun;
		m_owner = i_owner;
		m_velocity = i_direction.normalized * (i_definition.Speed ?? 15f);
		m_damage = i_damage;
		m_knockbackX = i_knockbackX;
		m_knockbackY = i_knockbackY;
		m_remainingPenetration = Mathf.Max(0, i_penetration);
		m_gravity = i_definition.Gravity ?? 0f;
		m_explosionRadius = i_definition.ExplosionRadius ?? 0f;
		m_explosionDamageMultiplier = i_definition.ExplosionDamageMultiplier ?? 1f;
		m_impactLifetime = i_definition.ImpactLifetimeSeconds ?? 0.15f;
		m_impactSprites = i_impactSprites == null ? new List<Sprite>() : new List<Sprite>(i_impactSprites);
		m_dieAt = Time.time + (i_definition.LifetimeSeconds ?? 5f);
	}

	private void Update()
	{
		if (m_gun == null || m_owner == null || Time.time >= m_dieAt)
		{
			Destroy(gameObject);
			return;
		}
		Vector2 origin = transform.position;
		m_velocity += Vector2.down * m_gravity * Time.deltaTime;
		Vector2 delta = m_velocity * Time.deltaTime;
		if (delta.sqrMagnitude <= 0f) return;
		int mask = LayerMask.GetMask("Actor", "BodyPart", "Platform", "InvisibleWall", "Interactable", "Trap");
		RaycastHit2D[] hits = Physics2D.RaycastAll(origin, delta.normalized, delta.magnitude, mask);
		foreach (RaycastHit2D hit in hits)
		{
			BodyPartActor body = hit.collider.GetComponent<BodyPartActor>();
			if (body != null && body.GetOwner() == m_owner) continue;
			if (body != null && m_hitActors.Contains(body.GetOwner())) continue;
			Bullet bullet = new Bullet(m_gun, m_owner, delta.normalized, new[] { hit }, m_damage,
				m_knockbackX, m_knockbackY, 0, false);
			bullet.HandleHits();
			if (body != null && bullet.IsActorHit(body.GetOwner()))
			{
				m_hitActors.Add(body.GetOwner());
				if (m_remainingPenetration-- > 0) continue;
			}
			Impact(hit.point);
			return;
		}
		transform.position = origin + delta;
		transform.right = m_velocity.normalized;
	}

	private void Impact(Vector2 i_point)
	{
		if (m_explosionRadius > 0f) ApplyExplosion(i_point);
		if (m_impactSprites.Count > 0)
		{
			GameObject effect = new GameObject("Mod Projectile Impact");
			effect.transform.SetParent(transform.parent, true);
			effect.transform.position = i_point;
			SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
			renderer.sprite = m_impactSprites[Random.Range(0, m_impactSprites.Count)];
			renderer.sortingOrder = 21;
			Destroy(effect, m_impactLifetime);
		}
		Destroy(gameObject);
	}

	private void ApplyExplosion(Vector2 i_point)
	{
		HashSet<Actor> actors = new HashSet<Actor>();
		foreach (Collider2D collider in Physics2D.OverlapCircleAll(i_point, m_explosionRadius, LayerMask.GetMask("BodyPart")))
		{
			BodyPartActor body = collider.GetComponent<BodyPartActor>();
			if (body == null || body.GetOwner() == null || body.GetOwner() == m_owner || actors.Contains(body.GetOwner())) continue;
			actors.Add(body.GetOwner());
			Bullet explosion = new Bullet(m_gun, m_owner, Vector2.zero, new RaycastHit2D[0],
				m_damage * m_explosionDamageMultiplier, m_knockbackX, m_knockbackY, 0, false);
			explosion.HitBodyPartDirect(body, body.transform.position);
		}
	}
}

public sealed class ModAmmunitionEffect : MonoBehaviour
{
	private Actor m_target;
	private StatModifier m_slowModifier;

	public void Configure(Actor i_target, CaptivityReloaded.Modding.WeaponAmmunitionDefinition i_definition)
	{
		m_target = i_target;
		if (m_target == null || i_definition == null) { Destroy(this); return; }
		float duration = i_definition.EffectDurationSeconds ?? 0f;
		float damage = i_definition.DamageOverTime ?? 0f;
		if (damage > 0f && duration > 0f)
			StartCoroutine(ApplyDamageOverTime(damage, duration, i_definition.TickIntervalSeconds ?? 0.5f));
		float slow = i_definition.SlowMultiplier ?? 1f;
		float slowDuration = i_definition.SlowDurationSeconds ?? 0f;
		if (slow < 1f && slowDuration > 0f)
		{
			float speed = m_target.GetStat("SpeedMax").GetValueTotal();
			m_slowModifier = m_target.AddStatModifier("SpeedMax", speed * (slow - 1f));
			StartCoroutine(RemoveSlowAfter(slowDuration));
		}
		StartCoroutine(DestroyAfter(Mathf.Max(duration, slowDuration) + 0.05f));
	}

	private IEnumerator ApplyDamageOverTime(float i_damage, float i_duration, float i_interval)
	{
		float end = Time.time + i_duration;
		while (m_target != null && !m_target.IsDead() && Time.time < end)
		{
			yield return new WaitForSeconds(i_interval);
			if (m_target != null && !m_target.IsDead() && Time.time <= end) m_target.TakeDamage(i_damage);
		}
	}

	private IEnumerator RemoveSlowAfter(float i_duration)
	{
		yield return new WaitForSeconds(i_duration);
		RemoveSlow();
	}

	private IEnumerator DestroyAfter(float i_duration)
	{
		yield return new WaitForSeconds(i_duration);
		Destroy(this);
	}

	private void OnDestroy() { RemoveSlow(); }

	private void RemoveSlow()
	{
		if (m_slowModifier == null || m_target == null) return;
		m_target.RemoveStatModifier(m_slowModifier);
		m_slowModifier = null;
	}
}
