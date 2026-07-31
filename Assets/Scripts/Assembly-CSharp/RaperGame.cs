using System.Collections;
using UnityEngine;

public abstract class RaperGame : Raper
{
	public delegate void OnLoseDel();

	public delegate void OnWinDel();

	public delegate void OnGiveUpDel();

	[Header("---Raper Game---")]
	[SerializeField]
	protected RaperAnimation m_raperAnimToLoseOn;

	[SerializeField]
	protected bool m_isDestroySelfAfterWin;

	[SerializeField]
	protected bool m_isDestroySelfAfterFail;

	[SerializeField]
	protected bool m_isCooldownAfterWin;

	[SerializeField]
	protected bool m_isCooldownAfterFail;

	[SerializeField]
	protected float m_durationCooldownRaperGame;

	protected float m_escapePowerPlayer01;

	protected float m_difficulty02;

	protected AudioClip m_audioStartRaperGame;

	private AudioClip m_audioWin;

	private AudioClip m_audioLose;

	protected bool m_isLose;

	protected bool m_isDone;

	protected bool m_isStarted;

	protected float m_timeToEscape;

	private float m_timeLeftCooldownRape;

	private bool m_isWaitingForCooldownRape;

	private Coroutine m_coroutineWaitForCooldownRape;

	private Coroutine m_coroutineAnimateWaitForCooldownRape;

	public event OnLoseDel OnLose;

	public event OnWinDel OnWin;

	public event OnGiveUpDel OnGiveUp;

	protected override void Start()
	{
		base.Start();
		m_escapePowerPlayer01 = 1f;
		m_difficulty02 = 0.45f;
		m_audioWin = Resources.Load<AudioClip>("Audio/RapeWin");
		m_audioLose = Resources.Load<AudioClip>("Audio/RaperGameLose");
		if (m_isDestroySelfAfterWin)
		{
			base.OnEndRape += DestroySelfOnWin;
		}
		if (m_isDestroySelfAfterFail)
		{
			base.OnEndRape += DestroySelfOnFail;
		}
		CalculateTimeToEscape();
		m_player.OnDie += OnPlayerDie;
		base.OnEndRape += HandleEndRape;
	}

	private void CalculateTimeToEscape()
	{
		int num = 0;
		int num2 = 0;
		foreach (RaperAnimation raperAnimation in m_raperAnimations)
		{
			if (raperAnimation == m_raperAnimToLoseOn)
			{
				num = num2;
				break;
			}
			num2++;
		}
		float num3 = 0f;
		for (num2 = 0; num2 < num; num2++)
		{
			num3 += m_raperAnimations[num2].GetDurationSecondsAnimClipRaper();
		}
		m_timeToEscape = num3;
	}

	private void OnEnable()
	{
		if (m_isWaitingForCooldownRape)
		{
			StartCoroutine(CoroutineWaitForCooldownRape());
		}
	}

	public override void BeginRape()
	{
		base.BeginRape();
		SetStartAudioRaperGame();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioStartRaperGame);
	}

	protected override void HandleRape()
	{
		if (m_player.IsDead())
		{
			GiveUp();
		}
	}

	protected abstract void SetStartAudioRaperGame();

	protected abstract void ShowGameOverlay();

	protected abstract void HideGameOverlay();

	protected virtual void Lose()
	{
		if (this.OnLose != null)
		{
			this.OnLose();
		}
		m_isLose = true;
		m_isDone = true;
		m_isStarted = false;
		HideGameOverlay();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioWin);
		EndRape();
		m_player.EscapeFromRape();
		m_player.DisableRagdoll();
	}

	private void OnPlayerDie()
	{
		m_player.OnDie -= OnPlayerDie;
		if (m_isRaping)
		{
			Win();
		}
	}

	protected void Win()
	{
		if (!m_isDone)
		{
			if (this.OnWin != null)
			{
				this.OnWin();
			}
			m_isDone = true;
			m_isLose = false;
			m_isStarted = false;
			HideGameOverlay();
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioLose);
		}
	}

	private void GiveUp()
	{
		if (this.OnGiveUp != null)
		{
			this.OnGiveUp();
		}
		m_npc.GetAnimator().Play(GetNameAnimRapeNPC());
	}

	protected virtual void HandleEndRape()
	{
		Win();
	}

	private void DestroySelfOnWin()
	{
		if (!m_isLose)
		{
			m_npc.DieRapeWin();
			Object.Destroy(base.gameObject);
		}
	}

	private void DestroySelfOnFail()
	{
		if (m_isLose)
		{
			m_npc.Die();
			Object.Destroy(base.gameObject);
		}
	}

	protected override void RecoverFromRape()
	{
		if (m_isCooldownAfterWin && !m_isLose)
		{
			StartCooldownAfterRape();
		}
		else if (m_isCooldownAfterFail && m_isLose)
		{
			StartCooldownAfterRape();
		}
	}

	protected void StartCooldownAfterRape()
	{
		m_coroutineWaitForCooldownRape = StartCoroutine(CoroutineWaitForCooldownRape());
		m_coroutineAnimateWaitForCooldownRape = StartCoroutine(CoroutineAnimateWaitForCooldownRape());
	}

	protected IEnumerator CoroutineWaitForCooldownRape()
	{
		m_npc.SetIsThinking(i_isThinking: false);
		m_npc.SetIsCanAttack(i_isCanAttack: false);
		m_timeLeftCooldownRape = m_durationCooldownRaperGame;
		m_isWaitingForCooldownRape = true;
		m_npc.OnGetHit += InterruptWaitForCooldownRape;
		while (m_timeLeftCooldownRape > 0f)
		{
			yield return new WaitForEndOfFrame();
			float deltaTime = Time.deltaTime;
			m_timeLeftCooldownRape -= deltaTime;
		}
		if (m_coroutineAnimateWaitForCooldownRape != null)
		{
			StopCoroutine(m_coroutineAnimateWaitForCooldownRape);
		}
		m_npc.OnGetHit -= InterruptWaitForCooldownRape;
		m_isWaitingForCooldownRape = false;
		m_npc.SetIsThinking(i_isThinking: true);
		m_npc.SetIsCanAttack(i_isCanAttack: true);
		m_isCanRape = true;
	}

	protected void InterruptWaitForCooldownRape(Actor i_attacker, Actor i_victim)
	{
		StopCoroutine(m_coroutineWaitForCooldownRape);
		StopCoroutine(m_coroutineAnimateWaitForCooldownRape);
		m_npc.OnGetHit -= InterruptWaitForCooldownRape;
		m_isWaitingForCooldownRape = false;
		m_npc.SetIsThinking(i_isThinking: true);
		m_npc.SetIsCanAttack(i_isCanAttack: true);
		m_isCanRape = true;
	}

	private IEnumerator CoroutineAnimateWaitForCooldownRape()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.15f);
			yield return new WaitForSeconds(0.15f);
		}
	}

	protected override void RecoverVictimFromRape()
	{
		if (!m_isLose)
		{
			m_player.RecoverFromRape();
		}
	}

	public float GetTimeMax()
	{
		return m_timeToEscape;
	}
}
