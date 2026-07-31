using UnityEngine;

public class HeadHumper : Walker
{
	public delegate void DelLetGo();

	[SerializeField]
	private Vector3 m_offsetHeadHugging;

	[SerializeField]
	private Vector3 m_offsetRotationHeadHugging;

	[SerializeField]
	private ParticleSystem m_particleEmptySack;

	[SerializeField]
	private AudioClip m_audioEmptySack;

	[SerializeField]
	private AudioClip m_audioEmptySackImpregnate;

	[SerializeField]
	private AudioClip m_audioScreechIgnite;

	private StatusPlayerHudItem m_statusEmptySack;

	private bool m_isHuggingHead;

	private int m_timesEmptiedSack;

	public event DelLetGo OnLetGo;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIHeadHumper>();
		m_xAI.Initialize(this);
	}

	public void HugHead()
	{
		if ((bool)CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetComponentInChildren<HeadHumper>())
		{
			CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
				.GetComponentInChildren<HeadHumper>()
				.LetGoOfHead();
		}
		m_isHuggingHead = true;
		m_timesEmptiedSack = 0;
		SetIsThinking(i_isThinking: false);
		GetRigidbody2D().isKinematic = true;
		GetSkeletonActor().SetToRape();
		GetSkeleton().DisableCollisionOnAllBodyParts();
		m_isIgnoreWave = true;
		CommonReferences.Instance.GetPlayer().SetIsMute(i_isMute: true);
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetManagerFacePlayer()
			.GetManagerPlayerMouth()
			.SetIsHeadHumping(i_isHeadHumping: true);
		m_animator.Play("HeadHugging");
		base.transform.parent = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.transform;
		base.transform.localPosition = m_offsetHeadHugging;
		base.transform.localEulerAngles = m_offsetRotationHeadHugging;
		base.transform.localScale = Vector3.one;
		m_statusEmptySack = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus(GetName() + " Face Breeding", "The " + GetName() + "'s liquid is making you woozy... it needs to get off!", StatusPlayerHudItemColor.Lewd);
		CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().HideNpcHealthDisplay(this);
		CommonReferences.Instance.GetPlayer().OnBeingRaped += OnPlayerBeingRaped;
		m_interactions[0].Trigger(this);
		CHLamarr cHLamarr = (CHLamarr)CommonReferences.Instance.GetManagerChallenge().GetChallenge("Lamarr");
		if (cHLamarr.IsActive())
		{
			cHLamarr.SetHeadHumperHumping(this);
		}
	}

	private void OnPlayerBeingRaped()
	{
		CommonReferences.Instance.GetPlayer().OnRapeEnd += OnPlayerRapeEnd;
	}

	private void OnPlayerRapeEnd()
	{
		CommonReferences.Instance.GetPlayer().OnRapeEnd -= OnPlayerRapeEnd;
	}

	private void OnAdvanceRaperAnimation()
	{
		if (CommonReferences.Instance.GetPlayer().GetRaperCurrent().GetIsHasOral())
		{
			CommonReferences.Instance.GetPlayer().GetRaperCurrent().OnThrust += LetGoOfHead;
			CommonReferences.Instance.GetPlayer().GetRaperCurrent().OnCumThrust += LetGoOfHead;
		}
	}

	public void LetGoOfHead()
	{
		m_isHuggingHead = false;
		CommonReferences.Instance.GetPlayer().OnBeingRaped -= OnPlayerBeingRaped;
		GetRaper().OnThrust -= LetGoOfHead;
		GetRaper().OnCumThrust -= LetGoOfHead;
		SetIsThinking(i_isThinking: true);
		GetRigidbody2D().isKinematic = false;
		GetSkeletonActor().SetToDefault();
		GetSkeleton().EnableCollisionOnAllBodyParts();
		m_isIgnoreWave = false;
		CommonReferences.Instance.GetPlayer().SetIsMute(i_isMute: false);
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetManagerFacePlayer()
			.GetManagerPlayerMouth()
			.SetIsHeadHumping(i_isHeadHumping: false);
		m_animator.Play("Idle");
		base.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
			.transform;
		base.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().DestroyStatusItem(m_statusEmptySack);
		CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().ShowNpcHealthDisplay(this);
		this.OnLetGo?.Invoke();
	}

	public void EmptySack()
	{
		if (!m_isHuggingHead)
		{
			PlayAudioSFX(m_audioEmptySack);
			return;
		}
		Player player = CommonReferences.Instance.GetPlayer();
		player.ApplyStatusEffect(GetEffectEmptySack());
		ParticleSystem particleSystem = CommonReferences.Instance.GetUtilityTools().CreateParticleDuplicateAndPlay(m_particleEmptySack, player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Head).transform);
		particleSystem.transform.localPosition = Vector3.zero;
		particleSystem.transform.localEulerAngles = Vector3.zero;
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, particleSystem.main.duration + 5f);
		m_timesEmptiedSack++;
		if (m_timesEmptiedSack == 10)
		{
			CommonReferences.Instance.GetPlayer().CreateAndAddFetus(this, Library.Instance.Actors.GetActor("Head Humper"), 90f, "Head Humper");
			PlayAudioSFX(m_audioEmptySackImpregnate);
			m_timesEmptiedSack = 0;
		}
		else
		{
			PlayAudioSFX(m_audioEmptySack);
		}
		ManagerDB.AddLitreCumTaken(this, 0.01f);
	}

	private SELibidoIncrease GetEffectEmptySack()
	{
		return new SELibidoIncrease(GetName() + " Empty Sack", GetName() + GetInstanceID(), TypeStatusEffect.Negative, 1f, i_isStackable: true, 20f, 0.1f);
	}

	public ParticleSystem GetParticleEmptySack()
	{
		return m_particleEmptySack;
	}

	public bool IsHeadHugging()
	{
		return m_isHuggingHead;
	}

	public void ScreechIgnite()
	{
		PlayAudioVoice(m_audioScreechIgnite);
	}
}
