using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{

	public string SpawnGun;

	public delegate void FireGun(List<Bullet> i_bulletsFired);

	public new delegate void DelPickUp(PickUpable i_pickUpable);

	public delegate void DelUse(Interactable i_interactable);

	public delegate void DelOnBeingRaped();

	public delegate void DelOnOrgasm();

	public delegate void DelOnRapeEnd();

	public delegate void DelOnFetusInsert(Fetus i_fetusInserted);

	public delegate void DelOnLabor();

	public delegate void DelOnLaborEnd();

	public delegate void DelOnBirth(Actor i_actorChild);

	public delegate void DelOnBirthEnd();

	public new delegate void DelOnDie();

	public delegate void GetHit();

	public delegate void DelTakeDamage();

	public delegate void DelOnEndExhaustion();

	public delegate void DelGrounded();

	public delegate void DelRagdoll();

	public delegate void DelRagdollEnd();

	public delegate void DelKill(NPC i_npcKilled);

	[Header("Player")]
	[SerializeField]
	private ParticleSystem m_particleOrgasm;

	[SerializeField]
	private AudioClip m_audioOrgasm;

	[SerializeField]
	private List<AudioClip> m_audiosVoiceOrgasm = new List<AudioClip>();

	[SerializeField]
	private ParticleSystem m_particleLibidoJuice;

	[SerializeField]
	private ParticleSystem m_particleLibidoSpeak;

	[SerializeField]
	private string m_descriptionPlayer;

	[SerializeField]
	private int m_age;

	[SerializeField]
	private int m_numOfHeartsMax;

	[SerializeField]
	private int m_numOfHeartsCurrent;

	[SerializeField]
	private ManagerArmWeaponPlayer m_managerArmWeaponPlayer;

	[SerializeField]
	private float m_rangePickUp;

	[SerializeField]
	private Collider2D m_colliderFeet;

	[SerializeField]
	private PlayerCollisionHandler m_playerCollisionHandler;

	[SerializeField]
	private GameObject m_posMiddleFace;

	[SerializeField]
	protected float m_strengthCurrent;

	[SerializeField]
	protected float m_strengthMax;

	[SerializeField]
	protected float m_libidoMax;

	[SerializeField]
	protected float m_libidoCurrent;

	[SerializeField]
	protected float m_pleasureMax;

	[SerializeField]
	protected float m_pleasureCurrent;

	[SerializeField]
	private int m_delayBetweenAbleToBeRaped;

	[SerializeField]
	private List<AudioClip> m_audiosVoiceGetHit = new List<AudioClip>();

	[SerializeField]
	private AudioClip m_audioRagdoll;

	[SerializeField]
	private List<AudioClip> m_audiosCum = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosFootStep = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosDash = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosVoiceLaborStart = new List<AudioClip>();

	[SerializeField]
	private AudioClip m_audioLaborStart;

	[SerializeField]
	private AnimationClip m_animBirth;

	[SerializeField]
	private AudioClip m_audioBirth;

	[SerializeField]
	private List<AudioClip> m_audiosVoiceBirth = new List<AudioClip>();

	[SerializeField]
	private ParticleSystem m_particleBirth;

	private Weapon m_weaponEquipped;

	private Weapon m_weaponPrevious;

	private Raper m_raperCurrent;

	private List<Fetus> m_fetuses = new List<Fetus>();

	private StatePlayer m_statePlayerCurrent;

	private Color m_colorStart;

	private bool m_isWakingUp;

	private bool m_isReloading;

	private bool m_isEquipping;

	private bool m_isUsingUsable;

	private bool m_isSprinting;

	private bool m_isWalkingBackwards;

	private bool m_isCanSwitchFacingSide = true;

	private bool m_isHoldingGrapple;

	private bool m_isCrouching;

	private bool m_isExposing;

	private bool m_isHypnotized;

	private bool m_isFear;

	private float m_staminaCurrent;

	private bool m_isCanRegenerateHealth;

	private bool m_isCanRegenerateStamina = true;

	private bool m_isWasSprinting;

	private bool m_isDashLeft = true;

	private bool m_isDashWindowOpen = true;

	private Coroutine m_coroutineReload;

	private Coroutine m_coroutineWaitBeforeRegenerateHealth;

	private Coroutine m_coroutineWaitBeforeRegenerateStamina;

	private Coroutine m_coroutineRagdoll;

	private Coroutine m_coroutineGetUp;

	private Coroutine m_coroutineWaitForDashDelay;

	private Coroutine m_coroutineWaitForDashToEnd;

	private Coroutine m_coroutineWaitForEquipWeapon;

	private Coroutine m_coroutineClimb;

	private Coroutine m_coroutineBecomeInvulnerable;

	private Coroutine m_coroutineStartUseEquippedUsable;

	private Coroutine m_coroutineSetIsCrouching;

	private Coroutine m_coroutineSetIsExposing;

	private Coroutine m_coroutineStrengthDrain;

	private Coroutine m_coroutineWakeUp;

	public event FireGun OnShoot;

	public new event DelPickUp OnPickUp;

	public event DelUse OnUse;

	public event DelOnBeingRaped OnBeingRaped;

	public event DelOnOrgasm OnOrgasm;

	public event DelOnRapeEnd OnRapeEnd;

	public event DelOnFetusInsert OnFetusInsert;

	public event DelOnLabor OnLabor;

	public event DelOnLaborEnd OnLaborEnd;

	public event DelOnBirth OnBirth;

	public event DelOnBirthEnd OnBirthEnd;

	public new event DelOnDie OnDie;

	public event GetHit OnGetHit;

	public event DelTakeDamage OnTakeDamage;

	public event DelOnEndExhaustion OnEndExhaustion;

	public event DelGrounded OnGrounded;

	public event DelRagdoll OnRagdoll;

	public event DelRagdoll OnRagdollEnd;

	public event DelKill OnKill;

	public override void Awake()
	{
		base.Awake();
		m_statePlayerCurrent = StatePlayer.None;
		m_libidoCurrent = 0f;
		m_pleasureCurrent = 0f;
		m_strengthCurrent = m_strengthMax;
		m_isReloading = false;
		m_isWalkingBackwards = false;
		m_staminaCurrent = GetStat("HealthMax").GetValueTotal();
	}

	protected override void InitializeStats()
	{
		base.InitializeStats();
		m_stats.Add(new Stat(StatNamePlayer.DamageMultiplierGun, 1f));
		m_stats.Add(new Stat(StatNamePlayer.SpeedSprint, 4f));
		m_stats.Add(new Stat(StatNamePlayer.PowerJump, 18f));
		m_stats.Add(new Stat(StatNamePlayer.PowerDash, 40f));
		m_stats.Add(new Stat("LibidoRegen", 0f));
		m_stats.Add(new Stat("LibidoRegenRate", 0f));
	}

	public override void Start()
	{
		base.Start();
		OnGrounded += ApplyOnGroundedVel;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_isCanAttack = true;
		StartCoroutine(CoroutineUpdateLibido());
		m_isCanRegenerateStamina = true;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		HandleStamina();
		HandleAnimatorLayerWeight();
		HandleLibidoParticle();
		if (GetVelocity().x != 0f)
		{
			if (GetVelocity().x > 0f)
			{
				if (GetIsFacingLeft())
				{
					m_isWalkingBackwards = true;
				}
				else
				{
					m_isWalkingBackwards = false;
				}
			}
			else if (!GetIsFacingLeft())
			{
				m_isWalkingBackwards = true;
			}
			else
			{
				m_isWalkingBackwards = false;
			}
		}
		else
		{
			m_isWalkingBackwards = false;
		}
	}

	private void HandleLibidoParticle()
	{
		ParticleSystem.MainModule main = m_particleLibidoJuice.main;
		main.maxParticles = 0;
		if (m_libidoCurrent >= m_libidoMax / 4f)
		{
			main.maxParticles = 1;
		}
		if (m_libidoCurrent >= m_libidoMax / 4f * 2f)
		{
			main.maxParticles = 2;
		}
		if (m_libidoCurrent >= m_libidoMax / 4f * 3f)
		{
			main.maxParticles = 3;
		}
		if (m_libidoCurrent >= m_libidoMax - 5f)
		{
			main.maxParticles = 5;
		}
	}

	private void HandleAnimatorLayerWeight()
	{
		if (m_statePlayerCurrent != StatePlayer.BeingRaped)
		{
			if (m_stateActorCurrent == StateActor.Climbing || m_isFear)
			{
				m_animator.SetLayerWeight(m_animator.GetLayerIndex("Tired"), 0f);
				m_animator.SetLayerWeight(m_animator.GetLayerIndex("Libido"), 0f);
				return;
			}
			float weight = 0f;
			float weight2 = GetLibidoCurrent() / GetLibidoMax();
			m_animator.SetLayerWeight(m_animator.GetLayerIndex("Tired"), weight);
			m_animator.SetLayerWeight(m_animator.GetLayerIndex("Libido"), weight2);
		}
	}

	public void FaceSideAim()
	{
		if (CommonReferences.Instance.GetUtilityTools().GetPosMousePerspectiveCamera().x < base.transform.position.x)
		{
			SetIsFacingLeft(i_isFacingLeft: true);
		}
		else
		{
			SetIsFacingLeft(i_isFacingLeft: false);
		}
		PlaceEquippableOnHand();
	}

	public override void UpdateAnim()
	{
		if (m_statePlayerCurrent == StatePlayer.BeingRaped || m_statePlayerCurrent == StatePlayer.Labor || m_stateActorCurrent == StateActor.Climbing || m_statePlayerCurrent == StatePlayer.Grappling || m_statePlayerCurrent == StatePlayer.Dashing || m_stateActorCurrent == StateActor.Ragdoll)
		{
			m_animator.speed = 1f;
			return;
		}
		if (m_isDead)
		{
			m_statePlayerCurrent = StatePlayer.Dead;
			SetAllAnimatorBoolsToFalse();
			m_animator.SetBool("IsDead", value: true);
			return;
		}
		if (GetComponent<Rigidbody2D>().velocity == Vector2.zero)
		{
			SetStateToIdleAndNone();
		}
		if (m_stateActorCurrent != StateActor.Jumping && (GetComponent<Rigidbody2D>().velocity.x <= 0f - m_miniumXSpeedForAnim || GetComponent<Rigidbody2D>().velocity.x >= m_miniumXSpeedForAnim))
		{
			m_stateActorCurrent = StateActor.Moving;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(GetPosFeet(), Vector2.down, 0.5f, LayerMask.GetMask("Platform", "Ledge"));
		if (!raycastHit2D)
		{
			m_stateActorCurrent = StateActor.Jumping;
			GetComponent<Rigidbody2D>().gravityScale = 1.5f;
		}
		if ((bool)raycastHit2D && m_stateActorCurrent == StateActor.Jumping)
		{
			this.OnGrounded?.Invoke();
			m_stateActorCurrent = StateActor.Moving;
			GetComponent<Rigidbody2D>().gravityScale = 1f;
		}
		SetAllAnimatorBoolsToFalse();
		if (m_isCrouching)
		{
			m_animator.SetBool("IsCrouching", value: true);
		}
		switch (m_stateActorCurrent)
		{
		case StateActor.Moving:
			if (m_isSprinting)
			{
				m_animator.SetBool("IsSprinting", value: true);
				m_animator.SetBool("IsMovingForward", value: true);
			}
			else if (m_isWalkingBackwards)
			{
				m_animator.SetBool("IsMovingBackward", value: true);
			}
			else
			{
				m_animator.SetBool("IsMovingForward", value: true);
			}
			break;
		case StateActor.Jumping:
			m_animator.SetBool("IsJumping", value: true);
			break;
		}
		if (m_stateActorCurrent != StateActor.Moving || m_statePlayerCurrent == StatePlayer.Dashing || m_isWalkingBackwards)
		{
			m_animator.speed = 1f;
		}
	}

	private void SetAllAnimatorBoolsToFalse()
	{
		m_animator.SetBool("IsMovingForward", value: false);
		m_animator.SetBool("IsSprinting", value: false);
		m_animator.SetBool("IsMovingBackward", value: false);
		m_animator.SetBool("IsJumping", value: false);
		m_animator.SetBool("IsCrouching", value: false);
	}

	private void HandleStamina()
	{
		if (GetIsCanRegenerateHealthAndStamina())
		{
			RegenerateStamina();
		}
	}

	private bool GetIsCanRegenerateHealthAndStamina()
	{
		if (m_statePlayerCurrent != StatePlayer.BeingRaped && m_statePlayerCurrent != StatePlayer.Labor && m_statePlayerCurrent != StatePlayer.Dead && m_stateActorCurrent != StateActor.Ragdoll)
		{
			return true;
		}
		return false;
	}

	private void RegenerateStamina()
	{
		if (m_isCanRegenerateStamina)
		{
			m_staminaCurrent += 0.5f;
			if (m_staminaCurrent >= GetStat("HealthMax").GetValueTotal())
			{
				m_staminaCurrent = GetStat("HealthMax").GetValueTotal();
			}
		}
	}

	private void DepleteStamina()
	{
		m_staminaCurrent -= 0.25f;
		if (m_staminaCurrent < 0f)
		{
			m_staminaCurrent = 0f;
		}
	}

	public void DepleteStamina(float i_amount)
	{
		m_staminaCurrent -= i_amount;
		if (m_staminaCurrent < 0f)
		{
			m_staminaCurrent = 0f;
		}
	}

	private IEnumerator CoroutineWaitBeforeRegenerateStamina()
	{
		m_isCanRegenerateStamina = false;
		yield return new WaitForSeconds(1f);
		m_isCanRegenerateStamina = true;
	}

	private IEnumerator CoroutineWaitBeforeRegenerateHealth()
	{
		m_isCanRegenerateHealth = false;
		yield return new WaitForSeconds(3f);
		m_isCanRegenerateHealth = true;
	}

	private void HandleHealthRegeneration()
	{
		if (GetIsCanRegenerateHealthAndStamina() && m_isCanRegenerateHealth && !(m_healthCurrent > GetStat("HealthMax").GetValueTotal() / 4f))
		{
			m_healthCurrent += 0.25f;
			if (m_healthCurrent >= GetStat("HealthMax").GetValueTotal() / 4f)
			{
				m_healthCurrent = GetStat("HealthMax").GetValueTotal() / 4f;
			}
		}
	}

	private IEnumerator CoroutineWaitForDashDelay()
	{
		m_animator.SetBool("IsDashing", value: true);
		m_isDashWindowOpen = false;
		yield return new WaitForSeconds(0.33f);
		m_isDashWindowOpen = true;
		m_animator.SetBool("IsDashing", value: false);
	}

	public void Dash(bool i_left)
	{
		if (!m_isDashWindowOpen || (i_left && m_isTouchingLeftWall) || (!i_left && m_isTouchingRightWall))
		{
			return;
		}
		if (m_staminaCurrent < GetStat("HealthMax").GetValueTotal() / 4f)
		{
			CommonReferences.Instance.GetManagerHud().GetStaminaBar().FlashNoStamina(2);
			return;
		}
		SetVelocity(new Vector2(0f, GetVelocity().y));
		GetComponent<Rigidbody2D>().drag = 4f;
		RaycastHit2D raycastHit2D = ((!i_left) ? Physics2D.Raycast(GetPos(), Vector2.right, 4.5f, LayerMask.GetMask("Platform")) : Physics2D.Raycast(GetPos(), Vector2.left, 4.5f, LayerMask.GetMask("Platform")));
		if ((bool)raycastHit2D)
		{
			if (i_left)
			{
				GetComponent<Rigidbody2D>().AddForce(new Vector2((0f - GetStat(StatNamePlayer.PowerDash).GetValueTotal()) / 2f, 0f), ForceMode2D.Impulse);
			}
			else
			{
				GetComponent<Rigidbody2D>().AddForce(new Vector2(GetStat(StatNamePlayer.PowerDash).GetValueTotal() / 2f, 0f), ForceMode2D.Impulse);
			}
		}
		else if (i_left)
		{
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f - GetStat(StatNamePlayer.PowerDash).GetValueTotal(), 0f), ForceMode2D.Impulse);
		}
		else
		{
			GetComponent<Rigidbody2D>().AddForce(new Vector2(GetStat(StatNamePlayer.PowerDash).GetValueTotal(), 0f), ForceMode2D.Impulse);
		}
		SetIsCrouching(i_isCrouching: false);
		m_statePlayerCurrent = StatePlayer.Dashing;
		m_staminaCurrent -= 33f;
		if (m_coroutineWaitBeforeRegenerateStamina != null)
		{
			StopCoroutine(m_coroutineWaitBeforeRegenerateStamina);
		}
		m_coroutineWaitBeforeRegenerateStamina = StartCoroutine(CoroutineWaitBeforeRegenerateStamina());
		GameObject gameObject = Object.Instantiate(ResourceContainer.Resources.m_particleDash, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		gameObject.transform.position = GetPosFeet();
		if (i_left)
		{
			if (!GetIsFacingLeft())
			{
				m_animator.Play("DashBackward", 0, 0f);
			}
			else
			{
				m_animator.Play("DashForward", 0, 0f);
			}
		}
		else
		{
			gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
			if (GetIsFacingLeft())
			{
				m_animator.Play("DashBackward", 0, 0f);
			}
			else
			{
				m_animator.Play("DashForward", 0, 0f);
			}
		}
		gameObject.SetActive(value: true);
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(gameObject, 2f);
		PlayAudio(m_audiosDash);
		if (m_coroutineWaitForDashDelay != null)
		{
			StopCoroutine(m_coroutineWaitForDashDelay);
		}
		m_coroutineWaitForDashDelay = StartCoroutine(CoroutineWaitForDashDelay());
		if (m_coroutineWaitForDashToEnd != null)
		{
			StopCoroutine(m_coroutineWaitForDashToEnd);
		}
		m_coroutineWaitForDashToEnd = StartCoroutine(CoroutineWaitForDashToEnd(i_left));
		BecomeInvulnerableDash(0.25f);
	}

	private IEnumerator CoroutineWaitForDashToEnd(bool i_isDashingLeft)
	{
		bool l_isDone = false;
		while (!l_isDone)
		{
			yield return new WaitForEndOfFrame();
			RaycastHit2D raycastHit2D = ((!i_isDashingLeft) ? Physics2D.Raycast(GetPos(), Vector2.right, 2f, LayerMask.GetMask("Platform")) : Physics2D.Raycast(GetPos(), Vector2.left, 2f, LayerMask.GetMask("Platform")));
			_ = (bool)raycastHit2D;
			if (GetVelocity().x < 7f && GetVelocity().x > -7f)
			{
				l_isDone = true;
			}
		}
		if (m_statePlayerCurrent == StatePlayer.Dashing && m_stateActorCurrent != StateActor.Ragdoll)
		{
			SetStateToIdleAndNone();
		}
		GetComponent<Rigidbody2D>().drag = 0f;
	}

	private void OnGetHitDuringDash()
	{
		OnGetHit -= OnGetHitDuringDash;
		StopCoroutine(m_coroutineWaitForDashToEnd);
		GetComponent<Rigidbody2D>().drag = 0f;
		Ragdoll(12f);
	}

	public override void MoveHorizontal(bool i_left)
	{
		if (!m_isCanMove || (i_left && m_isTouchingLeftWall) || (!i_left && m_isTouchingRightWall))
		{
			return;
		}
		float num = GetStat("SpeedMax").GetValueTotal();
		float valueTotal = GetStat("SpeedAccel").GetValueTotal();
		if (m_isSprinting)
		{
			num += GetStat(StatNamePlayer.SpeedSprint).GetValueTotal();
		}
		if (m_stateActorCurrent != StateActor.Jumping)
		{
			if (m_isWalkingBackwards)
			{
				num *= 0.65f;
			}
			if (m_isCrouching)
			{
				num *= 0.65f;
			}
		}
		Vector2 vector = default(Vector2);
		vector = ((!i_left) ? new Vector2(1f, 0f) : new Vector2(-1f, 0f));
		Vector2 vector2 = vector * Time.deltaTime * (valueTotal * 10f);
		GetRigidbody2D().velocity += vector2;
		if (GetVelocity().x * vector.x >= num)
		{
			GetRigidbody2D().velocity = new Vector2(num * vector.x, GetRigidbody2D().velocity.y);
		}
	}

	private void HandleStairs()
	{
		float num = 0.3f;
		Vector2 posFeet = GetPosFeet();
		posFeet.y += 0.05f;
		Vector2 posFeet2 = GetPosFeet();
		posFeet2.y += 0.4f;
		Debug.DrawRay(posFeet, Vector2.right * num, Color.red);
		Debug.DrawRay(posFeet2, Vector2.right * num, Color.blue);
		RaycastHit2D raycastHit2D = default(RaycastHit2D);
		RaycastHit2D raycastHit2D2 = default(RaycastHit2D);
		if (GetIsWalkingLeft())
		{
			raycastHit2D = Physics2D.Raycast(posFeet, Vector2.left, num, LayerMask.GetMask("Platform"));
			raycastHit2D2 = Physics2D.Raycast(posFeet2, Vector2.left, num, LayerMask.GetMask("Platform"));
		}
		else
		{
			raycastHit2D = Physics2D.Raycast(posFeet, Vector2.right, num, LayerMask.GetMask("Platform"));
			raycastHit2D2 = Physics2D.Raycast(posFeet2, Vector2.right, num, LayerMask.GetMask("Platform"));
		}
		if (((bool)raycastHit2D & !raycastHit2D2) && raycastHit2D.transform.eulerAngles.z == 0f)
		{
			float num2 = 0f;
			num2 = ((!GetIsWalkingLeft()) ? 0.1f : (-0.1f));
			PlaceFeetOnPos(new Vector2(GetPos().x + num2, raycastHit2D.collider.transform.position.y));
		}
	}

	private void HandleSlope()
	{
		float distance = 0.25f;
		Vector2 posFeet = GetPosFeet();
		posFeet.y += 0.1f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(posFeet, Vector2.left, distance, LayerMask.GetMask("Platform"));
		raycastHit2D = ((!GetIsWalkingLeft()) ? Physics2D.Raycast(posFeet, Vector2.right, distance, LayerMask.GetMask("Platform")) : Physics2D.Raycast(posFeet, Vector2.left, distance, LayerMask.GetMask("Platform")));
		if ((bool)raycastHit2D)
		{
			float num = 0f;
			num = ((!GetIsWalkingLeft()) ? (0f - Vector2.Angle(raycastHit2D.normal, Vector2.up)) : Vector2.Angle(raycastHit2D.normal, Vector2.up));
			SetVelocity((GetVelocity() * num).normalized * 8f);
		}
	}

	private void ApplyOnGroundedVel()
	{
		StartCoroutine(CoroutineApplyOnGroundedVel());
	}

	private IEnumerator CoroutineApplyOnGroundedVel()
	{
		Vector2 l_velOld = GetRigidbody2D().velocity;
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		bool flag = false;
		if (l_velOld.x > 0f)
		{
			if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.MoveRight))
			{
				flag = true;
			}
		}
		else if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.MoveLeft))
		{
			flag = true;
		}
		if (flag)
		{
			GetRigidbody2D().velocity = new Vector2(l_velOld.x, GetVelocity().y);
		}
	}

	public void Jump()
	{
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		velocity.y += GetStat(StatNamePlayer.PowerJump).GetValueTotal();
		GetComponent<Rigidbody2D>().velocity = velocity;
	}

	public void Climb(Ledge i_ledgeToClimb)
	{
		if (!m_isUsingUsable && m_isThinking && !m_isEquipping && !m_isDead)
		{
			if (m_coroutineClimb != null)
			{
				StopCoroutine(m_coroutineClimb);
			}
			if (m_isReloading)
			{
				InterruptReload();
			}
			if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
			{
				CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
			}
			SetIsCrouching(i_isCrouching: false);
			SetIsExposing(i_isExposing: false);
			m_coroutineClimb = StartCoroutine(CoroutineClimb(i_ledgeToClimb));
		}
	}

	private IEnumerator CoroutineClimb(Ledge i_ledgeToClimb)
	{
		if (GetIsCanClimb() && GetPos().y - 1.5f < i_ledgeToClimb.transform.position.y)
		{
			m_stateActorCurrent = StateActor.Climbing;
			StopMoving();
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
			GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
			PlaceFeetOnPos(i_ledgeToClimb.transform.position);
			m_animator.Play("Climb");
			yield return new WaitForSeconds(1.5f);
			GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
			SetStateToIdleAndNone();
			m_coroutineClimb = null;
		}
	}

	private void InterruptClimb()
	{
		if (m_coroutineClimb != null)
		{
			StopCoroutine(m_coroutineClimb);
			m_coroutineClimb = null;
		}
		GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
		if (m_stateActorCurrent != StateActor.Ragdoll)
		{
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
			SetStateToIdleAndNone();
		}
	}

	private bool GetIsCanClimb()
	{
		if (m_statePlayerCurrent == StatePlayer.BeingRaped || m_statePlayerCurrent == StatePlayer.Labor || m_stateActorCurrent == StateActor.Ragdoll)
		{
			return false;
		}
		return true;
	}

	public void UseEquippedEquippable(bool i_isAltFire)
	{
		if (!(GetEquippableEquipped() == null))
		{
			if (GetEquippableEquipped() is Gun)
			{
				UseEquippedGun(i_isAltFire);
			}
			else if (GetEquippableEquipped() is Usable)
			{
				StartUseEquippedUsable();
			}
			else if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.Fire))
			{
				GetEquippableEquipped().Use(i_isAltFire: false);
			}
		}
	}

	private void UseEquippedGun(bool i_isAltFire)
	{
		Gun gun = (Gun)GetEquippableEquipped();
		if (!i_isAltFire)
		{
			gun.Use(i_isAltFire: false);
			if (gun.GetAmmoMagazineLeft() < 1)
			{
				return;
			}
			m_managerArmWeaponPlayer.ShootGun(gun);
			List<Bullet> i_bulletsFired = gun.Shoot();
			this.OnShoot?.Invoke(i_bulletsFired);
			if (gun.GetIsKnocksbackShooterFire())
			{
				TakeKnockbackFromWeapon(gun);
			}
		}
		else
		{
			gun.Use(i_isAltFire: true);
			if (gun.GetIsKnocksbackShooterAltFire())
			{
				TakeKnockbackFromWeapon(gun);
			}
		}
		StartCoroutine(CoroutineWaitForNextAttack());
	}

	public void HandleGrappling()
	{
		Grapple grapple = null;
		foreach (PickUpable allPickUpable in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllPickUpables())
		{
			if (allPickUpable is Grapple)
			{
				grapple = (Grapple)allPickUpable;
				break;
			}
		}
		grapple.GetHookCurrent();
		if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.Jump) && GetComponent<SpringJoint2D>().distance > 1.5f)
		{
			GetComponent<SpringJoint2D>().distance -= 0.1f;
		}
		if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.Crouch) && !Physics2D.Raycast(new Vector2(GetPos().x, GetPos().y - GetComponent<SpriteRenderer>().bounds.size.y / 2f), Vector2.down, 0.1f, LayerMask.GetMask("Platform", "Ledge")))
		{
			GetComponent<SpringJoint2D>().distance += 0.1f;
		}
	}

	private void TakeKnockbackFromWeapon(Gun i_gun)
	{
	}

	public void Reload()
	{
		Gun obj = (Gun)GetEquippableEquipped();
		if (obj.GetReloadTypeGun() == GunReloadType.PumpAction)
		{
			ReloadPumpAction();
		}
		else
		{
			m_coroutineReload = StartCoroutine(CoroutineReload());
		}
		if (!obj.IsReloadCancelable() && CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
		{
			CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
		}
	}

	private IEnumerator CoroutineReload()
	{
		Gun l_weapon = (Gun)GetEquippableEquipped();
		if (l_weapon.GetReloadTypeGun() == GunReloadType.SingleBarrel)
		{
			m_animator.SetTrigger("ReloadSingleBarrel");
		}
		else
		{
			switch (l_weapon.GetWeaponType())
			{
			case WeaponType.Pistol:
				m_animator.SetTrigger("ReloadPistol");
				break;
			case WeaponType.Smg:
				if (l_weapon.GetHoldTypeGun() == GunHoldType.OneHanded)
				{
					m_animator.SetTrigger("ReloadPistol");
				}
				else
				{
					m_animator.SetTrigger("ReloadMagazineSmg");
				}
				break;
			case WeaponType.Shotgun:
				if (l_weapon.GetHoldTypeGun() == GunHoldType.OneHanded)
				{
					m_animator.SetTrigger("ReloadPistol");
				}
				else
				{
					m_animator.SetTrigger("ReloadMagazineRifle");
				}
				break;
			case WeaponType.Rifle:
				m_animator.SetTrigger("ReloadMagazineRifle");
				break;
			}
		}
		m_isReloading = true;
		l_weapon.AnimateReload();
		yield return new WaitForSeconds(l_weapon.GetDurationReload());
		l_weapon.Reload();
		m_isReloading = false;
	}

	private void ReloadPumpAction()
	{
		Gun obj = (Gun)GetEquippableEquipped();
		m_isReloading = true;
		m_animator.SetTrigger("ReloadPumpAction");
		obj.AnimateReload();
	}

	public void InterruptReload()
	{
		if (m_coroutineReload != null)
		{
			StopCoroutine(m_coroutineReload);
		}
		m_animator.Play("EmptyStateWeaponLayer");
		m_isReloading = false;
		if (GetEquippableEquipped() != null && GetEquippableEquipped() is Gun)
		{
			((Gun)GetEquippableEquipped()).ReloadInterrupt();
		}
	}

	private IEnumerator CoroutineWaitForNextAttack()
	{
		Gun gun = (Gun)GetEquippableEquipped();
		m_isCanAttack = false;
		m_isAttacking = true;
		float num = gun.GetDelayBetweenShots();
		if (IsStatusEffectAppliedAlready("Double Trigger"))
		{
			num *= 0.5f;
		}
		yield return new WaitForSeconds(num);
		m_isCanAttack = true;
		m_isAttacking = false;
	}

	public void EquipWeapon(Weapon i_weapon)
	{
		if (i_weapon == null)
		{
			if (m_isReloading)
			{
				InterruptReload();
			}
			CommonReferences.Instance.GetPlayerController().GetInventory().HideAllItems();
			m_weaponEquipped = null;
			return;
		}
		StartCoroutine(CoroutineWaitForShootCancelAfterEquip());
		if (m_weaponEquipped == i_weapon)
		{
			return;
		}
		if (m_isReloading)
		{
			InterruptReload();
		}
		if (GetEquippableEquipped() != null)
		{
			if (GetEquippableEquipped() is Usable)
			{
				Usable usable = (Usable)GetEquippableEquipped();
				if (usable.IsUsed())
				{
					DropPickupAble(usable);
				}
				else
				{
					GetEquippableEquipped().gameObject.SetActive(value: false);
				}
			}
			else
			{
				GetEquippableEquipped().gameObject.SetActive(value: false);
			}
		}
		m_weaponPrevious = m_weaponEquipped;
		m_weaponEquipped = i_weapon;
		CommonReferences.Instance.GetPlayerController().GetInventory().HideAllItems();
		m_isHoldingGrapple = false;
		GetEquippableEquipped().gameObject.SetActive(value: true);
		m_weaponEquipped.Equip();
		PlaceEquippableOnHand();
		m_managerArmWeaponPlayer.ShowCurrentArm();
		if (m_coroutineWaitForEquipWeapon != null)
		{
			StopCoroutine(m_coroutineWaitForEquipWeapon);
		}
		m_coroutineWaitForEquipWeapon = StartCoroutine(CoroutineWaitForEquipWeapon());
		int num = Random.Range(0, 2);
		string text = "WeaponSwitch" + num;
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/" + text));
	}

	public void EquipPreviousWeapon()
	{
		if (m_weaponPrevious != null)
		{
			EquipWeapon(m_weaponPrevious);
		}
	}

	public void EquipNextWeapon(bool i_onlyUsables, bool i_backwards = false)
	{
		List<Weapon> list = new List<Weapon>();
		foreach (Weapon allEquippable in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllEquippables())
		{
			if ((allEquippable is Usable) == i_onlyUsables)
			{
				list.Add(allEquippable);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		int num = list.IndexOf(m_weaponEquipped);
		if (num < 0)
		{
			num = i_backwards ? 0 : -1;
		}
		int num2 = i_backwards ? -1 : 1;
		EquipWeapon(list[(num + num2 + list.Count) % list.Count]);
	}

	private IEnumerator CoroutineWaitForShootCancelAfterEquip()
	{
		m_isCanAttack = false;
		yield return new WaitForSeconds(0.2f);
		m_isCanAttack = true;
	}

	private IEnumerator CoroutineWaitForEquipWeapon()
	{
		switch (GetEquippableEquipped().GetWeaponType())
		{
		case WeaponType.Pistol:
			m_animator.Play("EquipPistol", m_animator.GetLayerIndex("Weapon"), 0f);
			break;
		case WeaponType.Smg:
			if (((Gun)GetEquippableEquipped()).GetHoldTypeGun() == GunHoldType.OneHanded)
			{
				m_animator.Play("EquipPistol", m_animator.GetLayerIndex("Weapon"), 0f);
			}
			else
			{
				m_animator.Play("EquipSmg", m_animator.GetLayerIndex("Weapon"), 0f);
			}
			break;
		case WeaponType.Shotgun:
			if (((Gun)GetEquippableEquipped()).GetHoldTypeGun() == GunHoldType.OneHanded)
			{
				m_animator.Play("EquipPistol", m_animator.GetLayerIndex("Weapon"), 0f);
			}
			else
			{
				m_animator.Play("EquipShotgun", m_animator.GetLayerIndex("Weapon"), 0f);
			}
			break;
		case WeaponType.Rifle:
			m_animator.Play("EquipShotgun", m_animator.GetLayerIndex("Weapon"), 0f);
			break;
		case WeaponType.Special:
			m_animator.Play("EquipPistol", m_animator.GetLayerIndex("Weapon"), 0f);
			break;
		case WeaponType.Usable:
			m_animator.Play("EquipPistol", m_animator.GetLayerIndex("Weapon"), 0f);
			break;
		}
		m_isEquipping = true;
		yield return new WaitForSeconds(GetEquippableEquipped().GetDurationEquip());
		m_isEquipping = false;
	}

	public void ReEquipCurrentWeapon()
	{
		EquipWeapon(m_weaponEquipped);
	}

	public void EquipOrUnEquipGrapple()
	{
		if (m_isHoldingGrapple)
		{
			EquipWeapon(m_weaponEquipped);
			return;
		}
		if (GetEquippableEquipped() != null)
		{
			GetEquippableEquipped().gameObject.SetActive(value: false);
		}
		CommonReferences.Instance.GetPlayerController().GetInventory().HideAllItems();
		m_isHoldingGrapple = true;
		GetEquippableEquipped().gameObject.SetActive(value: true);
		PlaceEquippableOnHand();
		m_managerArmWeaponPlayer.ShowCurrentArm();
	}

	public void TakeHit(NPC i_initiator)
	{
		if (!m_isCanBeAttacked || GetIsBeingRaped())
		{
			return;
		}
		_ = m_stateActorCurrent;
		_ = 3;
		bool flag = false;
		if (m_stateActorCurrent == StateActor.Ragdoll)
		{
			flag = true;
		}
		TakeKnockback(i_initiator);
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().Shake(i_initiator.GetDamageAttackCurrentTotal() / 50f, 0.25f);
		TakeDamage(i_initiator.GetDamageAttackCurrentTotal());
		DamageStrength(i_initiator.GetDamageAttackCurrentTotal() / 8f);
		ManagerDB.AddDamageTaken(i_initiator, (int)i_initiator.GetDamageAttackCurrentTotal());
		if (!flag && m_stateActorCurrent == StateActor.Ragdoll)
		{
			CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().AddStatusKO(i_initiator);
			if (i_initiator.GetId() != -1)
			{
				ManagerDB.KO(i_initiator);
			}
		}
		BecomeInvulnerableTakeHit();
		if ((bool)i_initiator)
		{
			PlayAudioSFX(i_initiator.GetAudioAttackHitAttackCurrent());
		}
		TryLoseRandomClothingPiece();
		if (m_stateActorCurrent == StateActor.Ragdoll)
		{
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectRagdoll();
		}
		m_animator.SetTrigger("Flinch");
		if (this.OnGetHit != null)
		{
			this.OnGetHit();
		}
	}

	public override void TakeDamage(float i_amount)
	{
		switch (ManagerDB.GetDifficulty())
		{
		case "Casual":
			i_amount *= 0.75f;
			break;
		case "Hard":
			i_amount *= 1.25f;
			break;
		}
		CommonReferences.Instance.GetManagerHud().TakeDamage();
		PlayAudioVoice(m_audiosVoiceGetHit);
		GetSkeletonPlayer().GetManagerFacePlayer().TakeDamage();
		m_healthCurrent -= i_amount;
		if (m_coroutineWaitBeforeRegenerateHealth != null)
		{
			StopCoroutine(m_coroutineWaitBeforeRegenerateHealth);
		}
		m_coroutineWaitBeforeRegenerateHealth = StartCoroutine(CoroutineWaitBeforeRegenerateHealth());
		if (m_stateActorCurrent != StateActor.Ragdoll)
		{
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectTakeHit(i_amount);
		}
		if (m_staminaCurrent < GetStat("HealthMax").GetValueTotal() / 4f)
		{
			_ = m_stateActorCurrent;
			_ = 4;
		}
		if (m_healthCurrent <= 0f)
		{
			m_healthCurrent = 0f;
			if (!GetIsBeingRaped())
			{
				Ragdoll(5f);
				BecomeInvulnerableTakeHit();
			}
		}
		this.OnTakeDamage?.Invoke();
	}

	public void TakeKnockback(NPC i_initiator)
	{
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		if (i_initiator.GetPos().x > base.transform.position.x)
		{
			velocity.x -= i_initiator.GetKnockbackXAttackCurrentTotal();
		}
		else
		{
			velocity.x += i_initiator.GetKnockbackXAttackCurrentTotal();
		}
		velocity.y = i_initiator.GetKnockbackYAttackCurrentTotal();
		GetRigidbody2D().velocity = velocity;
	}

	public void LoseHealth(int i_amount)
	{
		m_healthCurrent -= i_amount;
		if (m_healthCurrent <= 0f)
		{
			m_healthCurrent = 0f;
			Ragdoll(12f);
			BecomeInvulnerableTakeHit();
		}
	}

	private void TryLoseRandomClothingPiece()
	{
		if (Random.Range(0, 101) >= 60)
		{
			GetSkeletonPlayer().DropOrDestroyRandomClothingPiece();
		}
	}

	private void StartExhaustionGame()
	{
		if (m_statePlayerCurrent != StatePlayer.Labor)
		{
			EnableRagdoll();
			BecomeInvulnerable("Exhaustion Game Invulnerability Window", "ExhaustionGameWindow", 3f, i_isAffectSkeleton: true);
			SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
			CommonReferences.Instance.GetManagerHud().StartExhaustedState();
		}
	}

	private void InterruptExhaustionGame()
	{
		CommonReferences.Instance.GetManagerHud().InterruptExhaustedState();
	}

	public void WinExhaustionGame()
	{
		SetStateToIdleAndNone();
		this.OnEndExhaustion?.Invoke();
		DisableRagdoll();
		GetUp();
		BecomeInvulnerable("Exhaustion Game Win Invulnerability Window", "ExhaustionGameWinWindow", 2.5f, i_isAffectSkeleton: true);
		if (m_healthCurrent <= 1f)
		{
			m_healthCurrent = m_healthMax / 4f;
		}
	}

	public void RecoverFromRape()
	{
		if (m_numOfHeartsCurrent < 1 && !m_isDead)
		{
			Die();
		}
		if (m_isDead)
		{
			m_healthCurrent = 0f;
			this.OnRapeEnd?.Invoke();
			if (m_statePlayerCurrent != StatePlayer.Labor)
			{
				EnableRagdoll();
			}
		}
		else
		{
			this.OnRapeEnd?.Invoke();
			StartExhaustionGame();
		}
	}

	public void EscapeFromRape()
	{
		if (m_healthCurrent == 0f)
		{
			this.OnEndExhaustion?.Invoke();
		}
		SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		BecomeInvulnerable("Rape Escape Invulnerability Window", "RapeEscapeWindow", 1.5f, i_isAffectSkeleton: true);
		this.OnRapeEnd?.Invoke();
		if (m_healthCurrent <= 1f)
		{
			m_healthCurrent = m_healthMax / 4f;
		}
	}

	public void DestroyAHeart()
	{
		if (m_numOfHeartsCurrent > 0)
		{
			CommonReferences.Instance.GetManagerHud().DestroyAHeart();
			m_numOfHeartsCurrent--;
		}
		if (m_numOfHeartsCurrent < 1)
		{
			Die();
		}
	}

	public override void Die()
	{
		if (m_isDead)
		{
			return;
		}
		SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		if ((bool)GetRaperCurrent())
		{
			ManagerDB.MindBreak(GetRaperCurrent().GetNPC());
		}
		if (m_statePlayerCurrent != StatePlayer.BeingRaped)
		{
			m_isDead = true;
		}
		else if (!m_isDead)
		{
			if (this.OnDie != null)
			{
				this.OnDie();
			}
			CommonReferences.Instance.GetManagerScreens().GetScreenGame().SlowMoDeath();
			OnRapeEnd += HandleDeathAfterRapeEnd;
		}
	}

	private void HandleDeathAfterRapeEnd()
	{
		OnRapeEnd -= HandleDeathAfterRapeEnd;
		m_isDead = true;
		m_healthCurrent = 0f;
		DropAllPickupAbles();
		if (m_statePlayerCurrent != StatePlayer.Labor)
		{
			EnableRagdoll();
		}
		CommonReferences.Instance.GetManagerHud().GameOver();
		CommonReferences.Instance.GetManagerHud().DeadHud();
	}

	public override void DropPickupAble(PickUpable i_pickUpable)
	{
		if (i_pickUpable.GetIsCanDrop())
		{
			i_pickUpable.Drop(0.5f);
			CommonReferences.Instance.GetPlayerController().RemovePickupAbleFromInventory(i_pickUpable);
			if (GetEquippableEquipped() == i_pickUpable)
			{
				m_weaponEquipped = null;
			}
		}
	}

	public override void DropPickupAble(PickUpable i_pickUpable, float i_powerDrop01)
	{
		if (i_pickUpable.GetIsCanDrop())
		{
			i_pickUpable.Drop(i_powerDrop01);
			CommonReferences.Instance.GetPlayerController().RemovePickupAbleFromInventory(i_pickUpable);
			if (GetEquippableEquipped() == i_pickUpable)
			{
				m_weaponEquipped = null;
			}
		}
	}

	public void DropEquippedEquippable()
	{
		Weapon equippableEquipped = GetEquippableEquipped();
		if (!(equippableEquipped == null) && equippableEquipped.GetIsCanDrop())
		{
			equippableEquipped.Drop(0.25f);
			CommonReferences.Instance.GetPlayerController().RemovePickupAbleFromInventory(equippableEquipped);
			ClearDroppedEquippable(equippableEquipped);
			if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
			{
				CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
			}
		}
	}

	public void DropEquippedEquippableWithPlayerForce()
	{
		Weapon equippableEquipped = GetEquippableEquipped();
		if (!(equippableEquipped == null) && equippableEquipped.GetIsCanDrop())
		{
			equippableEquipped.Drop(GetVelocity());
			CommonReferences.Instance.GetPlayerController().RemovePickupAbleFromInventory(equippableEquipped);
			ClearDroppedEquippable(equippableEquipped);
		}
	}

	public void DropEquippedEquippable(float i_powerDrop01)
	{
		Weapon equippableEquipped = GetEquippableEquipped();
		if (!(equippableEquipped == null) && equippableEquipped.GetIsCanDrop())
		{
			equippableEquipped.Drop(i_powerDrop01);
			CommonReferences.Instance.GetPlayerController().RemovePickupAbleFromInventory(equippableEquipped);
			ClearDroppedEquippable(equippableEquipped);
		}
	}

	public void DropEquippedEquippable(Vector2 i_forceDrop)
	{
		Weapon equippableEquipped = GetEquippableEquipped();
		if (!(equippableEquipped == null) && equippableEquipped.GetIsCanDrop())
		{
			equippableEquipped.Drop(i_forceDrop);
			CommonReferences.Instance.GetPlayerController().RemovePickupAbleFromInventory(equippableEquipped);
			ClearDroppedEquippable(equippableEquipped);
		}
	}

	private void ClearDroppedEquippable(Weapon i_droppedEquippable)
	{
		if (m_weaponPrevious == i_droppedEquippable)
		{
			m_weaponPrevious = null;
		}
		if (m_weaponEquipped == i_droppedEquippable)
		{
			m_weaponEquipped = null;
		}
		else if (m_isHoldingGrapple)
		{
			m_isHoldingGrapple = false;
		}
	}

	public override void DropAllPickupAbles()
	{
		List<PickUpable> list = new List<PickUpable>();
		UnEquipEquippedWeapon();
		foreach (PickUpable allPickUpable in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllPickUpables())
		{
			list.Add(allPickUpable);
		}
		foreach (PickUpable item in list)
		{
			item.Drop(1f);
			CommonReferences.Instance.GetPlayerController().GetInventory().RemovePickUpable(item);
		}
	}

	public void UnEquipEquippedWeapon()
	{
		EquipWeapon(null);
	}

	public void ShowEquippedWeapon()
	{
		if ((bool)GetEquippableEquipped())
		{
			GetEquippableEquipped().Show();
		}
	}

	public void HideEquippedWeapon()
	{
		if ((bool)GetEquippableEquipped())
		{
			GetEquippableEquipped().Hide();
		}
	}

	private void BecomeInvulnerableDash(float i_secsDuration)
	{
		BecomeInvulnerable("Dash Invulnerability", "PlayerDash", i_secsDuration, i_isAffectSkeleton: true);
	}

	private void BecomeInvulnerableTakeHit()
	{
		BecomeInvulnerable("Invulnerable Window Take Hit", "TakeHitWindow", 0.2f, i_isAffectSkeleton: false);
	}

	public void GainLibido(float i_amount)
	{
		if (!(i_amount <= 0f))
		{
			m_libidoCurrent += i_amount;
			CommonReferences.Instance.GetManagerHud().GainLibido();
			if (m_libidoCurrent >= m_libidoMax)
			{
				m_libidoCurrent = m_libidoMax;
			}
		}
	}

	public void LoseLibido(float i_amount)
	{
		m_libidoCurrent -= i_amount;
		if (m_libidoCurrent <= 0f)
		{
			m_libidoCurrent = 0f;
		}
	}

	private IEnumerator CoroutineUpdateLibido()
	{
		float l_libidoLostSinceWhole = 0f;
		while (true)
		{
			float num = 1f;
			float valueTotal = GetStat("LibidoRegenRate").GetValueTotal();
			num *= 1f - valueTotal;
			float num2 = 0.05f;
			Timer timer = new Timer(0f);
			if (num >= num2)
			{
				timer.SetDurationAndResetTimer(num);
			}
			else
			{
				timer.SetDurationAndResetTimer(num2);
			}
			yield return timer.CoroutinePlayAndWaitForEnd();
			float valueTotal2 = GetStat("LibidoRegen").GetValueTotal();
			if (valueTotal2 != 0f)
			{
				if (valueTotal2 < 0f)
				{
					GainLibido(valueTotal2 * -1f);
				}
				else
				{
					LoseLibido(valueTotal2);
				}
				l_libidoLostSinceWhole += valueTotal2;
				if (l_libidoLostSinceWhole >= 1f)
				{
					CommonReferences.Instance.GetManagerHud().LoseLibido();
					l_libidoLostSinceWhole = 0f;
				}
				if (m_libidoCurrent < 0f)
				{
					m_libidoCurrent = 0f;
				}
				if (m_libidoCurrent > m_libidoMax)
				{
					m_libidoCurrent = m_libidoMax;
				}
			}
		}
	}

	public void GainPleasure(float i_amount)
	{
		float num = i_amount / 100f * (m_libidoCurrent * 24f);
		num /= 2f;
		if (!(num < 0f))
		{
			m_pleasureCurrent += num;
			CommonReferences.Instance.GetManagerHud().GainPleasure();
			if (m_pleasureCurrent >= m_pleasureMax)
			{
				m_pleasureCurrent = m_pleasureMax;
				Orgasm();
			}
		}
	}

	public void GainPleasureFlat(float i_amount)
	{
		if (!(i_amount < 0f))
		{
			m_pleasureCurrent += i_amount;
			CommonReferences.Instance.GetManagerHud().GainPleasure();
			if (m_pleasureCurrent >= m_pleasureMax)
			{
				m_pleasureCurrent = m_pleasureMax;
				Orgasm();
			}
		}
	}

	public void LosePleasure(float i_amount)
	{
		m_pleasureCurrent -= i_amount;
		CommonReferences.Instance.GetManagerHud().GainPleasure();
		if (m_pleasureCurrent <= 0f)
		{
			m_pleasureCurrent = 0f;
		}
	}

	public void Orgasm()
	{
		DestroyAHeart();
		PlayAudio(m_audiosCum);
		CommonReferences.Instance.GetManagerHud().PlayerCum();
		m_pleasureCurrent = 0f;
		GetSkeletonPlayer().GetManagerFacePlayer().Orgasm();
		m_particleOrgasm.Play();
		PlayAudioSFX(m_audioOrgasm);
		PlayAudioVoice(m_audiosVoiceOrgasm);
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectOrgasm();
		RemoveAllBuffs();
		if (this.OnOrgasm != null)
		{
			this.OnOrgasm();
		}
		if ((bool)GetRaperCurrent() && GetRaperCurrent().GetNPC().GetId() != -1)
		{
			ManagerDB.Orgasm(GetRaperCurrent().GetNPC());
		}
	}

	private void RemoveAllBuffs()
	{
		List<StatusEffect> statusEffects = GetStatusEffects();
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i].GetTypeStatusEffect() == TypeStatusEffect.Positive)
			{
				statusEffects[i].End();
				CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus(statusEffects[i].GetName() + " lost!", "You lost " + statusEffects[i].GetName() + " effect after orgasm!", StatusPlayerHudItemColor.Rape, 15f);
			}
		}
	}

	public void AudioPlayRandomFootstep()
	{
		PlayAudio(m_audiosFootStep);
	}

	public void Spawn()
	{
		Enable();
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(base.gameObject);
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetSmoothLevel(SmoothLevel.Medium);
		m_healthCurrent = GetStat("HealthMax").GetValueTotal();
		SetIsFacingLeft(i_isFacingLeft: false);
		m_isDead = false;
		m_isCanAttack = true;
		m_isAttacking = false;
		m_isCanBeAttacked = true;
		m_isCanMove = true;
		SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: true);
		SetStateToIdleAndNone();
		m_libidoCurrent = 0f;
		m_pleasureCurrent = 0f;
		m_strengthCurrent = m_strengthMax;
		m_numOfHeartsCurrent = m_numOfHeartsMax;
		CommonReferences.Instance.GetManagerHud().Retry();
		GetRigidbody2D().bodyType = RigidbodyType2D.Dynamic;
		SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		EquipEquippedClothes();
		GetSkeletonPlayer().SetSkinColor(ManagerDB.GetSkinColorPlayer());
		GetSkeletonPlayer().SetEyeColor(ManagerDB.GetEyeColorPlayer());
	}

	private void EquipEquippedClothes()
	{
		foreach (int idsEquippedClothe in ManagerDB.GetIdsEquippedClothes())
		{
			GetSkeletonPlayer().EquipClothing(Library.Instance.Clothes.GetClothing(idsEquippedClothe));
		}
	}

	public void EnterStage()
	{
		if (CommonReferences.Instance.GetManagerStages().GetStageCurrent() is StageHub)
		{
			if (CommonReferences.Instance.GetManagerScreens().GetScreenGame().GetIsFirstTimeSpawn())
			{
				EnterHubFirstTime();
			}
			else
			{
				RespawnHub();
			}
		}
		else
		{
			Gun gun = Object.Instantiate(Library.Instance.Guns.GetGun(SpawnGun), base.transform.parent);
			PickUp(gun, i_isDuplicate: false);
			EquipWeapon(gun);
		}
	}

	public void EnterHubFirstTime()
	{
		PlaceInBed(i_isDied: true);
	}

	public void RespawnHub()
	{
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetSmoothLevel(SmoothLevel.Low);
		PlaceInBed(i_isDied: true);
	}

	private void PlaceInBed(bool i_isDied)
	{
		StageHub stageHub = (StageHub)CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		SetPosX(stageHub.GetPosBed().x);
		if (!i_isDied)
		{
			SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		}
		if (m_coroutineWakeUp != null)
		{
			StopCoroutine(m_coroutineWakeUp);
		}
		m_isWakingUp = true;
		SetIsCanSwitchFacingSide(i_isCanSwitchFacingSide: false);
		m_coroutineWakeUp = StartCoroutine(CoroutineWakeUp(i_isDied));
		if (i_isDied)
		{
			StartCoroutine(CoroutineListenToInputGetUpCancel());
		}
	}

	private IEnumerator CoroutineWakeUp(bool i_isDied)
	{
		if (i_isDied)
		{
			GetAnimator().Play("GetUpBedGameOver");
			GetSkeletonPlayer().GetManagerFacePlayer().WakeUpGameOver();
		}
		else
		{
			GetAnimator().Play("GetUpBed");
			GetSkeletonPlayer().GetManagerFacePlayer().WakeUp();
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		SetIsFacingLeft(i_isFacingLeft: false);
		yield return new WaitForSeconds(GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.length);
		SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		m_isWakingUp = false;
		SetIsCanSwitchFacingSide(i_isCanSwitchFacingSide: true);
		m_coroutineWakeUp = null;
	}

	private IEnumerator CoroutineListenToInputGetUpCancel()
	{
		while (m_isWakingUp)
		{
			yield return new WaitForEndOfFrame();
			if (Input.anyKey)
			{
				StopCoroutine(m_coroutineWakeUp);
				m_coroutineWakeUp = null;
				SetIsCanSwitchFacingSide(i_isCanSwitchFacingSide: true);
				SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
				m_animator.Play("GetUpBed", 0, 0.78f);
				GetSkeletonPlayer().GetManagerFacePlayer().GetAnimatorFace().Play("Idle");
				yield return new WaitForSeconds(1.5f);
				SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
				m_isWakingUp = false;
			}
		}
	}

	public override void PickUp(PickUpable i_pickUpable, bool i_isDuplicate)
	{
		PickUpable pickUpable = null;
		pickUpable = ((!i_isDuplicate) ? i_pickUpable : Object.Instantiate(i_pickUpable));
		pickUpable.PickUp(this);
		CommonReferences.Instance.GetPlayerController().GetInventory().AddItem(pickUpable);
		if ((bool)pickUpable.GetComponent<Rigidbody2D>())
		{
			pickUpable.GetComponent<Rigidbody2D>().isKinematic = true;
		}
		Collider2D[] components = pickUpable.GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
		CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotificationPickUp(pickUpable);
		if (this.OnPickUp != null)
		{
			this.OnPickUp(pickUpable);
		}
	}

	public void PlaceEquippableOnHand()
	{
		Weapon equippableEquipped = GetEquippableEquipped();
		if (equippableEquipped == null)
		{
			return;
		}
		equippableEquipped.transform.SetParent(m_managerArmWeaponPlayer.GetPointHoldWeapon().transform);
		equippableEquipped.transform.position = m_managerArmWeaponPlayer.GetPosHoldWeapon();
		if (equippableEquipped is Usable)
		{
			if (((Usable)equippableEquipped).GetUsableType() == UsableType.Syringe)
			{
				equippableEquipped.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
			}
			else
			{
				equippableEquipped.transform.localEulerAngles = Vector3.zero;
			}
		}
		else
		{
			equippableEquipped.transform.localEulerAngles = Vector3.zero;
		}
		equippableEquipped.transform.localScale = new Vector3(1f, 1f, 1f);
		GetSkeletonPlayer().PlaceEquippableOnHand(equippableEquipped);
	}

	public void PickUpTry()
	{
		foreach (PickUpable allItem in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllItems())
		{
			if (!(allItem is Consumable) && allItem.GetIsPickUpable() && !allItem.IsPickedUp() && allItem.isActiveAndEnabled && Vector2.Distance(allItem.transform.position, base.transform.position) <= m_rangePickUp)
			{
				if (!CommonReferences.Instance.GetPlayerController().GetInventory().IsHasRoomForPickUpable(allItem))
				{
					CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Too heavy (weight weapon: " + allItem.GetWeight() + ", room left: " + CommonReferences.Instance.GetPlayerController().GetInventory().GetRoomLeft() + ")", ColorTextNotification.Other, i_isContinues: false);
				}
				else
				{
					PickUp(allItem, i_isDuplicate: false);
				}
				break;
			}
		}
	}

	public float GetRangePickUp()
	{
		return m_rangePickUp;
	}

	public void InteractTry()
	{
		List<Interactable> allInteractables = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllInteractables();
		List<Interactable> list = new List<Interactable>();
		float num = 2.25f;
		foreach (Interactable item in allInteractables)
		{
			if (item.GetIsCanBeUsedToActivate() && (Vector2.Distance(item.transform.position, GetPosFeet()) <= num || Vector2.Distance(item.transform.position, GetPosTopHead()) <= num))
			{
				list.Add(item);
			}
		}
		if (list.Count < 1)
		{
			return;
		}
		Interactable interactable = list[0];
		foreach (Interactable item2 in list)
		{
			foreach (Interactable item3 in list)
			{
				if (!(item2 == item3))
				{
					float num2 = Vector2.Distance(item2.transform.position, GetPosFeet()) + Vector2.Distance(item2.transform.position, GetPosTopHead());
					if (Vector2.Distance(item3.transform.position, GetPosFeet()) + Vector2.Distance(item3.transform.position, GetPosTopHead()) < num2)
					{
						interactable = item3;
					}
				}
			}
		}
		this.OnUse?.Invoke(interactable);
		interactable.Use();
	}

	public void StartUseEquippedUsable()
	{
		switch (((Usable)GetEquippableEquipped()).GetUsableType())
		{
		case UsableType.Syringe:
			m_animator.Play("UseSyringe");
			break;
		case UsableType.Pills:
			m_animator.Play("UsePills");
			break;
		}
		m_isUsingUsable = true;
		if (m_coroutineStartUseEquippedUsable != null)
		{
			StopCoroutine(m_coroutineStartUseEquippedUsable);
		}
		m_coroutineStartUseEquippedUsable = StartCoroutine(CoroutineStartUseEquippedUsable());
	}

	private IEnumerator CoroutineStartUseEquippedUsable()
	{
		yield return new WaitForSeconds(1f);
		m_isUsingUsable = false;
	}

	private void InterruptStartUseEquippedUsable()
	{
		if (m_coroutineStartUseEquippedUsable != null)
		{
			StopCoroutine(m_coroutineStartUseEquippedUsable);
		}
		m_isUsingUsable = false;
	}

	public void UseEquippedUsable()
	{
		((Usable)GetEquippableEquipped()).Use(i_isAltFire: false);
	}

	public bool GetIsUsingUsable()
	{
		return m_isUsingUsable;
	}

	public bool GetIsCanDoActionsOtherThanMovement()
	{
		if (m_isUsingUsable || m_isReloading || !m_isThinking || m_isEquipping || m_isDead)
		{
			return false;
		}
		return true;
	}

	public void CreateAndAddFetus(NPC i_npcParent, Actor i_actorToMakeFetus, float i_durationPregnant, string i_nameFetus)
	{
		Fetus fetus = base.gameObject.AddComponent<Fetus>();
		Actor i_actorInFetus = Object.Instantiate(i_actorToMakeFetus, base.transform);
		fetus.InitializeFetus(i_npcParent, i_actorInFetus, i_durationPregnant, i_nameFetus);
		InsertFetus(fetus);
	}

	public void CreateAndAddFetus(NPC i_npcParent, List<Actor> i_actorsToMakeFetusses, float i_durationPregnantPerFetus, string i_nameFetus)
	{
		foreach (Actor i_actorsToMakeFetuss in i_actorsToMakeFetusses)
		{
			Fetus fetus = base.gameObject.AddComponent<Fetus>();
			Actor i_actorInFetus = Object.Instantiate(i_actorsToMakeFetuss, base.transform);
			fetus.InitializeFetus(i_npcParent, i_actorInFetus, i_durationPregnantPerFetus, i_nameFetus);
			InsertFetus(fetus);
		}
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Effect5"));
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlayPopup(new Color(0.56f, 0.36f, 0.36f, 0.72f), i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: true, 0.15f, 0f, 0.75f, 1f, 0.75f, 0f, 0f);
	}

	public void CreateAndAddFetusWhole(NPC i_npcParent, List<Actor> i_actorsToMakeFetusWhole, float i_durationPregnant, string i_nameFetus)
	{
		List<Actor> list = new List<Actor>();
		foreach (Actor item in i_actorsToMakeFetusWhole)
		{
			list.Add(Object.Instantiate(item));
		}
		Fetus fetus = base.gameObject.AddComponent<Fetus>();
		fetus.InitializeFetus(list, i_durationPregnant, i_nameFetus);
		InsertFetus(fetus);
	}

	public void AddEgg(NPC i_npcParent, Egg i_egg, float i_durationPregnant, string i_nameFetus)
	{
		Fetus fetus = base.gameObject.AddComponent<Fetus>();
		fetus.InitializeFetus(i_npcParent, i_egg, i_durationPregnant, i_nameFetus);
		InsertFetus(fetus);
	}

	private void InsertFetus(Fetus i_fetus)
	{
		i_fetus.StartTimerPregnant();
		m_fetuses.Add(i_fetus);
		CommonReferences.Instance.GetManagerHud().GetManagerFetusHud().AddFetus(i_fetus);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Effect5"));
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlayPopup(new Color(0.56f, 0.36f, 0.36f, 0.72f), i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: true, 0.15f, 0f, 0.75f, 1f, 0.75f, 0f, 0f);
		this.OnFetusInsert?.Invoke(i_fetus);
		if ((bool)i_fetus.GetNpcParent() && i_fetus.GetNpcParent().GetId() != -1)
		{
			ManagerDB.AddFetusCount(i_fetus.GetNpcParent());
		}
	}

	public void StartLabor(Fetus i_fetus)
	{
		if (this.OnLabor != null)
		{
			this.OnLabor();
		}
		SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: true);
		if (m_stateActorCurrent == StateActor.Ragdoll)
		{
			DisableRagdoll();
		}
		if (m_coroutineReload != null)
		{
			InterruptReload();
		}
		if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
		{
			CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
		}
		InterruptExhaustionGame();
		InterruptClimb();
		SetIsCrouching(i_isCrouching: false);
		PlayAudioVoice(m_audiosVoiceLaborStart);
		if (i_fetus.GetActorsInFetus()[0] is Egg)
		{
			StartCoroutine(CoroutineStartLaborEgg());
		}
		else
		{
			StartCoroutine(CoroutineStartLabor());
		}
	}

	private IEnumerator CoroutineStartLabor()
	{
		m_statePlayerCurrent = StatePlayer.Labor;
		m_isInvulnerable = true;
		m_isCanBeAttacked = false;
		SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_isThinking = false;
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetFOVOriginal() / 2f, 0.5f);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioLaborStart);
		GameObject l_overlayLabor = CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(Color.black, i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: false, 0.5f, 0f, 0.35f);
		Timer l_timer = new Timer(1f);
		m_animator.Play("LaborStart");
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		l_timer.SetDuration(5f);
		l_timer.Reset();
		m_animator.Play("LaborLoop1");
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		l_timer.SetDuration(1.5f);
		l_timer.Reset();
		m_animator.Play("LaborTransition");
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		l_timer.SetDuration(5f);
		l_timer.Reset();
		m_animator.Play("LaborLoop2");
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		bool flag = false;
		while (!flag)
		{
			l_timer.SetDuration(1f);
			l_timer.Reset();
			m_animator.Play("Birth", 0, 0f);
			yield return l_timer.CoroutinePlayAndWaitForEnd();
			GiveBirth(i_isEgg: false);
			flag = true;
			if (GetFetusesReadyNonEgg().Count > 0)
			{
				flag = false;
			}
		}
		if (this.OnLaborEnd != null)
		{
			this.OnLaborEnd();
		}
		l_timer.SetDuration(3.5f);
		l_timer.Reset();
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(l_overlayLabor, i_isDuplicateOverlay: false, i_isDestroyOverlayAfterAnimation: true, 1f, 0.35f, 0f);
		EndBirth();
	}

	private IEnumerator CoroutineStartLaborEgg()
	{
		m_statePlayerCurrent = StatePlayer.Labor;
		m_isInvulnerable = true;
		m_isCanBeAttacked = false;
		SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_isThinking = false;
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetFOVOriginal() / 2f, 0.5f);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioLaborStart);
		GameObject l_overlayLabor = CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(Color.black, i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: false, 0.5f, 0f, 0.35f);
		Timer l_timer = new Timer(6f);
		m_animator.Play("LaborEgg");
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		bool flag = false;
		while (!flag)
		{
			l_timer.SetDuration(0.916f);
			l_timer.Reset();
			m_animator.Play("BirthEgg", 0, 0f);
			yield return l_timer.CoroutinePlayAndWaitForEnd();
			GiveBirth(i_isEgg: true);
			flag = true;
			if (GetFetusesReadyEgg().Count > 0)
			{
				flag = false;
			}
		}
		if (this.OnLaborEnd != null)
		{
			this.OnLaborEnd();
		}
		l_timer.SetDuration(1.166f);
		l_timer.Reset();
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(l_overlayLabor, i_isDuplicateOverlay: false, i_isDestroyOverlayAfterAnimation: true, 1f, 0.35f, 0f);
		EndBirth();
	}

	private void GiveBirth(bool i_isEgg)
	{
		Fetus fetus = null;
		try
		{
			fetus = ((!i_isEgg) ? GetFetusesReadyNonEgg()[0] : GetFetusesReadyEgg()[0]);
		}
		catch
		{
			Debug.Log("Poiw");
			fetus = GetFetusesReadyNonEgg()[0];
		}
		foreach (Actor actorsInFetu in fetus.GetActorsInFetus())
		{
			actorsInFetu.SetPos(GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform.position);
			actorsInFetu.SetStateActor(StateActor.Idle);
			actorsInFetu.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
				.transform);
				actorsInFetu.gameObject.SetActive(value: true);
				Vector2 zero = Vector2.zero;
				if (GetIsFacingLeft())
				{
					zero.x = Random.Range(-1f, -5f);
				}
				else
				{
					zero.x = Random.Range(1f, 5f);
				}
				zero.y = Random.Range(6f, 10f);
				actorsInFetu.SetVelocity(zero);
				actorsInFetu.DisableThinking(2f);
				if (Random.Range(0, 2) == 0)
				{
					actorsInFetu.SetIsFacingLeft(i_isFacingLeft: false);
				}
				else
				{
					actorsInFetu.SetIsFacingLeft(i_isFacingLeft: true);
				}
				if (actorsInFetu is NPC)
				{
					NPC nPC = (NPC)actorsInFetu;
					nPC.SetIsPlayerParent(i_isPlayerParent: true);
					if (!(actorsInFetu is Egg))
					{
						nPC.Spawn(i_isFadeIn: false);
					}
				}
				ManagerDB.Birth(fetus, (NPC)actorsInFetu);
				this.OnBirth?.Invoke(actorsInFetu);
			}
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioBirth);
			PlayAudioVoice(m_audiosVoiceBirth);
			m_particleBirth.Play();
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectBirth();
			m_fetuses.Remove(fetus);
			Object.Destroy(fetus);
		}

		private void EndBirth()
		{
			if (!m_isDead)
			{
				m_managerArmWeaponPlayer.ShowCurrentArm();
				SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
				m_isThinking = true;
			}
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetFOVOriginal(), 0.5f);
			if (m_healthCurrent == 0f && !m_isDead)
			{
				m_isInvulnerable = false;
				m_isCanBeAttacked = true;
				StartExhaustionGame();
			}
			else
			{
				SetStateToIdleAndNone();
				BecomeInvulnerable("Birth End Invulnerability Window", "BirthEndWindow", 2f, i_isAffectSkeleton: true);
			}
			if (m_isDead)
			{
				EnableRagdoll();
			}
			else
			{
				m_animator.Play("Idle");
			}
			if (this.OnBirthEnd != null)
			{
				this.OnBirthEnd();
			}
		}

		private List<Fetus> GetFetusesReadyNonEgg()
		{
			List<Fetus> fetusesReady = GetFetusesReady();
			List<Fetus> list = new List<Fetus>();
			for (int i = 0; i < fetusesReady.Count; i++)
			{
				if (!(fetusesReady[i].GetActorsInFetus()[0] is Egg))
				{
					list.Add(fetusesReady[i]);
				}
			}
			return list;
		}

		private List<Fetus> GetFetusesReadyEgg()
		{
			List<Fetus> fetusesReady = GetFetusesReady();
			List<Fetus> list = new List<Fetus>();
			for (int i = 0; i < fetusesReady.Count; i++)
			{
				if (fetusesReady[i].GetActorsInFetus()[0] is Egg)
				{
					list.Add(fetusesReady[i]);
				}
			}
			return list;
		}

		private List<Fetus> GetFetusesReady()
		{
			List<Fetus> list = new List<Fetus>();
			for (int i = 0; i < m_fetuses.Count; i++)
			{
				if (m_fetuses[i].IsReadyToBirth())
				{
					list.Add(m_fetuses[i]);
				}
			}
			return list;
		}

		public override void Ragdoll(float i_secsDuration)
		{
			if (m_stateActorCurrent != StateActor.Ragdoll)
			{
				m_stateActorCurrent = StateActor.Ragdoll;
				SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: true);
				InterruptReload();
				InterruptClimb();
				if (m_coroutineRagdoll != null)
				{
					StopCoroutine(m_coroutineRagdoll);
				}
				if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
				{
					CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
				}
				m_coroutineRagdoll = StartCoroutine(CoroutineRagdoll(i_secsDuration));
				PlayAudioSFX(m_audioRagdoll);
			}
		}

		private IEnumerator CoroutineRagdoll(float i_secsDuration)
		{
			EnableRagdoll();
			SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
			this.OnRagdoll?.Invoke();
			yield return new WaitForSeconds(i_secsDuration);
			DisableRagdoll();
			GetUp();
			this.OnRagdollEnd?.Invoke();
		}

		public void InterruptRagdoll()
		{
			if (m_coroutineRagdoll != null)
			{
				StopCoroutine(m_coroutineRagdoll);
			}
			DisableRagdoll();
			GetUp();
			this.OnRagdollEnd?.Invoke();
		}

		private void GetUp()
		{
			if (m_coroutineGetUp != null)
			{
				StopCoroutine(m_coroutineGetUp);
			}
			m_coroutineGetUp = StartCoroutine(CoroutineGetUp());
		}

		private IEnumerator CoroutineGetUp()
		{
			m_animator.Play("GetUpFaceDown");
			yield return new WaitForSeconds(1f);
			SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
			SetStateToIdleAndNone();
			if (!GetIsBeingRaped() && m_healthCurrent <= 1f)
			{
				m_healthCurrent = m_healthMax / 4f;
			}
		}

		public void EnableRagdoll()
		{
			m_stateActorCurrent = StateActor.Ragdoll;
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).gameObject);
			GetSkeletonPlayer().EnableRagdoll();
			GetSkeletonPlayer().GetManagerFacePlayer().KO();
		}

		public void DisableRagdoll()
		{
			if (m_coroutineRagdoll != null)
			{
				StopCoroutine(m_coroutineRagdoll);
			}
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(base.gameObject);
			HandleSkeletonPositionRagdollEnd();
			GetSkeletonPlayer().DisableRagdoll();
			GetSkeletonPlayer().GetManagerFacePlayer().UnKO();
		}

		private void HandleSkeletonPositionRagdollEnd()
		{
			int mask = LayerMask.GetMask("Platform");
			RaycastHit2D raycastHit2D = Physics2D.Raycast(GetPosHips(), Vector2.down, GetHeight(), mask);
			if ((bool)raycastHit2D)
			{
				PlaceFeetOnPos(raycastHit2D.point);
			}
			else
			{
				SetPos(new Vector2(((Vector3)GetPosHips()).x, GetPos().y));
			}
		}

		public int GetNumOfFetuses()
		{
			return m_fetuses.Count;
		}

		public void SetIsForceIgnoreInput(bool i_isForceIgnoreInput)
		{
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput);
		}

		public void SetName(string i_name)
		{
			m_name = i_name;
		}

		public void RestoreStamina(float i_amount)
		{
			m_staminaCurrent += i_amount;
			if (m_staminaCurrent > m_healthCurrent)
			{
				m_staminaCurrent = m_healthCurrent;
			}
		}

		public void RestoreStaminaFully()
		{
			m_staminaCurrent = m_healthCurrent;
		}

		public float GetStaminaCurrent()
		{
			return m_staminaCurrent;
		}

		public float GetLibidoMax()
		{
			return m_libidoMax;
		}

		public float GetPleasureMax()
		{
			return m_pleasureMax;
		}

		public float GetLibidoCurrent()
		{
			return m_libidoCurrent;
		}

		public float GetPleasureCurrent()
		{
			return m_pleasureCurrent;
		}

		public float GetStrengthCurrent()
		{
			return m_strengthCurrent;
		}

		public float GetStrengthMax()
		{
			return m_strengthMax;
		}

		public int GetNumOfHeartsCurrent()
		{
			return m_numOfHeartsCurrent;
		}

		public bool GetIsCanBeRaped()
		{
			if (!m_isInvulnerable && m_statePlayerCurrent != StatePlayer.BeingRaped && m_statePlayerCurrent != StatePlayer.Labor)
			{
				return true;
			}
			return false;
		}

		public bool GetIsBeingRaped()
		{
			if (m_statePlayerCurrent == StatePlayer.BeingRaped || GetRaperCurrent() != null)
			{
				return true;
			}
			return false;
		}

		public void SetIsBeingRaped(bool i_isBeingRaped)
		{
			if (i_isBeingRaped)
			{
				if (m_stateActorCurrent == StateActor.Ragdoll)
				{
					DisableRagdoll();
					if (m_coroutineGetUp != null)
					{
						StopCoroutine(m_coroutineGetUp);
					}
					InterruptExhaustionGame();
				}
				if (m_isDead)
				{
					DisableRagdoll();
				}
				if (m_coroutineClimb != null)
				{
					InterruptClimb();
				}
				if (m_coroutineStartUseEquippedUsable != null)
				{
					InterruptStartUseEquippedUsable();
				}
				if (IsOnFire())
				{
					Extinguish();
				}
				SetIsExposing(i_isExposing: false);
				m_statePlayerCurrent = StatePlayer.BeingRaped;
				if (this.OnBeingRaped != null)
				{
					this.OnBeingRaped();
				}
			}
			else
			{
				m_raperCurrent = null;
				if (m_numOfHeartsCurrent == 0)
				{
					m_isDead = true;
				}
				if (m_isDead)
				{
					m_statePlayerCurrent = StatePlayer.Dead;
				}
				else
				{
					SetStateToIdleAndNone();
				}
			}
		}

		public void SetRaperRaping(Raper i_raper)
		{
			m_raperCurrent = i_raper;
		}

		public Raper GetRaperCurrent()
		{
			return m_raperCurrent;
		}

		public StatePlayer GetStatePlayerCurrent()
		{
			return m_statePlayerCurrent;
		}

		public void SetStatePlayer(StatePlayer i_statePlayer)
		{
			m_statePlayerCurrent = i_statePlayer;
		}

		public bool GetIsSprinting()
		{
			return m_isSprinting;
		}

		public Weapon GetEquippableEquipped()
		{
			if (m_isHoldingGrapple)
			{
				return (Weapon)CommonReferences.Instance.GetPlayerController().GetInventory().GetPickUpableByName("Grappling Hook");
			}
			return m_weaponEquipped;
		}

		public bool GetIsHealthLowEnoughTeGetRaped()
		{
			if (m_healthCurrent < 1f)
			{
				return true;
			}
			return false;
		}

		public bool GetIsCanInteractWith()
		{
			if (m_statePlayerCurrent == StatePlayer.BeingRaped || m_statePlayerCurrent == StatePlayer.Labor)
			{
				return false;
			}
			return true;
		}

		public bool GetIsCanSwitchFacingSide()
		{
			return m_isCanSwitchFacingSide;
		}

		public void SetIsCanSwitchFacingSide(bool i_isCanSwitchFacingSide)
		{
			m_isCanSwitchFacingSide = i_isCanSwitchFacingSide;
		}

		public override Vector2 GetPosFeet()
		{
			return new Vector2(GetPosHips().x, m_colliderFeet.bounds.center.y - m_colliderFeet.bounds.size.y / 2f);
		}

		public override Vector2 GetPosTopHead()
		{
			return new Vector2(GetPosHips().x, m_playerCollisionHandler.m_collidersStanding[0].bounds.center.y + m_playerCollisionHandler.m_collidersStanding[0].bounds.size.y / 2f);
		}

		public SkeletonPlayer GetSkeletonPlayer()
		{
			return (SkeletonPlayer)GetSkeleton();
		}

		public override float GetHeight()
		{
			return m_playerCollisionHandler.m_collidersStanding[0].bounds.size.y;
		}

		public override Vector2 GetPosHips()
		{
			return GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform.position;
		}

		public bool GetIsReloading()
		{
			return m_isReloading;
		}

		public void SetIsSprinting(bool i_isSprinting)
		{
			m_isSprinting = i_isSprinting;
		}

		public bool GetIsWalkingBackwards()
		{
			return m_isWalkingBackwards;
		}

		public bool GetIsWalkingLeft()
		{
			if (GetIsWalkingBackwards())
			{
				if (GetIsFacingLeft())
				{
					return false;
				}
				return true;
			}
			if (GetIsFacingLeft())
			{
				return true;
			}
			return false;
		}

		public string GetDescriptionPlayer()
		{
			return m_descriptionPlayer;
		}

		public new string GetSex()
		{
			return m_sex;
		}

		public int GetAge()
		{
			return m_age;
		}

		public bool GetIsEquipping()
		{
			return m_isEquipping;
		}

		private void SetStateToIdleAndNone()
		{
			m_stateActorCurrent = StateActor.Idle;
			m_statePlayerCurrent = StatePlayer.None;
		}

		public Stat GetStat(StatNamePlayer i_nameStat)
		{
			foreach (Stat stat in m_stats)
			{
				if (stat.GetName() == i_nameStat.ToString())
				{
					return stat;
				}
			}
			return null;
		}

		public override void PlayAudioVoice(AudioClip i_audio)
		{
			if (!m_isMute)
			{
				base.PlayAudioVoice(i_audio);
				if (m_libidoCurrent > m_libidoMax / 2f)
				{
					m_particleLibidoSpeak.Play();
				}
			}
		}

		public override void PlayAudioVoice(List<AudioClip> i_audiosToChooseFrom)
		{
			if (!m_isMute)
			{
				base.PlayAudioVoice(i_audiosToChooseFrom);
				if (m_libidoCurrent > m_libidoMax / 2f)
				{
					m_particleLibidoSpeak.Play();
				}
			}
		}

		public GameObject GetPosMiddleFace()
		{
			return m_posMiddleFace;
		}

		public void EnableHypnotized()
		{
			int layerIndex = GetSkeletonPlayer().GetManagerFacePlayer().GetAnimatorFace().GetLayerIndex("Hypnotized");
			GetSkeletonPlayer().GetManagerFacePlayer().GetAnimatorFace().SetLayerWeight(layerIndex, 1f);
			m_isHypnotized = true;
		}

		public void DisableHypnotized()
		{
			int layerIndex = GetSkeletonPlayer().GetManagerFacePlayer().GetAnimatorFace().GetLayerIndex("Hypnotized");
			GetSkeletonPlayer().GetManagerFacePlayer().GetAnimatorFace().SetLayerWeight(layerIndex, 0f);
			m_isHypnotized = false;
		}

		public bool IsHypnotized()
		{
			return m_isHypnotized;
		}

		public void SetIsCrouching(bool i_isCrouching)
		{
			if (i_isCrouching != m_isCrouching)
			{
				m_isCrouching = i_isCrouching;
				if (m_coroutineSetIsCrouching != null)
				{
					StopCoroutine(m_coroutineSetIsCrouching);
				}
				m_coroutineSetIsCrouching = StartCoroutine(CoroutineSetIsCrouching(i_isCrouching));
			}
		}

		private IEnumerator CoroutineSetIsCrouching(bool i_isCrouching)
		{
			float l_weightFrom = m_animator.GetLayerWeight(m_animator.GetLayerIndex("Crouch"));
			float l_weightTo = (i_isCrouching ? 1 : 0);
			float l_timeToMove = 0.25f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
				m_animator.SetLayerWeight(m_animator.GetLayerIndex("Crouch"), weight);
				yield return new WaitForFixedUpdate();
			}
			m_animator.SetLayerWeight(m_animator.GetLayerIndex("Crouch"), l_weightTo);
		}

		public void SetIsExposing(bool i_isExposing)
		{
			if (m_isExposing == i_isExposing)
			{
				return;
			}
			m_isExposing = i_isExposing;
			if (i_isExposing)
			{
				HideEquippedWeapon();
				if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
				{
					CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
				}
				InterruptReload();
			}
			else
			{
				ShowEquippedWeapon();
			}
			if (m_coroutineSetIsExposing != null)
			{
				StopCoroutine(m_coroutineSetIsExposing);
			}
			m_coroutineSetIsExposing = StartCoroutine(CoroutineSetIsExposing(i_isExposing));
		}

		private IEnumerator CoroutineSetIsExposing(bool i_isExposing)
		{
			int l_layerIndex = m_animator.GetLayerIndex("Expose");
			if (l_layerIndex < 0)
			{
				yield break;
			}
			float l_weightFrom = m_animator.GetLayerWeight(l_layerIndex);
			float l_weightTo = (i_isExposing ? 1 : 0);
			float l_timeToMove = (i_isExposing ? 0.5f : 0.25f);
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
				m_animator.SetLayerWeight(l_layerIndex, weight);
				yield return new WaitForFixedUpdate();
			}
			m_animator.SetLayerWeight(l_layerIndex, l_weightTo);
		}

		public bool IsCrouching()
		{
			return m_isCrouching;
		}

		public bool IsExposing()
		{
			return m_isExposing;
		}

		public void KillNpc(NPC i_npcKilled)
		{
			ManagerDB.Kill(i_npcKilled);
			this.OnKill?.Invoke(i_npcKilled);
		}

		public void Fear(float i_duration)
		{
			InterruptClimb();
			InterruptReload();
			InterruptStartUseEquippedUsable();
			m_isFear = true;
			SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
			StopMovingHorizontally();
			m_animator.Play("Fear");
			m_animator.SetBool("IsFear", value: true);
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectFear(i_duration);
			StartCoroutine(CoroutineFear(i_duration));
		}

		private IEnumerator CoroutineFear(float i_duration)
		{
			yield return new WaitForSeconds(i_duration);
			SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
			m_animator.SetBool("IsFear", value: false);
			m_isFear = false;
		}

		public override bool ApplyStatusEffect(StatusEffect i_statusEffect)
		{
			bool num = base.ApplyStatusEffect(i_statusEffect);
			if (!num)
			{
				CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Cannot apply drug again", ColorTextNotification.Usable, i_isContinues: false);
			}
			return num;
		}

		public void EnableStrengthDrain()
		{
			if (m_coroutineStrengthDrain != null)
			{
				StopCoroutine(m_coroutineStrengthDrain);
			}
			m_coroutineStrengthDrain = StartCoroutine(CoroutineStrengthDrain());
		}

		public void DisableStrengthDrain()
		{
			if (m_coroutineStrengthDrain != null)
			{
				StopCoroutine(m_coroutineStrengthDrain);
				m_coroutineStrengthDrain = null;
			}
		}

		private IEnumerator CoroutineStrengthDrain()
		{
			while (true)
			{
				yield return new WaitForEndOfFrame();
				m_strengthCurrent -= 0.025f;
				if (m_strengthCurrent < 0f)
				{
					m_strengthCurrent = 0f;
				}
			}
		}

		public void RestoreStrength(float i_amount)
		{
			m_strengthCurrent += i_amount;
			if (m_strengthCurrent > m_strengthMax)
			{
				m_strengthCurrent = m_strengthMax;
			}
			CommonReferences.Instance.GetManagerHud().GetStrengthBar().Increase();
		}

		public void DamageStrength(float i_damage)
		{
			m_strengthCurrent -= i_damage;
			if (m_strengthCurrent < 0f)
			{
				m_strengthCurrent = 0f;
			}
			CommonReferences.Instance.GetManagerHud().GetStrengthBar().Decrease();
		}
	}
