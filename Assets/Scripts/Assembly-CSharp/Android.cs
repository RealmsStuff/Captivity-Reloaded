using System.Collections;
using UnityEngine;

public class Android : Walker
{
	[SerializeField]
	private float m_chanceToDodge01;

	[SerializeField]
	private float m_speedIncreaseDodge;

	[SerializeField]
	private AudioClip m_audioDodge;

	private StatModifier m_modifierDodge;

	private bool m_isDodging;

	private bool m_isHitBySchockGewehr;

	private Coroutine m_coroutineDodge;

	private Coroutine m_coroutineWaitBeforeCanDodgeAgain;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIAndroid>();
		m_xAI.Initialize(this);
	}

	public override bool TakeHitBullet(Actor i_initiator, Bullet i_bullet, BodyPartActor i_bodyPart)
	{
		bool flag = base.TakeHitBullet(i_initiator, i_bullet, i_bodyPart);
		if (CommonReferences.Instance.GetPlayer().GetEquippableEquipped().GetName() == "Schockgewehr" && flag && !m_isHitBySchockGewehr)
		{
			Die();
			m_isHitBySchockGewehr = true;
			Ragdoll(5f);
			GetSkeleton().EnableElectrocute(2f, new Color(0.5f, 0.5f, 1f));
			StartCoroutine(CoroutineWaitBeforeExplode());
		}
		return flag;
	}

	private IEnumerator CoroutineWaitBeforeExplode()
	{
		yield return new WaitForSeconds(3f);
		int num = 3;
		int num2 = 0;
		BodyPartAndroid[] componentsInChildren = GetComponentsInChildren<BodyPartAndroid>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Explode();
			num2++;
			if (num2 >= num)
			{
				break;
			}
		}
		yield return new WaitForSeconds(1f);
		GetSkeletonActor().DisableElectrocute();
	}

	public void Dodge()
	{
		m_isDodging = true;
		m_animator.Play("Dodge", 0, 0f);
		if (m_coroutineDodge != null)
		{
			StopCoroutine(m_coroutineDodge);
			RemoveStatModifier(m_modifierDodge);
		}
		if (m_coroutineWaitBeforeCanDodgeAgain != null)
		{
			m_isDodging = false;
			m_coroutineWaitBeforeCanDodgeAgain = null;
		}
		m_modifierDodge = AddStatModifier("SpeedMax", m_speedIncreaseDodge);
		m_coroutineDodge = StartCoroutine(CoroutineDodge());
		m_coroutineWaitBeforeCanDodgeAgain = StartCoroutine(CoroutineWaitBeforeCanDodgeAgain());
		PlayAudioSFX(m_audioDodge);
	}

	private IEnumerator CoroutineDodge()
	{
		yield return new WaitForSeconds(0.1f);
		RemoveStatModifier(m_modifierDodge);
		m_coroutineDodge = null;
	}

	private IEnumerator CoroutineWaitBeforeCanDodgeAgain()
	{
		yield return new WaitForSeconds(0.25f);
		m_isDodging = false;
		m_coroutineWaitBeforeCanDodgeAgain = null;
	}

	private void Electrocute()
	{
	}

	public float GetChangeToDodge()
	{
		return m_chanceToDodge01;
	}

	public bool IsDodging()
	{
		return m_isDodging;
	}
}
