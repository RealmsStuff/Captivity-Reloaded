using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinMinor : Walker
{
	[SerializeField]
	private GameObject m_stick;

	[SerializeField]
	private GameObject m_syringe;

	[SerializeField]
	private AudioClip m_audioStatusEffectSyringe;

	private bool m_isAttackingSyringe;

	private bool m_isHasUsedSyringe;

	private List<StatModifier> m_modifiersAttackSyringe = new List<StatModifier>();

	private Coroutine m_coroutineFlyAttackRagdoll;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIGoblinMinor>();
		m_xAI.Initialize(this);
	}

	public void FlyAttackRagdoll()
	{
		m_coroutineFlyAttackRagdoll = StartCoroutine(CoroutineFlyAttackRagdoll());
	}

	private IEnumerator CoroutineFlyAttackRagdoll()
	{
		if (GetIsFacingLeft())
		{
			GetRigidbody2D().AddForce(new Vector2(-10f, 10f), ForceMode2D.Impulse);
			GetRigidbody2D().AddTorque(-100f);
		}
		else
		{
			GetRigidbody2D().AddForce(new Vector2(10f, 10f), ForceMode2D.Impulse);
			GetRigidbody2D().AddTorque(100f);
		}
		Timer l_timer = new Timer(0.33f);
		yield return l_timer.CoroutinePlayAndWaitForEnd();
		bool flag = false;
		if (CommonReferences.Instance.GetPlayer().GetIsBeingRaped() && CommonReferences.Instance.GetPlayer().GetRaperCurrent() == GetRaper())
		{
			flag = true;
		}
		if (!flag && !IsDead())
		{
			Ragdoll(1f);
			SetIsThinking(i_isThinking: false);
			if (GetIsFacingLeft())
			{
				GetSkeletonActor().GetBoneHips().GetRigidbody2D().AddTorque(-300f);
			}
			else
			{
				GetSkeletonActor().GetBoneHips().GetRigidbody2D().AddTorque(300f);
			}
			l_timer.SetDurationAndResetTimer(1f);
			yield return l_timer.CoroutinePlayAndWaitForEnd();
			m_animator.Play("GetUp");
			l_timer.SetDurationAndResetTimer(1f);
			yield return l_timer.CoroutinePlayAndWaitForEnd();
			SetIsThinking(i_isThinking: true);
		}
	}

	public void StickNeedle()
	{
		CommonReferences.Instance.GetPlayer().ApplyStatusEffect(GetStatusEffectSyringe());
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectStrangeLiquid();
		GetComponentInParent<GoblinMinor>().PlayAudioSFX(Resources.Load<AudioClip>("Audio\\Syringe"));
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(GetAudioStatusEffectSyringe());
		GetAllInteractions()[0].Trigger(this);
	}

	public GameObject GetStick()
	{
		return m_stick;
	}

	public GameObject GetSyringe()
	{
		return m_syringe;
	}

	public SELibidoIncrease GetStatusEffectSyringe()
	{
		SELibidoIncrease sELibidoIncrease = new SELibidoIncrease("Strange Liquid", GetName() + base.gameObject.GetInstanceID(), TypeStatusEffect.Negative, 10f, i_isStackable: true, 8f, 0.25f);
		sELibidoIncrease.AddPlayerStatusHudItem("Strange Liquid", "The little goblin injected some odd liquid into you... it's making you... woozy", StatusPlayerHudItemColor.Lewd);
		return sELibidoIncrease;
	}

	public AudioClip GetAudioStatusEffectSyringe()
	{
		return m_audioStatusEffectSyringe;
	}

	public bool IsAttackingSyringe()
	{
		return m_isAttackingSyringe;
	}

	public bool IsHasUsedSyringe()
	{
		return m_isHasUsedSyringe;
	}

	public override void DropAllPickupAbles()
	{
		StageJungle stageJungle = (StageJungle)CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		if (!stageJungle.IsFetishDropped(m_pickUpablesToDrop[0]))
		{
			stageJungle.SetFetishDropped(m_pickUpablesToDrop[0]);
			base.DropAllPickupAbles();
		}
	}
}
