using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPlayerMouth : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer m_mouth;

	[SerializeField]
	private Sprite m_mouthTakeHit;

	[SerializeField]
	private Sprite m_mouthKO;

	[SerializeField]
	private Sprite m_mouthDead;

	[Header("Item 1 = 0-24 tiredness, item 2 = 25-49, item 3 = 50 - 74, item 4 = 75 - 100")]
	[SerializeField]
	private List<Sprite> m_mouthsIdle = new List<Sprite>();

	[Header("---Rape---")]
	[SerializeField]
	private Sprite m_mouthVoiceRape;

	[Header("Item 1 = 0-24 libido, item 2 = 25-49, item 3 = 50 - 74, item 4 = 75 - 100")]
	[SerializeField]
	private List<Sprite> m_mouthsIdleRape = new List<Sprite>();

	[Header("Item 1 = 0-24 pleasure, item 2 = 25-49, item 3 = 50 - 74, item 4 = 75 - 100")]
	[SerializeField]
	private List<Sprite> m_mouthsThrust = new List<Sprite>();

	[SerializeField]
	private List<Sprite> m_mouthsCumThrust = new List<Sprite>();

	[SerializeField]
	private List<Sprite> m_mouthsVoiceOrgasm = new List<Sprite>();

	[Header("Each item is a bigger and bigger mouth")]
	[SerializeField]
	private List<Sprite> m_mouthsOral = new List<Sprite>();

	private Player m_player;

	private Raper m_raper;

	private bool m_isKO;

	private bool m_isHeadHumping;

	private Coroutine m_coroutineChangeMouthToIdle;

	private void Start()
	{
		m_player = CommonReferences.Instance.GetPlayer();
	}

	public void TakeDamage()
	{
		SetMouth(m_mouthTakeHit, 0.35f);
	}

	public void ApplyRaper(Raper i_raper)
	{
		m_raper = i_raper;
		AddListenersToRaper();
		m_raper.OnEndRape += RemoveListenersFromRaper;
	}

	private void AddListenersToRaper()
	{
		m_raper.OnPlayerVoice += OnPlayerVoice;
		m_raper.OnThrust += OnThrust;
		m_raper.OnCumThrust += OnCumThrust;
		m_player.OnOrgasm += OnOrgasm;
	}

	private void RemoveListenersFromRaper()
	{
		m_raper.OnPlayerVoice -= OnPlayerVoice;
		m_raper.OnThrust -= OnThrust;
		m_raper.OnCumThrust -= OnCumThrust;
		m_player.OnOrgasm -= OnOrgasm;
		m_raper.OnEndRape -= RemoveListenersFromRaper;
		m_raper = null;
	}

	private void OnPlayerVoice()
	{
		if (m_raper.GetIsHasOral())
		{
			m_mouth.sprite = m_mouthsOral[0];
		}
		else
		{
			SetMouth(m_mouthVoiceRape, 0.5f);
		}
	}

	private void OnThrust()
	{
		if (m_raper.GetIsHasOral())
		{
			m_mouth.sprite = m_mouthsOral[0];
			return;
		}
		int index = 0;
		if (m_player.GetPleasureCurrent() >= 25f)
		{
			index = 1;
		}
		if (m_player.GetPleasureCurrent() >= 50f)
		{
			index = 2;
		}
		if (m_player.GetPleasureCurrent() >= 75f)
		{
			index = 3;
		}
		SetMouth(m_mouthsThrust[index], 0.25f);
	}

	private void OnCumThrust()
	{
		if (m_raper.GetIsHasOral())
		{
			m_mouth.sprite = m_mouthsOral[0];
			return;
		}
		int index = 0;
		if (m_player.GetPleasureCurrent() >= 25f)
		{
			index = 1;
		}
		if (m_player.GetPleasureCurrent() >= 50f)
		{
			index = 2;
		}
		if (m_player.GetPleasureCurrent() >= 75f)
		{
			index = 3;
		}
		SetMouth(m_mouthsCumThrust[index], 0.25f);
	}

	private void OnOrgasm()
	{
		if (m_raper.GetIsHasOral())
		{
			m_mouth.sprite = m_mouthsOral[0];
			return;
		}
		int index = 0;
		if (m_player.GetPleasureCurrent() >= 25f)
		{
			index = 1;
		}
		if (m_player.GetPleasureCurrent() >= 50f)
		{
			index = 2;
		}
		if (m_player.GetPleasureCurrent() >= 75f)
		{
			index = 3;
		}
		SetMouth(m_mouthsVoiceOrgasm[index], 0.75f);
	}

	private void SetMouth(Sprite i_sprite, float i_secsDuration)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (m_isHeadHumping)
		{
			m_mouth.sprite = m_mouthsOral[0];
		}
		else
		{
			if (m_isKO)
			{
				return;
			}
			if (m_player.IsMute())
			{
				m_mouth.sprite = m_mouthsOral[0];
				return;
			}
			m_player = CommonReferences.Instance.GetPlayer();
			if (!m_mouthsVoiceOrgasm.Contains(m_mouth.sprite))
			{
				m_mouth.sprite = i_sprite;
				if (m_coroutineChangeMouthToIdle != null)
				{
					StopCoroutine(m_coroutineChangeMouthToIdle);
				}
				m_coroutineChangeMouthToIdle = StartCoroutine(CoroutineChangeMouthToIdle(i_secsDuration));
			}
		}
	}

	private IEnumerator CoroutineChangeMouthToIdle(float i_secondsToWait)
	{
		yield return new WaitForSeconds(i_secondsToWait);
		if (!m_isKO)
		{
			m_mouth.sprite = GetMouthIdle();
		}
	}

	private Sprite GetMouthIdle()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		if (m_player.IsDead())
		{
			return m_mouthDead;
		}
		int index = 0;
		if (m_player.GetIsBeingRaped())
		{
			if (m_player.GetLibidoCurrent() >= 25f)
			{
				index = 1;
			}
			if (m_player.GetLibidoCurrent() >= 50f)
			{
				index = 2;
			}
			if (m_player.GetLibidoCurrent() >= 75f)
			{
				index = 3;
			}
			return m_mouthsIdleRape[index];
		}
		if (m_player.GetHealthCurrent() <= m_player.GetStat("HealthMax").GetValueTotal() / 4f * 3f - 1f)
		{
			index = 1;
		}
		if (m_player.GetHealthCurrent() <= m_player.GetStat("HealthMax").GetValueTotal() / 4f * 2f - 1f)
		{
			index = 2;
		}
		if (m_player.GetHealthCurrent() <= m_player.GetStat("HealthMax").GetValueTotal() / 4f * 1f - 1f)
		{
			index = 3;
		}
		return m_mouthsIdle[index];
	}

	public void MouthWakeUpGameOver()
	{
		SetMouth(m_mouthVoiceRape, 3f);
	}

	public void KO()
	{
		m_isKO = true;
		if (!m_isHeadHumping)
		{
			m_mouth.sprite = m_mouthKO;
		}
	}

	public void UnKO()
	{
		m_isKO = false;
		if (!m_isHeadHumping)
		{
			m_mouth.sprite = m_mouthsIdle[0];
		}
	}

	public void SetIsHeadHumping(bool i_isHeadHumping)
	{
		m_isHeadHumping = i_isHeadHumping;
		if (m_isHeadHumping)
		{
			m_mouth.sprite = m_mouthsOral[0];
		}
	}
}
