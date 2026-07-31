using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
	public delegate void DelOnRestoreHealth();

	public delegate void DelOnDie();

	public delegate void DelPickUp();

	public delegate void DelPlatformChange();

	public delegate void DelChangeFacingSide(bool i_isFacingLeft);

	public delegate void DelIgnite();

	[SerializeField]
	protected string m_name;

	[SerializeField]
	protected string m_sex;

	[SerializeField]
	protected float m_healthMax;

	protected float m_healthCurrent;

	[SerializeField]
	protected float m_speedAcceleration;

	[SerializeField]
	protected float m_speedMax;

	[SerializeField]
	protected float m_traction01;

	protected List<Stat> m_stats = new List<Stat>();

	[SerializeField]
	protected List<AudioClip> m_audiosDie = new List<AudioClip>();

	[SerializeField]
	protected List<AudioClip> m_audiosUnique = new List<AudioClip>();

	[SerializeField]
	protected float m_chanceDropPickUpables01;

	[SerializeField]
	protected List<PickUpable> m_pickUpablesToDrop = new List<PickUpable>();

	[SerializeField]
	protected Color m_colorBlood;

	[SerializeField]
	protected List<PickUpable> m_pickUpables = new List<PickUpable>();

	protected Animator m_animator;

	private Rigidbody2D m_rigidbody2D;

	private Skeleton m_skeleton;

	protected float m_miniumXSpeedForAnim;

	protected float m_miniumYSpeedForAnim;

	protected float m_maxYSpeedForAnim;

	protected bool m_isTouchingLeftWall;

	protected bool m_isTouchingRightWall;

	protected StateActor m_stateActorCurrent;

	private float m_initialScaleX;

	protected bool m_isInvulnerable;

	protected bool m_isDead;

	protected bool m_isCanAttack;

	protected bool m_isAttacking;

	protected bool m_isCanBeAttacked;

	protected bool m_isCanMove;

	protected bool m_isThinking;

	protected bool m_isUpdateAnimToMovement;

	protected bool m_isGrounded;

	protected bool m_isMute;

	protected bool m_isOnFire;

	protected AudioSource m_audioSourceSFX;

	protected AudioSource m_audioSourceVoice;

	[SerializeField]
	protected Platform m_platformCurrent;

	protected float m_distanceFeetFromPos;

	protected float m_distanceHeadFromPos;

	private Coroutine m_coroutineRagdoll;

	private Coroutine m_coroutineGetUp;

	protected Coroutine m_coroutineCheckIfIsOnFire;

	private Coroutine m_coroutineCheckInvulnerability;

	public event DelOnRestoreHealth OnRestoreHealth;

	public event DelOnDie OnDie;

	public event DelPickUp OnPickUp;

	public event DelPlatformChange OnPlatformChange;

	public event DelChangeFacingSide OnChangeFacingSide;

	public event DelIgnite OnIgnite;

	public virtual void Awake()
	{
		if (m_animator == null)
		{
			m_animator = GetComponent<Animator>();
		}
		if (m_animator == null)
		{
			m_animator = GetComponentInChildren<Animator>();
		}
		InitializeStats();
		m_isThinking = true;
		m_isUpdateAnimToMovement = true;
		m_initialScaleX = base.transform.localScale.x;
		m_healthCurrent = GetStat("HealthMax").GetValueTotal();
		m_isDead = false;
		m_isCanAttack = true;
		m_isAttacking = false;
		m_isCanBeAttacked = true;
		m_isCanMove = true;
		m_isMute = false;
		m_isOnFire = false;
		m_miniumXSpeedForAnim = 1.25f;
		m_miniumYSpeedForAnim = 3f;
		m_maxYSpeedForAnim = 0f;
		m_stateActorCurrent = StateActor.Idle;
		AddAudioSources();
		m_skeleton = GetComponentInChildren<Skeleton>(includeInactive: true);
	}

	protected virtual void InitializeStats()
	{
		m_stats.Add(new Stat(StatNameActor.HealthMax, m_healthMax));
		m_stats.Add(new Stat(StatNameActor.DamageMultiplier, 1f));
		m_stats.Add(new Stat(StatNameActor.KnockbackXMultiplier, 1f));
		m_stats.Add(new Stat(StatNameActor.KnockbackYMultiplier, 1f));
		m_stats.Add(new Stat(StatNameActor.SpeedAccel, m_speedAcceleration));
		m_stats.Add(new Stat(StatNameActor.SpeedMax, m_speedMax));
		m_stats.Add(new Stat(StatNameActor.Traction, m_traction01));
	}

	private void AddAudioSources()
	{
		m_audioSourceSFX = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
		m_audioSourceVoice = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceVoice(base.gameObject);
	}

	protected virtual void RetrieveAnimationInfos(RuntimeAnimatorController i_runtimeAnimatorController)
	{
	}

	public virtual void Start()
	{
		HandleItemsInInventory();
	}

	protected virtual void OnEnable()
	{
		m_isAttacking = false;
		m_isCanAttack = true;
		m_stateActorCurrent = StateActor.Idle;
		SetIsThinking(i_isThinking: true);
		StartCoroutine(CoroutineCheckCurrentPlatform());
		StartCoroutine(CoroutineCheckIsGrounded());
		StartCoroutine(CoroutineUpdateIsTouchingWall());
		RetrieveAnimationInfos(GetAnimator().runtimeAnimatorController);
	}

	public virtual void FixedUpdate()
	{
		if (m_isUpdateAnimToMovement)
		{
			UpdateAnim();
		}
		else
		{
			m_animator.speed = 1f;
		}
	}

	private IEnumerator CoroutineCheckCurrentPlatform()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			CheckCurrentPlatform();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
		}
	}

	public void CheckCurrentPlatform()
	{
		Vector2 vector = ((m_stateActorCurrent != StateActor.Ragdoll) ? GetPosFeet() : ((Vector2)GetSkeleton().GetBone("hips").transform.position));
		RaycastHit2D[] array = Physics2D.RaycastAll(new Vector2(vector.x, vector.y + 0.15f), Vector2.down, 2f, LayerMask.GetMask("Platform"));
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i].collider.GetComponent<Platform>())
			{
				Platform component = array[i].collider.GetComponent<Platform>();
				if (m_platformCurrent != component)
				{
					m_platformCurrent = component;
					this.OnPlatformChange?.Invoke();
				}
				break;
			}
		}
	}

	public abstract void UpdateAnim();

	protected void MatchMoveAnimationWithMovementSpeed()
	{
		Vector3 vector = GetVelocity();
		if (vector.x < 0f)
		{
			vector.x *= -1f;
		}
		m_animator.speed = 1f / GetStat("SpeedMax").GetValueTotal() * vector.x;
	}

	private void HandleItemsInInventory()
	{
		foreach (PickUpable item in m_pickUpablesToDrop)
		{
			PickUpable pickUpable = Object.Instantiate(item, base.transform);
			pickUpable.PickUp(this);
			m_pickUpables.Add(pickUpable);
		}
	}

	public void Enable()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public virtual void MoveHorizontal(bool i_left)
	{
		if (!m_isCanMove || m_stateActorCurrent == StateActor.Ragdoll || (i_left && m_isTouchingLeftWall) || (!i_left && m_isTouchingRightWall))
		{
			return;
		}
		float valueTotal = GetStat("SpeedAccel").GetValueTotal();
		float valueTotal2 = GetStat("SpeedMax").GetValueTotal();
		Vector2 vector = default(Vector2);
		if (i_left)
		{
			vector = new Vector2(-1f, 0f);
			SetIsFacingLeft(i_isFacingLeft: true);
		}
		else
		{
			vector = new Vector2(1f, 0f);
			SetIsFacingLeft(i_isFacingLeft: false);
		}
		Vector2 vector2 = vector * Time.deltaTime * (valueTotal * 10f);
		float num = GetStat("Traction").GetValueTotal();
		if (num > 1f)
		{
			num = 1f;
		}
		if (num > 0f)
		{
			if (GetVelocity().x < 0f && !i_left)
			{
				vector2.x += num * 10f;
			}
			if (GetVelocity().x > 0f && i_left)
			{
				vector2.x -= num * 10f;
			}
		}
		GetRigidbody2D().velocity += vector2;
		if (GetVelocity().x * vector.x >= valueTotal2)
		{
			GetRigidbody2D().velocity = new Vector2(valueTotal2 * vector.x, GetRigidbody2D().velocity.y);
		}
	}

	private void HandleSlope()
	{
		float num = Mathf.Sign(GetVelocity().x);
		float num2 = Mathf.Abs(GetVelocity().x);
		Vector2 posFeet = GetPosFeet();
		Physics2D.Raycast(posFeet, Vector2.right * num, num2, LayerMask.NameToLayer("Platform"));
		Debug.DrawRay(posFeet, Vector2.right * num * num2, Color.red);
	}

	protected void MoveVertical(bool i_up)
	{
		if (i_up)
		{
			Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
			velocity.y -= GetStat("SpeedAccel").GetValueTotal() / 10f;
			if (GetComponent<Rigidbody2D>().velocity.y + velocity.y < 0f - GetStat("SpeedAccel").GetValueTotal())
			{
				velocity.y = 0f - GetStat("SpeedAccel").GetValueTotal();
			}
			GetComponent<Rigidbody2D>().velocity = velocity;
		}
		else
		{
			Vector2 velocity2 = GetComponent<Rigidbody2D>().velocity;
			velocity2.y += GetStat("SpeedAccel").GetValueTotal() / 10f;
			if (GetComponent<Rigidbody2D>().velocity.y + velocity2.y > GetStat("SpeedAccel").GetValueTotal())
			{
				velocity2.y = GetStat("SpeedAccel").GetValueTotal();
			}
			GetComponent<Rigidbody2D>().velocity = velocity2;
		}
	}

	public void StopMovingHorizontally()
	{
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		velocity.x = 0f;
		GetComponent<Rigidbody2D>().velocity = velocity;
	}

	public void StopMoving()
	{
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		velocity.x = 0f;
		velocity.y = 0f;
		GetComponent<Rigidbody2D>().velocity = velocity;
	}

	public void RestoreHealth(float i_amount)
	{
		m_healthCurrent += i_amount;
		if (m_healthCurrent > GetStat("HealthMax").GetValueTotal())
		{
			m_healthCurrent = GetStat("HealthMax").GetValueTotal();
		}
		if (this.OnRestoreHealth != null)
		{
			this.OnRestoreHealth();
		}
	}

	public virtual void Die()
	{
		if (m_audiosDie.Count > 0)
		{
			PlayAudio(m_audiosDie, 50);
		}
		m_isDead = true;
		if (Random.Range(0f, 1f) > 1f - m_chanceDropPickUpables01)
		{
			DropAllPickupAbles();
		}
		if (m_coroutineRagdoll != null)
		{
			StopCoroutine(m_coroutineRagdoll);
		}
		if (m_coroutineGetUp != null)
		{
			StopCoroutine(m_coroutineGetUp);
		}
		if (this.OnDie != null)
		{
			this.OnDie();
		}
		GetSkeleton().EnableRagdoll();
	}

	public virtual void PickUp(PickUpable i_pickUpable, bool i_isDuplicate)
	{
		PickUpable pickUpable = null;
		pickUpable = ((!i_isDuplicate) ? i_pickUpable : Object.Instantiate(i_pickUpable));
		m_pickUpables.Add(pickUpable);
		pickUpable.PickUp(this);
		if (this.OnPickUp != null)
		{
			this.OnPickUp();
		}
	}

	public virtual void DropPickupAble(PickUpable i_pickUpable, float i_powerDrop01)
	{
		if (i_pickUpable.GetIsCanDrop())
		{
			i_pickUpable.Drop(i_powerDrop01);
		}
	}

	public virtual void DropPickupAble(PickUpable i_pickUpable)
	{
		if (i_pickUpable.GetIsCanDrop())
		{
			i_pickUpable.Drop(0.5f);
		}
	}

	public virtual void DropAllPickupAbles()
	{
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			pickUpable.Drop(0.5f);
		}
	}

	public virtual void Ragdoll(float i_secsDuration)
	{
		if (m_stateActorCurrent != StateActor.Ragdoll)
		{
			m_stateActorCurrent = StateActor.Ragdoll;
			SetIsThinking(i_isThinking: false);
			if (m_coroutineRagdoll != null)
			{
				StopCoroutine(m_coroutineRagdoll);
			}
			m_coroutineRagdoll = StartCoroutine(CoroutineRagdoll(i_secsDuration));
		}
	}

	private IEnumerator CoroutineRagdoll(float i_secsDuration)
	{
		GetSkeletonActor().EnableRagdoll();
		yield return new WaitForSeconds(i_secsDuration);
		if (!m_isDead)
		{
			GetSkeletonActor().DisableRagdoll();
			GetUp();
		}
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
		m_animator.Play("GetUp");
		yield return new WaitForSeconds(1f);
		SetStateActor(StateActor.Idle);
		SetIsThinking(i_isThinking: true);
	}

	public abstract void TakeDamage(float i_amount);

	public void Ignite(string i_source)
	{
		float i_duration = 5f;
		float i_damagePerTick = 2f;
		float i_ticksPerSec = 2f;
		Ignite(i_source, i_duration, i_damagePerTick, i_ticksPerSec);
	}

	public void Ignite(string i_source, float i_duration, float i_damagePerTick, float i_ticksPerSec)
	{
		foreach (StatusEffect statusEffect in GetStatusEffects())
		{
			if (statusEffect is SEFire && statusEffect.GetSource() == i_source)
			{
				ResetStatusEffectComponentTimer(statusEffect);
				return;
			}
		}
		SEFire sEFire = new SEFire("Fire", i_source, TypeStatusEffect.Negative, i_duration, i_isStackable: true, i_ticksPerSec, i_damagePerTick);
		ApplyStatusEffect(sEFire);
		sEFire.Activate();
		if (!m_isOnFire)
		{
			GetSkeleton().EnableIgnition();
		}
		m_isOnFire = true;
		if (m_coroutineCheckIfIsOnFire == null)
		{
			m_coroutineCheckIfIsOnFire = StartCoroutine(CoroutineCheckIfIsOnFire());
		}
		this.OnIgnite?.Invoke();
	}

	private IEnumerator CoroutineCheckIfIsOnFire()
	{
		while (m_isOnFire)
		{
			m_isOnFire = false;
			foreach (StatusEffect statusEffect in GetStatusEffects())
			{
				if (statusEffect is SEFire && statusEffect.IsActive())
				{
					m_isOnFire = true;
					break;
				}
			}
			yield return new WaitForSeconds(0.25f);
		}
		m_isOnFire = false;
		GetSkeleton().DisableIgnition();
		m_coroutineCheckIfIsOnFire = null;
	}

	public void Extinguish()
	{
		if (m_coroutineCheckIfIsOnFire != null)
		{
			StopCoroutine(m_coroutineCheckIfIsOnFire);
		}
		foreach (StatusEffect statusEffect in GetStatusEffects())
		{
			if (statusEffect is SEFire)
			{
				statusEffect.End();
			}
		}
		GetSkeleton().DisableIgnition();
	}

	public bool IsOnFire()
	{
		return m_isOnFire;
	}

	public void DisableThinking(float i_secsDuration)
	{
		StartCoroutine(CoroutineDisableThinking(i_secsDuration));
	}

	private IEnumerator CoroutineDisableThinking(float i_duration)
	{
		SetIsThinking(i_isThinking: false);
		SetIsCanAttack(i_isCanAttack: false);
		yield return new WaitForSeconds(i_duration);
		SetIsThinking(i_isThinking: true);
		SetIsCanAttack(i_isCanAttack: true);
	}

	public virtual void PlayAudioVoice(AudioClip i_audio)
	{
		if (!m_isMute && !(i_audio == null))
		{
			GetAudioSourceVoice().PlayOneShot(i_audio);
		}
	}

	public virtual void PlayAudioVoice(List<AudioClip> i_audiosToChooseFrom)
	{
		if (!m_isMute && i_audiosToChooseFrom.Count != 0)
		{
			int index = Random.Range(0, i_audiosToChooseFrom.Count);
			GetAudioSourceVoice().PlayOneShot(i_audiosToChooseFrom[index]);
		}
	}

	public void PlayAudioSFX(AudioClip i_audio)
	{
		if (!(i_audio == null))
		{
			GetAudioSourceSFX().PlayOneShot(i_audio);
		}
	}

	public void PlayAudio(List<AudioClip> i_audiosToChooseFrom)
	{
		if (i_audiosToChooseFrom.Count != 0)
		{
			int index = Random.Range(0, i_audiosToChooseFrom.Count);
			GetAudioSourceSFX().PlayOneShot(i_audiosToChooseFrom[index]);
		}
	}

	public void PlayAudio(List<AudioClip> i_audiosToChooseFrom, int i_chanceOfPlaying)
	{
		if (i_audiosToChooseFrom.Count != 0)
		{
			int num = Random.Range(0, 101);
			if (num >= 100 - i_chanceOfPlaying)
			{
				num = Random.Range(0, i_audiosToChooseFrom.Count);
				GetAudioSourceSFX().PlayOneShot(i_audiosToChooseFrom[num]);
			}
		}
	}

	public void PlayAudioUnique(int i_indexAudioUnique)
	{
		PlayAudioSFX(m_audiosUnique[i_indexAudioUnique]);
	}

	public AudioSource GetAudioSourceSFX()
	{
		return m_audioSourceSFX;
	}

	public AudioSource GetAudioSourceVoice()
	{
		return m_audioSourceVoice;
	}

	public void SetIsFacingLeft(bool i_isFacingLeft)
	{
		if (GetIsFacingLeft() != i_isFacingLeft)
		{
			this.OnChangeFacingSide?.Invoke(i_isFacingLeft);
		}
		if (i_isFacingLeft)
		{
			base.transform.localScale = new Vector3(-1f, base.transform.localScale.y, base.transform.localScale.z);
		}
		else
		{
			base.transform.localScale = new Vector3(1f, base.transform.localScale.y, base.transform.localScale.z);
		}
		if ((bool)GetSkeleton())
		{
			GetSkeleton().SetIsFacingLeft(i_isFacingLeft);
		}
	}

	public void BecomeInvulnerable(string i_name, string i_source, float i_duration, bool i_isAffectSkeleton)
	{
		SEInvulnerability i_statusEffect = new SEInvulnerability(i_name, i_source, TypeStatusEffect.Other, i_duration, i_isStackable: true, i_isAffectSkeleton);
		SetIsInvulnerable(i_isInvulnerable: true, i_isAffectSkeleton);
		ApplyStatusEffect(i_statusEffect);
		if (m_coroutineCheckInvulnerability != null)
		{
			StopCoroutine(m_coroutineCheckInvulnerability);
		}
		m_coroutineCheckInvulnerability = StartCoroutine(CoroutineCheckInvulnerability());
	}

	private IEnumerator CoroutineCheckInvulnerability()
	{
		while (m_isInvulnerable)
		{
			m_isInvulnerable = false;
			List<StatusEffect> statusEffects = GetStatusEffects();
			for (int i = 0; i < statusEffects.Count; i++)
			{
				if (statusEffects[i] is SEInvulnerability && statusEffects[i].IsActive())
				{
					m_isInvulnerable = true;
					break;
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: true);
		m_coroutineCheckInvulnerability = null;
	}

	public virtual void SetIsInvulnerable(bool i_isInvulnerable, bool i_isAffectSkeleton)
	{
		if (i_isInvulnerable)
		{
			m_isInvulnerable = true;
			m_isCanBeAttacked = false;
		}
		else
		{
			m_isInvulnerable = false;
			m_isCanBeAttacked = true;
		}
		if (i_isAffectSkeleton)
		{
			GetSkeleton().SetInvulnerable(i_isInvulnerable);
		}
	}

	public string GetName()
	{
		return m_name;
	}

	public Animator GetAnimator()
	{
		return m_animator;
	}

	public bool GetIsFacingLeft()
	{
		if (base.transform.localScale.x < 0f)
		{
			return true;
		}
		return false;
	}

	public bool GetIsThinking()
	{
		return m_isThinking;
	}

	public void SetIsThinking(bool i_isThinking)
	{
		m_isThinking = i_isThinking;
	}

	public virtual Vector2 GetPos()
	{
		return base.transform.position;
	}

	public virtual Vector2 GetPosHips()
	{
		return GetSkeletonActor().GetBone("hips").transform.position;
	}

	public void SetPos(Vector2 i_pos)
	{
		base.transform.position = i_pos;
	}

	public void SetPosX(float i_posX)
	{
		Vector2 pos = GetPos();
		pos.x = i_posX;
		SetPos(pos);
	}

	public void SetPosY(float i_posY)
	{
		Vector2 pos = GetPos();
		pos.y = i_posY;
		SetPos(pos);
	}

	public List<Collider2D> GetAllColliders()
	{
		List<Collider2D> list = new List<Collider2D>();
		Collider2D[] components = GetComponents<Collider2D>();
		foreach (Collider2D item in components)
		{
			list.Add(item);
		}
		components = GetComponentsInChildren<Collider2D>(includeInactive: true);
		foreach (Collider2D item2 in components)
		{
			list.Add(item2);
		}
		return list;
	}

	public bool GetIsAttacking()
	{
		return m_isAttacking;
	}

	public bool GetIsCanAttack()
	{
		return m_isCanAttack;
	}

	public void SetIsCanAttack(bool i_isCanAttack)
	{
		m_isCanAttack = i_isCanAttack;
	}

	public bool IsDead()
	{
		return m_isDead;
	}

	public float GetHealthCurrent()
	{
		return m_healthCurrent;
	}

	public bool GetIsCanBeAttacked()
	{
		return m_isCanBeAttacked;
	}

	public Vector2 GetVelocity()
	{
		return GetComponent<Rigidbody2D>().velocity;
	}

	public void SetVelocity(Vector2 i_velocity2d)
	{
		GetComponent<Rigidbody2D>().velocity = i_velocity2d;
	}

	public virtual bool GetIsHasPickUpable(PickUpable i_pickUpable)
	{
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (i_pickUpable == pickUpable)
			{
				return true;
			}
		}
		return false;
	}

	public StateActor GetStateActorCurrent()
	{
		return m_stateActorCurrent;
	}

	public void SetStateActor(StateActor i_stateActor)
	{
		m_stateActorCurrent = i_stateActor;
	}

	public bool GetIsInvulnerable()
	{
		return m_isInvulnerable;
	}

	public Vector2 GetSize()
	{
		return GetComponent<SpriteRenderer>().bounds.size;
	}

	public virtual Vector2 GetPosFeet()
	{
		if (m_distanceFeetFromPos == 0f)
		{
			Vector2 a = GetComponent<Collider2D>().ClosestPoint(new Vector2(GetPos().x, GetPos().y - 15f));
			m_distanceFeetFromPos = Vector2.Distance(a, GetPos());
		}
		return new Vector2(GetPos().x, GetPos().y - m_distanceFeetFromPos);
	}

	public virtual Vector2 GetPosTopHead()
	{
		if (m_distanceHeadFromPos == 0f)
		{
			Vector2 a = GetComponent<Collider2D>().ClosestPoint(new Vector2(GetPos().x, GetPos().y + 15f));
			m_distanceHeadFromPos = Vector2.Distance(a, GetPos());
		}
		return new Vector2(GetPos().x, GetPos().y + m_distanceHeadFromPos);
	}

	private IEnumerator CoroutineCheckIsGrounded()
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			m_isGrounded = GetIsGroundedRayCast();
		}
	}

	public bool GetIsGrounded()
	{
		return m_isGrounded;
	}

	public bool GetIsGroundedRayCast()
	{
		RaycastHit2D raycastHit2D;
		if (GetRigidbody2D().gravityScale > 0f)
		{
			if (m_name == "Jacky")
			{
				Debug.DrawLine(GetPosFeet(), Vector2.down * 0.15f, Color.blue, 1f);
			}
			raycastHit2D = Physics2D.Raycast(GetPosFeet(), Vector2.down, 0.15f, LayerMask.GetMask("Platform", "Ledge"));
		}
		else
		{
			raycastHit2D = Physics2D.Raycast(GetPosTopHead(), Vector2.up, 0.15f, LayerMask.GetMask("Platform", "Ledge"));
		}
		if ((bool)raycastHit2D)
		{
			m_isGrounded = true;
			return true;
		}
		m_isGrounded = false;
		return false;
	}

	private IEnumerator CoroutineUpdateIsTouchingWall()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		while (true)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			UpdateIsTouchingWall();
		}
	}

	private void UpdateIsTouchingWall()
	{
		if (m_stateActorCurrent == StateActor.Climbing)
		{
			return;
		}
		float num = 0.5f;
		int num2 = (int)(GetHeight() / num);
		int mask = LayerMask.GetMask("Platform", "Interactable");
		float num3 = 0.5f;
		bool isTouchingLeftWall = false;
		for (int i = 0; i < num2 - 1; i++)
		{
			Vector2 vector = new Vector2(GetPos().x, GetPosFeet().y + num * (float)(i + 1));
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.left, num3, mask);
			Debug.DrawRay(vector, Vector2.left, Color.cyan, 1f);
			if (!raycastHit2D || raycastHit2D.collider.isTrigger)
			{
				continue;
			}
			if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
			{
				Interactable component = raycastHit2D.collider.gameObject.GetComponent<Interactable>();
				if ((bool)component)
				{
					if (!component.IsObstructionPaths())
					{
						continue;
					}
				}
				else
				{
					component = raycastHit2D.collider.gameObject.GetComponentInChildren<Interactable>();
					if ((bool)component && !component.IsObstructionPaths())
					{
						continue;
					}
				}
			}
			isTouchingLeftWall = true;
			if (!m_isTouchingLeftWall)
			{
				StopMovingHorizontally();
				GetRigidbody2D().MovePosition(new Vector2(raycastHit2D.point.x + num3, GetRigidbody2D().position.y));
			}
			break;
		}
		m_isTouchingLeftWall = isTouchingLeftWall;
		isTouchingLeftWall = false;
		for (int j = 0; j < num2 - 1; j++)
		{
			Vector2 vector2 = new Vector2(GetPos().x, GetPosFeet().y + num * (float)(j + 1));
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.right, num3, mask);
			Debug.DrawRay(vector2, Vector2.right, Color.cyan, 1f);
			if (!raycastHit2D2 || raycastHit2D2.collider.isTrigger)
			{
				continue;
			}
			if (raycastHit2D2.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
			{
				Interactable component2 = raycastHit2D2.collider.gameObject.GetComponent<Interactable>();
				if ((bool)component2)
				{
					if (!component2.IsObstructionPaths())
					{
						continue;
					}
				}
				else
				{
					component2 = raycastHit2D2.collider.gameObject.GetComponentInChildren<Interactable>();
					if ((bool)component2 && !component2.IsObstructionPaths())
					{
						continue;
					}
				}
			}
			isTouchingLeftWall = true;
			if (!m_isTouchingRightWall)
			{
				StopMovingHorizontally();
				GetRigidbody2D().MovePosition(new Vector2(raycastHit2D2.point.x - num3, GetRigidbody2D().position.y));
			}
			break;
		}
		m_isTouchingRightWall = isTouchingLeftWall;
	}

	public bool IsTouchingLeftWall()
	{
		return m_isTouchingLeftWall;
	}

	public bool IsTouchingRightWall()
	{
		return m_isTouchingRightWall;
	}

	public Rigidbody2D GetRigidbody2D()
	{
		if (m_rigidbody2D == null)
		{
			m_rigidbody2D = GetComponent<Rigidbody2D>();
		}
		return m_rigidbody2D;
	}

	public virtual Skeleton GetSkeleton()
	{
		return m_skeleton;
	}

	public virtual SkeletonActor GetSkeletonActor()
	{
		return (SkeletonActor)GetSkeleton();
	}

	public Platform GetPlatformCurrent()
	{
		return m_platformCurrent;
	}

	public virtual float GetHeight()
	{
		return GetComponent<Collider2D>().bounds.size.y;
	}

	public void SetIsMute(bool i_isMute)
	{
		m_isMute = i_isMute;
	}

	public bool IsMute()
	{
		return m_isMute;
	}

	public void PlaceFeetOnPos(Vector2 i_pos)
	{
		SetPos(i_pos);
		float num = Vector2.Distance(new Vector2(i_pos.x, GetPosFeet().y), i_pos);
		SetPos(new Vector2(i_pos.x, i_pos.y + num));
	}

	public void PlaceHeadOnPos(Vector2 i_pos)
	{
		SetPos(i_pos);
		float num = Vector2.Distance(new Vector2(i_pos.x, GetPosTopHead().y), i_pos);
		SetPos(new Vector2(i_pos.x, i_pos.y - num));
	}

	public bool GetIsUpdateAnimToMovement()
	{
		return m_isUpdateAnimToMovement;
	}

	public Color GetColorBlood()
	{
		return m_colorBlood;
	}

	public void SetIsUpdateAnimToMovement(bool i_isUpdate)
	{
		m_isUpdateAnimToMovement = i_isUpdate;
	}

	public Stat GetStat(string i_nameStat)
	{
		for (int i = 0; i < m_stats.Count; i++)
		{
			if (m_stats[i].GetName() == i_nameStat)
			{
				return m_stats[i];
			}
		}
		return null;
	}

	public virtual bool ApplyStatusEffect(StatusEffect i_statusEffect)
	{
		if (!i_statusEffect.IsStackable() && IsStatusEffectAppliedAlready(i_statusEffect))
		{
			return false;
		}
		StatusEffectComponent statusEffectComponent = base.gameObject.AddComponent<StatusEffectComponent>();
		i_statusEffect.SetActor(this);
		statusEffectComponent.Initialize(i_statusEffect);
		statusEffectComponent.ActivateStatusEffect();
		return true;
	}

	public void EndStatusEffect(StatusEffect i_statusEffect)
	{
		StatusEffectComponent[] components = GetComponents<StatusEffectComponent>();
		foreach (StatusEffectComponent statusEffectComponent in components)
		{
			if (statusEffectComponent.GetStatusEffect() == i_statusEffect)
			{
				statusEffectComponent.EndStatusEffect();
				break;
			}
		}
	}

	public List<StatusEffect> GetStatusEffects()
	{
		List<StatusEffect> list = new List<StatusEffect>();
		StatusEffectComponent[] components = GetComponents<StatusEffectComponent>();
		foreach (StatusEffectComponent statusEffectComponent in components)
		{
			list.Add(statusEffectComponent.GetStatusEffect());
		}
		return list;
	}

	public List<StatusEffectComponent> GetStatusEffectComponents()
	{
		List<StatusEffectComponent> list = new List<StatusEffectComponent>();
		StatusEffectComponent[] components = GetComponents<StatusEffectComponent>();
		foreach (StatusEffectComponent item in components)
		{
			list.Add(item);
		}
		return list;
	}

	public bool IsStatusEffectAppliedAlready(string i_nameStatusEffectToCheck)
	{
		foreach (StatusEffect statusEffect in GetStatusEffects())
		{
			if (statusEffect.GetName() == i_nameStatusEffectToCheck)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsStatusEffectAppliedAlready(StatusEffect i_statusEffectToCheck)
	{
		foreach (StatusEffect statusEffect in GetStatusEffects())
		{
			if (statusEffect.GetName() == i_statusEffectToCheck.GetName())
			{
				return true;
			}
		}
		return false;
	}

	public void ResetStatusEffectComponentTimer(StatusEffect i_statusEffect)
	{
		foreach (StatusEffectComponent statusEffectComponent in GetStatusEffectComponents())
		{
			if (statusEffectComponent.GetStatusEffect() == i_statusEffect)
			{
				statusEffectComponent.ResetDurationLeft();
			}
		}
	}

	public StatModifier AddStatModifier(string i_nameStat, float i_valueModification)
	{
		return GetStat(i_nameStat).AddModifier(i_valueModification);
	}

	public void RemoveStatModifier(StatModifier i_modifier)
	{
		GetStat(i_modifier.GetNameStat()).RemoveModifier(i_modifier);
	}

	public void RemoveStatModifier(List<StatModifier> i_modifiers)
	{
		foreach (StatModifier i_modifier in i_modifiers)
		{
			GetStat(i_modifier.GetNameStat()).RemoveModifier(i_modifier);
		}
	}

	public string GetSex()
	{
		return m_sex;
	}
}
