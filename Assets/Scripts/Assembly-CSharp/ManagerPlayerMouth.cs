using System;
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

	private Dictionary<string, List<Action<Sprite>>> m_coreMouthBindings;
	private Dictionary<string, Sprite> m_coreMouthBaselines;

	internal IReadOnlyDictionary<string, Sprite> GetCoreMouthSprites()
	{
		EnsureCoreMouthBindings();
		return m_coreMouthBaselines;
	}

	internal void SetCoreMouthSprite(string i_originalName, Sprite i_replacement)
	{
		EnsureCoreMouthBindings();
		if (i_replacement == null || !m_coreMouthBindings.TryGetValue(i_originalName, out List<Action<Sprite>> bindings)) return;
		foreach (Action<Sprite> binding in bindings) binding(i_replacement);
	}

	private void EnsureCoreMouthBindings()
	{
		if (m_coreMouthBindings != null) return;
		m_coreMouthBindings = new Dictionary<string, List<Action<Sprite>>>(StringComparer.Ordinal);
		m_coreMouthBaselines = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		AddCoreMouthBinding(m_mouthTakeHit, sprite => m_mouthTakeHit = sprite);
		AddCoreMouthBinding(m_mouthKO, sprite => m_mouthKO = sprite);
		AddCoreMouthBinding(m_mouthDead, sprite => m_mouthDead = sprite);
		AddCoreMouthBinding(m_mouthVoiceRape, sprite => m_mouthVoiceRape = sprite);
		AddCoreMouthList(m_mouthsIdle);
		AddCoreMouthList(m_mouthsIdleRape);
		AddCoreMouthList(m_mouthsThrust);
		AddCoreMouthList(m_mouthsCumThrust);
		AddCoreMouthList(m_mouthsVoiceOrgasm);
		AddCoreMouthList(m_mouthsOral);
		if (m_mouth != null && m_mouth.sprite != null)
		{
			SpriteRenderer renderer = m_mouth;
			AddCoreMouthBinding(renderer.sprite, sprite => { if (renderer != null) renderer.sprite = sprite; });
		}
	}

	private void AddCoreMouthList(List<Sprite> i_sprites)
	{
		if (i_sprites == null) return;
		for (int index = 0; index < i_sprites.Count; index++)
		{
			int boundIndex = index;
			AddCoreMouthBinding(i_sprites[index], sprite => i_sprites[boundIndex] = sprite);
		}
	}

	private void AddCoreMouthBinding(Sprite i_sprite, Action<Sprite> i_setter)
	{
		if (i_sprite == null || i_setter == null || string.IsNullOrEmpty(i_sprite.name)) return;
		if (!m_coreMouthBindings.TryGetValue(i_sprite.name, out List<Action<Sprite>> bindings))
		{
			bindings = new List<Action<Sprite>>();
			m_coreMouthBindings.Add(i_sprite.name, bindings);
			m_coreMouthBaselines.Add(i_sprite.name, i_sprite);
		}
		bindings.Add(i_setter);
	}

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
