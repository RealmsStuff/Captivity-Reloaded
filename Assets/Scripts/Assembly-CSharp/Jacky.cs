using System.Collections;
using UnityEngine;

public class Jacky : Android
{
	[SerializeField]
	private float m_distanceMinVisible = 3f;

	[SerializeField]
	private float m_distanceMaxVisible = 10f;

	[SerializeField]
	private AudioClip m_audioScare;

	[SerializeField]
	private float m_distanceCanScare = 10f;

	[SerializeField]
	private AudioClip m_audioReverberation;

	private bool m_isHasScaredAlready;

	private bool m_isScaring;

	private AudioSource m_audioSourceReverberation;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIJacky>();
		m_xAI.Initialize(this);
	}

	public override void FixedUpdate()
	{
		if (GetRaper().GetIsRaping())
		{
			StopAudioReverberation();
			m_isHasScaredAlready = true;
		}
		base.FixedUpdate();
	}

	public override void Start()
	{
		base.Start();
		AddReverberation();
		CommonReferences.Instance.GetPlayer().OnDie += OnPlayerDie;
	}

	private void OnPlayerDie()
	{
		StopAudioReverberation();
	}

	private void AddReverberation()
	{
		m_audioSourceReverberation = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
		m_audioSourceReverberation.minDistance = 0.5f;
		m_audioSourceReverberation.maxDistance = 18f;
		m_audioSourceReverberation.loop = true;
		m_audioSourceReverberation.clip = m_audioReverberation;
		m_audioSourceReverberation.Play();
	}

	public void StartAudioReverberation()
	{
		if (!m_isHasScaredAlready && !m_audioSourceReverberation.isPlaying)
		{
			m_audioSourceReverberation.Play();
		}
	}

	public void StopAudioReverberation()
	{
		if (m_audioReverberation != null && m_audioSourceReverberation != null && m_audioSourceReverberation.isPlaying)
		{
			m_audioSourceReverberation.Stop();
		}
	}

	public override void Spawn(bool i_isFadeIn)
	{
		base.Spawn(i_isFadeIn: false);
		StartCoroutine(CoroutineHandleInvisibility());
		CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().HideNpcHealthDisplay(this);
	}

	private IEnumerator CoroutineHandleInvisibility()
	{
		SpriteRenderer[] l_sprites = GetComponentsInChildren<SpriteRenderer>();
		bool l_isHidden = false;
		while (!m_isHasScaredAlready && !IsDead())
		{
			float num = m_distanceMaxVisible - m_distanceMinVisible;
			float num2 = GetDistanceBetweenPlayerHips() - m_distanceMinVisible;
			float num3 = 1f - num2 / num;
			for (int i = 0; i < l_sprites.Length; i++)
			{
				Color color = l_sprites[i].color;
				color.a = num3;
				l_sprites[i].color = color;
			}
			if (num3 > 0f)
			{
				if (l_isHidden)
				{
					l_isHidden = false;
					SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: false);
					CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().ShowNpcHealthDisplay(this);
				}
			}
			else if (!l_isHidden)
			{
				l_isHidden = true;
				SetIsInvulnerable(i_isInvulnerable: true, i_isAffectSkeleton: false);
				CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().HideNpcHealthDisplay(this);
			}
			yield return new WaitForFixedUpdate();
		}
		for (int j = 0; j < l_sprites.Length; j++)
		{
			Color color2 = l_sprites[j].color;
			color2.a = 1f;
			l_sprites[j].color = color2;
		}
		SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: false);
		CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().ShowNpcHealthDisplay(this);
	}

	public void Scare()
	{
		if (!(GetDistanceBetweenPlayerHips() > m_distanceCanScare))
		{
			m_isHasScaredAlready = true;
			m_isScaring = true;
			m_animator.Play("Scare");
			CommonReferences.Instance.GetPlayer().Fear(2.25f);
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioScare);
			StopAudioReverberation();
			m_interactions[0].Trigger(this);
			m_isThinking = false;
			StartCoroutine(CoroutineWaitForEndScare());
		}
	}

	private IEnumerator CoroutineWaitForEndScare()
	{
		float seconds = 0.5f;
		AnimationClip[] animationClips = GetAnimator().runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name == "Scare")
			{
				seconds = animationClip.length;
				break;
			}
		}
		yield return new WaitForSeconds(seconds);
		m_isScaring = false;
		m_isThinking = true;
	}

	public bool IsScaring()
	{
		return m_isScaring;
	}

	public float GetDistanceCanScare()
	{
		return m_distanceCanScare;
	}

	public bool IsHasScaredAlready()
	{
		return m_isHasScaredAlready;
	}

	public override void Die()
	{
		base.Die();
		StopAudioReverberation();
	}
}
