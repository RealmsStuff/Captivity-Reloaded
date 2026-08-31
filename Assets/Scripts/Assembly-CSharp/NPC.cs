using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public abstract class NPC : Actor
{
	public delegate void Hit(Actor i_attacker, Actor i_receiver);

	public delegate void GetHit(Actor i_attacker, Actor i_receiver);

	public delegate void DelAttack();

	public delegate void DelTakeHitBullet();

	[Header("---NPC---")]
	[SerializeField]
	protected int m_id;

	[SerializeField]
	protected Sprite m_sprIcon;

	[SerializeField]
	protected Stage m_stageAppearance;

	[SerializeField]
	protected List<Interaction> m_interactions;

	[SerializeField]
	protected List<AttackNPC> m_attacks;

	protected AttackNPC m_attackCurrent;

	[TextArea(1, 5)]
	[SerializeField]
	protected string m_description;

	[SerializeField]
	protected int m_bounty;

	[SerializeField]
	protected float m_amountIncreaseHealthMaxPerWave;

	[SerializeField]
	protected bool m_isIgnoreWave;

	protected StateNPC m_stateNPCCurrent;

	protected XAI m_xAI;

	protected Raper m_raper;

	protected bool m_isPlayerParent;

	protected float m_timeSpawn;

	protected bool m_isHasLineOfSightToPlayer;

	public event Hit OnHit;

	public event GetHit OnGetHit;

	public event DelAttack OnAttack;

	public event DelTakeHitBullet OnTakeHitBullet;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
		if (!(this is Egg))
		{
			CommonReferences.Instance.GetManagerHud().AddHealthDisplay(this);
		}
		else
		{
			SetIsIgnoreWave(i_isIgnoreWave: true);
		}
		m_stateNPCCurrent = StateNPC.None;
		AddXAIComponent();
		if ((bool)GetComponentInChildren<Raper>())
		{
			m_raper = GetComponentInChildren<Raper>();
		}
		if (m_attacks.Count > 0)
		{
			PickNextAttack();
		}
	}

	protected abstract void AddXAIComponent();

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(CoroutineCheckIfOutOfBounds());
		StartCoroutine(CoroutineCheckIsHasLineOfSightToPlayer());
	}

	public virtual void Spawn(bool i_isFadeIn)
	{
		if (i_isFadeIn)
		{
			GetSkeletonActor().FadeBodyIn();
		}
		m_timeSpawn = Time.time;
		AddStatModifier("SpeedAccel", Random.Range(-0.2f, 0.2f));
		AddStatModifier("SpeedMax", Random.Range(-0.2f, 0.2f));
		if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs()
			.Count > 1)
		{
			RemoveOldestNpcOfSameType();
		}
		switch (ManagerDB.GetDifficulty())
		{
		case "Casual":
			AddStatModifier("HealthMax", GetStat("HealthMax").GetValueBase() * 0.25f * -1f);
			break;
		case "Hard":
			AddStatModifier("HealthMax", GetStat("HealthMax").GetValueBase() * 0.25f);
			break;
		}
		m_healthCurrent = GetStat("HealthMax").GetValueTotal();
		if ((bool)CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave())
		{
			int numWaveCurrent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
				.GetNumWaveCurrent();
			numWaveCurrent--;
			if (numWaveCurrent != 0)
			{
				AddStatModifier("HealthMax", (float)numWaveCurrent * m_amountIncreaseHealthMaxPerWave);
				m_healthCurrent = GetStat("HealthMax").GetValueTotal();
			}
		}
	}

	private void RemoveOldestNpcOfSameType()
	{
		List<NPC> allNPCs = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs();
		List<NPC> list = new List<NPC>();
		for (int i = 0; i < allNPCs.Count; i++)
		{
			if (!(allNPCs[i] == null) && !(allNPCs[i] == this) && allNPCs[i].GetId() != -1 && allNPCs[i].GetId() == GetId() && !allNPCs[i].GetRaper().GetIsRaping() && !allNPCs[i].IsDead() && (!(allNPCs[i] is HeadHumper) || !((HeadHumper)allNPCs[i]).IsHeadHugging()))
			{
				list.Add(allNPCs[i]);
			}
		}
		if (list.Count <= CommonReferences.Instance.GetManagerStages().GetMaxNumberOfNpcsOfSameTypeOnStageAtGivenMoment())
		{
			return;
		}
		NPC nPC = null;
		for (int j = 0; j < list.Count; j++)
		{
			if (nPC == null)
			{
				nPC = list[j];
			}
			else if (list[j].GetTimeSpawn() < nPC.GetTimeSpawn())
			{
				nPC = list[j];
			}
		}
		nPC.m_isDead = true;
		Object.Destroy(nPC.gameObject);
	}

	public override void FixedUpdate()
	{
		if (!m_isDead)
		{
			if (m_isThinking)
			{
				HandleThinking();
			}
			if (GetIsRaper())
			{
				GetRaper().GetIsRaping();
			}
			base.FixedUpdate();
		}
	}

	public Raper GetRaper()
	{
		if (m_raper == null)
		{
			m_raper = GetComponentInChildren<Raper>();
		}
		return m_raper;
	}

	protected virtual void HandleThinking()
	{
		m_xAI.HandleIntelligence();
	}

	public abstract void MoveToPlayer();

	public abstract void MoveAwayFromPlayer();

	public override void UpdateAnim()
	{
		if (m_stateActorCurrent == StateActor.Climbing)
		{
			return;
		}
		if (GetComponent<Rigidbody2D>().velocity == Vector2.zero)
		{
			m_stateActorCurrent = StateActor.Idle;
		}
		if (GetComponent<Rigidbody2D>().velocity.x <= 0f - m_miniumXSpeedForAnim || GetComponent<Rigidbody2D>().velocity.x >= m_miniumXSpeedForAnim)
		{
			m_stateActorCurrent = StateActor.Moving;
		}
		if (!GetIsGrounded())
		{
			m_stateActorCurrent = StateActor.Jumping;
		}
		switch (m_stateActorCurrent)
		{
		case StateActor.Idle:
			SetAllAnimatorBoolsToFalse();
			if (m_stateNPCCurrent == StateNPC.Await)
			{
				GetAnimator().SetBool("IsAwaiting", value: true);
			}
			break;
		case StateActor.Moving:
			SetAllAnimatorBoolsToFalse();
			if (GetIsRaper() && GetRaper().GetIsRaping())
			{
				m_animator.SetBool("IsMoving", value: false);
				return;
			}
			m_animator.SetBool("IsMoving", value: true);
			break;
		case StateActor.Jumping:
			SetAllAnimatorBoolsToFalse();
			m_animator.SetBool("IsJumping", value: true);
			break;
		}
		if (m_stateActorCurrent != StateActor.Moving || m_isAttacking)
		{
			m_animator.speed = 1f;
		}
	}

	protected virtual void SetAllAnimatorBoolsToFalse()
	{
		m_animator.SetBool("IsMoving", value: false);
		m_animator.SetBool("IsJumping", value: false);
		m_animator.SetBool("IsAwaiting", value: false);
	}

	public virtual void StartAttack()
	{
		if (m_isCanAttack && m_isThinking && m_attacks.Count != 0)
		{
			Attack(CommonReferences.Instance.GetPlayer());
		}
	}

	protected void Attack(Actor i_targetActor)
	{
		if (!m_attackCurrent.IsMovesDuringAttack())
		{
			m_isCanMove = false;
			StopMovingHorizontally();
		}
		PlayAudio(m_attackCurrent.GetAudiosAttackStart());
		m_attackCurrent.HandleAttackStart();
		StartCoroutine(CoroutineAttack(i_targetActor));
		if (this.OnAttack != null)
		{
			this.OnAttack();
		}
	}

	protected virtual IEnumerator CoroutineAttack(Actor i_targetActor)
	{
		m_isAttacking = true;
		m_animator.SetBool("IsAttacking", value: true);
		m_animator.SetTrigger(m_attackCurrent.GetNameStateAndTrigger());
		Timer timer = new Timer(m_attackCurrent.GetDurationAttack());
		yield return timer.CoroutinePlayAndWaitForEnd();
		m_animator.SetBool("IsAttacking", value: false);
		m_animator.ResetTrigger(m_attackCurrent.GetNameStateAndTrigger());
		m_attackCurrent.HandleAttackEnd();
		m_isAttacking = false;
		m_isCanMove = true;
	}

	public void PickNextAttack()
	{
		if (m_attacks.Count == 1)
		{
			m_attackCurrent = m_attacks[0];
			return;
		}
		float num = 0f;
		List<AttackNPCPickModel> list = new List<AttackNPCPickModel>();
		foreach (AttackNPC attack in m_attacks)
		{
			float i_startIndex = num;
			num += attack.GetChance01();
			float i_endIndex = num;
			list.Add(new AttackNPCPickModel(attack, i_startIndex, i_endIndex));
		}
		float num2 = Random.Range(0f, num);
		foreach (AttackNPCPickModel item in list)
		{
			if (num2 >= item.m_startIndex && num2 <= item.m_endIndex)
			{
				m_attackCurrent = item.m_attackNPC;
			}
		}
	}

	public void PerformAttack(Actor i_targetActor)
	{
		if (IsDead() || GetStateActorCurrent() == StateActor.Climbing)
		{
			return;
		}
		PlayAudio(m_attackCurrent.GetAudiosAttackPerform());
		StartCoroutine(CoroutineAttackCooldown());
		Player player = (Player)i_targetActor;
		if (GetIsRaper())
		{
			if (player.GetStatePlayerCurrent() == StatePlayer.BeingRaped)
			{
				m_attackCurrent.HandleAttackPerform();
				return;
			}
			if (player.IsExposing() && GetIsCloseEnoughToHitPlayer())
			{
				m_attackCurrent.HandleAttackPerform();
				StartRape();
				return;
			}
			if (GetIsCloseEnoughToHitPlayer() && i_targetActor.GetIsCanBeAttacked() && player.GetIsCanBeRaped() && player.GetStateActorCurrent() == StateActor.Ragdoll)
			{
				m_attackCurrent.HandleAttackPerform();
				StartRape();
				return;
			}
		}
		if (GetIsCloseEnoughToHitPlayer() && i_targetActor.GetIsCanBeAttacked())
		{
			m_attackCurrent.HandleAttackHit();
			player.TakeHit(this);
			if (this.OnHit != null)
			{
				this.OnHit(this, i_targetActor);
			}
		}
		m_attackCurrent.HandleAttackPerform();
	}

	private IEnumerator CoroutineAttackCooldown()
	{
		m_isCanAttack = false;
		Timer timer = new Timer(m_attackCurrent.GetDurationCooldown());
		yield return timer.CoroutinePlayAndWaitForEnd();
		PickNextAttack();
		m_isCanAttack = true;
	}

	public virtual bool TakeHitBullet(Actor i_initiator, Bullet i_bullet, BodyPartActor i_bodyPart)
	{
		this.OnTakeHitBullet?.Invoke();
		if (m_isInvulnerable)
		{
			return false;
		}
		if (!m_isDead)
		{
			TakeDamage(i_bullet.GetDamage() * i_bodyPart.GetDamageMultiplierAmountBodyPart());
		}
		TakeKnockbackBullet(i_bullet, i_bodyPart.GetComponentInParent<Bone>());
		this.OnGetHit?.Invoke(i_initiator, this);
		return true;
	}

	public void TakeKnockbackBullet(Bullet i_bullet, Bone i_bone)
	{
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		Vector2 velocity = component != null && component.bodyType == RigidbodyType2D.Dynamic ? component.velocity : Vector2.zero;
		if (i_bullet.GetOwner().transform.position.x > base.transform.position.x)
		{
			velocity.x -= i_bullet.GetKockbackX();
		}
		else
		{
			velocity.x += i_bullet.GetKockbackX();
		}
		Rigidbody2D rigidbody2D = i_bone != null ? i_bone.GetComponent<Rigidbody2D>() : null;
		if (rigidbody2D != null && rigidbody2D.bodyType == RigidbodyType2D.Dynamic && m_isDead)
		{
			if (((BodyPartActor)i_bone.GetBodyPart()).GetIsBodyPartDestroyed())
			{
				rigidbody2D.velocity = velocity;
			}
			else
			{
				rigidbody2D.velocity = velocity * 3f;
			}
			_ = (SkeletonActor)GetSkeleton();
		}
		if (component != null && component.bodyType == RigidbodyType2D.Dynamic)
		{
			component.velocity = velocity;
		}
	}

	public override void TakeDamage(float i_amount)
	{
		if (m_healthCurrent >= i_amount)
		{
			m_healthCurrent -= i_amount;
		}
		else
		{
			m_healthCurrent = 0f;
		}
		if (m_healthCurrent < 1f && !m_isDead)
		{
			Die();
		}
	}

	public virtual void StartRape()
	{
		if (IsOnFire())
		{
			Extinguish();
		}
		GetRaper().BeginRape();
	}

	public void StartRapeArchive()
	{
		GetRaper().BeginRapeArchive();
	}

	public void ForceEndRape()
	{
		if (GetRaper().GetIsRaping())
		{
			GetRaper().ForceEndRape();
		}
	}

	public void SetIsIgnoreCollisionWithPlayer(Actor i_actor, bool i_ignore)
	{
		foreach (Collider2D allCollider in i_actor.GetAllColliders())
		{
			foreach (Collider2D allCollider2 in CommonReferences.Instance.GetPlayer().GetAllColliders())
			{
				Physics2D.IgnoreCollision(allCollider, allCollider2, i_ignore);
			}
		}
	}

	public void FacePlayer()
	{
		if (base.transform.position.x - CommonReferences.Instance.GetPlayer().GetPosHips().x > 0f)
		{
			SetIsFacingLeft(i_isFacingLeft: true);
		}
		else
		{
			SetIsFacingLeft(i_isFacingLeft: false);
		}
	}

	public float GetDistanceBetweenPlayer()
	{
		return Vector3.Distance(CommonReferences.Instance.GetPlayer().transform.position, base.transform.position);
	}

	public float GetDistanceBetweenPlayerHips()
	{
		return Vector3.Distance(CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips)
			.transform.position, base.transform.position);
	}

	public bool GetIsRaper()
	{
		if (GetRaper() == null)
		{
			return false;
		}
		return true;
	}

	public override void Die()
	{
		base.Die();
		CommonReferences.Instance.GetPlayerController().GainMoney(m_bounty);
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(base.gameObject, 20f);
		CommonReferences.Instance.GetManagerAudio().PlayAudioHitsound(1f, i_isKill: true);
		CommonReferences.Instance.GetPlayer().KillNpc(this);
		StartCoroutine(CoroutineWaitBeforeDisableAllPhysics());
		TryDropAmmoBox();
	}

	private void TryDropAmmoBox()
	{
		int num = Random.Range(0, 101);
		int num2 = 7;
		if (num > 100 - num2)
		{
			AmmoBox ammoBoxDupe = Library.Instance.Items.GetAmmoBoxDupe();
			ammoBoxDupe.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetItemsParent();
			ammoBoxDupe.transform.position = GetPos();
			ammoBoxDupe.gameObject.SetActive(value: true);
			Object.Destroy(ammoBoxDupe.gameObject, 15f);
		}
	}

	private IEnumerator CoroutineWaitBeforeDisableAllPhysics()
	{
		yield return new WaitForSeconds(6f);
		Rigidbody2D[] componentsInChildren = GetComponentsInChildren<Rigidbody2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].bodyType = RigidbodyType2D.Static;
		}
	}

	public void SetState(StateActor i_stateActor)
	{
		m_stateActorCurrent = i_stateActor;
	}

	public bool GetIsCanRape()
	{
		if (GetIsRaper() && GetRaper().GetIsCanRape())
		{
			return true;
		}
		return false;
	}

	public virtual bool GetIsCloseEnoughToAttackPlayer()
	{
		if (Vector3.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPosHips()) < GetRangeInitiateAttackAttackCurrent())
		{
			return true;
		}
		if (Vector3.Distance(GetPosFeet(), CommonReferences.Instance.GetPlayer().GetPosHips()) < GetRangeInitiateAttackAttackCurrent())
		{
			return true;
		}
		return false;
	}

	public virtual bool GetIsCloseEnoughToHitPlayer()
	{
		if (Vector3.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPosHips()) < GetRangeAttackHitAttackCurrent())
		{
			return true;
		}
		if (Vector3.Distance(GetPosFeet(), CommonReferences.Instance.GetPlayer().GetPosHips()) < GetRangeAttackHitAttackCurrent())
		{
			return true;
		}
		return false;
	}

	public bool GetIsPlayerLeftOfMe()
	{
		if (CommonReferences.Instance.GetPlayer().GetPos().x < GetPos().x)
		{
			return true;
		}
		return false;
	}

	public bool GetIsPlayerBackTurnedToMe()
	{
		if (GetIsPlayerLeftOfMe())
		{
			if (CommonReferences.Instance.GetPlayer().GetIsFacingLeft())
			{
				return true;
			}
		}
		else if (!CommonReferences.Instance.GetPlayer().GetIsFacingLeft())
		{
			return true;
		}
		return false;
	}

	public string GetDescription()
	{
		return m_description;
	}

	public Sprite GetSprIcon()
	{
		return m_sprIcon;
	}

	public float GetRangeVision()
	{
		return 20f;
	}

	public bool GetIsCloseEnoughToSeePlayer()
	{
		if (Vector3.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPosHips()) < GetRangeVision())
		{
			return true;
		}
		return false;
	}

	private IEnumerator CoroutineCheckIsHasLineOfSightToPlayer()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			IsHasLineOfSightToPlayerRaycast();
		}
	}

	public virtual bool IsHasLineOfSightToPlayerRaycast()
	{
		Vector2 pos = GetPos();
		Vector2 pos2 = CommonReferences.Instance.GetPlayer().GetPos();
		Vector2 direction = pos2 - pos;
		direction.Normalize();
		float distance = Vector2.Distance(pos, pos2);
		int mask = LayerMask.GetMask("Player", "Platform", "Interactable");
		RaycastHit2D[] array = Physics2D.RaycastAll(pos, direction, distance, mask);
		m_isHasLineOfSightToPlayer = true;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
			{
				m_isHasLineOfSightToPlayer = false;
				break;
			}
			if (array[i].collider.gameObject.layer == LayerMask.NameToLayer("Interactable") && (bool)array[i].collider.GetComponent<Interactable>() && array[i].collider.GetComponent<Interactable>().IsObstructionPaths())
			{
				m_isHasLineOfSightToPlayer = false;
				break;
			}
		}
		return m_isHasLineOfSightToPlayer;
	}

	public bool IsHasLineOfSightToPos(Vector2 i_pos)
	{
		Vector2 posFeet = GetPosFeet();
		posFeet.y += 0.2f;
		Vector2 direction = i_pos - posFeet;
		float distance = Vector2.Distance(posFeet, i_pos);
		int mask = LayerMask.GetMask("Platform", "Interactable");
		RaycastHit2D raycastHit2D = Physics2D.Raycast(posFeet, direction, distance, mask);
		if ((bool)raycastHit2D)
		{
			if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
			{
				return false;
			}
			if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Interactable") && (bool)raycastHit2D.collider.gameObject.GetComponent<Interactable>() && raycastHit2D.collider.GetComponent<Interactable>().IsObstructionPaths())
			{
				return false;
			}
		}
		posFeet = GetPosTopHead();
		posFeet.y -= 0.2f;
		direction = i_pos - posFeet;
		distance = Vector2.Distance(posFeet, i_pos);
		raycastHit2D = Physics2D.Raycast(posFeet, direction, distance, mask);
		if ((bool)raycastHit2D)
		{
			if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
			{
				return false;
			}
			if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Interactable") && (bool)raycastHit2D.collider.gameObject.GetComponent<Interactable>() && raycastHit2D.collider.GetComponent<Interactable>().IsObstructionPaths())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsHasLineOfSightToPlayer()
	{
		return m_isHasLineOfSightToPlayer;
	}

	public void SetStateNPC(StateNPC i_state)
	{
		m_stateNPCCurrent = i_state;
	}

	public void DieRapeWin()
	{
		m_isDead = true;
		Object.Destroy(base.gameObject);
	}

	private IEnumerator CoroutineCheckIfOutOfBounds()
	{
		while (!IsDead())
		{
			yield return new WaitForSeconds(3f);
			if (GetDistanceBetweenPlayer() > 2000f)
			{
				Die();
			}
		}
	}

	public float GetDamageAttackCurrentTotal()
	{
		return m_attackCurrent.GetDamage() * GetStat("DamageMultiplier").GetValueTotal();
	}

	public float GetKnockbackXAttackCurrentTotal()
	{
		return m_attackCurrent.GetKnockbackX() * GetStat("KnockbackXMultiplier").GetValueTotal();
	}

	public float GetKnockbackYAttackCurrentTotal()
	{
		return m_attackCurrent.GetKnockbackY() * GetStat("KnockbackYMultiplier").GetValueTotal();
	}

	public bool GetIsMovesDuringAttackAttackCurrent()
	{
		return m_attackCurrent.IsMovesDuringAttack();
	}

	public List<AudioClip> GetAudiosDie()
	{
		return m_audiosDie;
	}

	public List<AudioClip> GetAudiosAttackStartAttackCurrent()
	{
		return m_attackCurrent.GetAudiosAttackStart();
	}

	public AudioClip GetAudioAttackHitAttackCurrent()
	{
		return m_attackCurrent.GetAudioAttackHit();
	}

	public float GetRangeInitiateAttackAttackCurrent()
	{
		return m_attackCurrent.GetRangeInitiate();
	}

	public float GetRangeAttackHitAttackCurrent()
	{
		return m_attackCurrent.GetRangeHit();
	}

	public AttackNPC GetAttackCurrent()
	{
		return m_attackCurrent;
	}

	public void SetIsIgnoreWave(bool i_isIgnoreWave)
	{
		m_isIgnoreWave = i_isIgnoreWave;
	}

	public bool IsIgnoreWave()
	{
		return m_isIgnoreWave;
	}

	public int GetId()
	{
		return m_id;
	}

	public void SetId(int i_id)
	{
		m_id = i_id;
	}

	public void ConfigureModEnemy(string i_displayName, string i_description, EnemyStatsDefinition i_stats)
	{
		SetId(-1);
		ConfigureModIdentity(i_displayName);
		m_description = i_description ?? string.Empty;
		if (i_stats == null) return;
		if (i_stats.Bounty.HasValue) m_bounty = i_stats.Bounty.Value;
		if (i_stats.HealthIncreasePerWave.HasValue) m_amountIncreaseHealthMaxPerWave = i_stats.HealthIncreasePerWave.Value;
		ConfigureModStats(i_stats);
	}

	public Stage GetStageAppearance()
	{
		return m_stageAppearance;
	}

	public List<Interaction> GetAllInteractions()
	{
		return m_interactions;
	}

	public bool IsPlayerParent()
	{
		return m_isPlayerParent;
	}

	public void SetIsPlayerParent(bool i_isPlayerParent)
	{
		m_isPlayerParent = i_isPlayerParent;
	}

	public float GetTimeSpawn()
	{
		return m_timeSpawn;
	}
}
