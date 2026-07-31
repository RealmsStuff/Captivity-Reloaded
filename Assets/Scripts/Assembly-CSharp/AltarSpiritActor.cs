using System.Collections;
using UnityEngine;


public class AltarSpiritActor : NPC
{
	[SerializeField]
	private AudioClip m_audioBirth;

	public override void Start()
	{
	}

	protected override void OnEnable()
	{
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioBirth);
		KillAllNpcs();
		StartCoroutine(CoroutineFlyAway());
		StartCoroutine(CoroutineReleaseSpirit());
		SetIsIgnoreWave(i_isIgnoreWave: true);
	}

	private IEnumerator CoroutineReleaseSpirit()
	{
		UnityEngine.Rendering.Universal.Light2D l_lightGlobal = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetLightGlobal();
		float l_weightRFrom = 0.5f;
		float l_weightRTo = l_lightGlobal.color.r;
		float l_weightGFrom = 0f;
		float l_weightGTo = l_lightGlobal.color.g;
		float l_weightBFrom = 1f;
		float l_weightBTo = l_lightGlobal.color.b;
		float l_timeToMove = 5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float r = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightRFrom, l_weightRTo, i_time);
			float g = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightGFrom, l_weightGTo, i_time);
			float b = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightBFrom, l_weightBTo, i_time);
			l_lightGlobal.color = new Color(r, g, b);
			yield return new WaitForFixedUpdate();
		}
	}

	private void KillAllNpcs()
	{
		foreach (NPC allNPC in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs())
		{
			if (!allNPC.IsDead() && !(allNPC == this))
			{
				allNPC.Die();
			}
		}
	}

	private IEnumerator CoroutineFlyAway()
	{
		float l_secsToMove = 10f;
		for (float l_secsPassed = 0f; l_secsPassed < l_secsToMove; l_secsPassed += Time.fixedDeltaTime)
		{
			SetPosY(GetPos().y + 0.005f);
			yield return new WaitForFixedUpdate();
		}
		Object.Destroy(base.gameObject);
	}

	public override void UpdateAnim()
	{
	}

	public override void TakeDamage(float i_amount)
	{
	}

	protected override void AddXAIComponent()
	{
	}

	public override void MoveToPlayer()
	{
	}

	public override void MoveAwayFromPlayer()
	{
	}

	public override void Awake()
	{
	}

	public override void Spawn(bool i_isFadeIn)
	{
	}

	public override void FixedUpdate()
	{
	}
}
