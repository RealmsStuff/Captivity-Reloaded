using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LegacyShackAberrantType
{
	Tank = 1,
	Boomer = 2,
	PinkHound = 3
}

// Compatibility behavior for Clothing Overhaul's ShackOverhaul DLL. Existing
// enemies are promoted in-place, matching the original mod's timed manager.
public sealed class LegacyShackAberrant : MonoBehaviour
{
	private NPC m_npc;
	private int m_hits;
	private bool m_initialized;
	private Coroutine m_boomer;
	private Coroutine m_boomerFlash;
	private bool m_boomerFlashed;
	private bool m_boomerExplodes = true;
	private readonly List<StatModifier> m_boomerModifiers = new List<StatModifier>();

	public LegacyShackAberrantType Type { get; private set; }

	public void Initialize(LegacyShackAberrantType i_type, bool i_boomerExplodes = true)
	{
		m_npc = GetComponent<NPC>();
		Type = i_type;
		m_boomerExplodes = i_boomerExplodes;
		if (Type == LegacyShackAberrantType.PinkHound)
			Tint(new Color(0.8f, 0f, 1f));
	}

	public bool HandleBullet(Bullet i_bullet, BodyPartActor i_bodyPart)
	{
		if (m_npc == null || Type == LegacyShackAberrantType.PinkHound) return false;
		float damage = i_bullet.GetDamage() * i_bodyPart.GetDamageMultiplierAmountBodyPart();
		float distance = Mathf.Abs(CommonReferences.Instance.GetPlayer().transform.position.x - transform.position.x);
		if (Type == LegacyShackAberrantType.Tank)
		{
			if (!m_initialized)
			{
				m_npc.AddStatModifier("SpeedAccel", 1.25f);
				m_npc.AddStatModifier("SpeedMax", 1.25f);
				float baseHealth = m_npc.GetStat("HealthMax").GetValueBase();
				m_npc.AddStatModifier("HealthMax", baseHealth * 2f);
				float healthDelta = baseHealth * 2f - m_npc.GetHealthCurrent();
				if (healthDelta > 0f) m_npc.RestoreHealth(healthDelta);
				else if (healthDelta < 0f) m_npc.TakeDamage(-healthDelta);
				m_initialized = true;
			}
			if (distance < 5f) m_npc.TakeDamage(damage / Mathf.Min(1f, m_hits * 0.35f));
			else
			{
				m_hits++;
				float shade = Mathf.Max(0.1f, 0.9f - 0.05f * m_hits);
				Tint(new Color(shade, shade, 1f));
			}
			return true;
		}
		if (!m_initialized)
		{
			m_initialized = true;
			m_boomer = StartCoroutine(BoomerCharge());
		}
		else m_npc.TakeDamage(damage / Mathf.Min(1f, m_hits * 0.4f));
		m_hits++;
		return true;
	}

	public void HandleRape()
	{
		if (Type == LegacyShackAberrantType.Tank)
		{
			DropClothing(4, null);
			ShowStatus("The zombie's vice-like grip is squeezing away your consciousness...");
			CommonReferences.Instance.GetPlayer().DropEquippedEquippable();
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectPoisonDartRagdoll();
		}
		else if (Type == LegacyShackAberrantType.PinkHound)
		{
			DropClothing(10, ClothingCategory.Lower);
			ShowStatus("The lust-crazed hound mounted you while you cowered in fear!");
		}
	}

	private IEnumerator BoomerCharge()
	{
		BodyPart head = m_npc.GetSkeletonActor().GetBoneHead().GetBodyPart();
		if (head != null) head.Explode();
		while (m_npc != null && !m_npc.IsDead())
		{
			m_npc.RemoveStatModifier(m_boomerModifiers);
			m_boomerModifiers.Clear();
			m_boomerModifiers.Add(m_npc.AddStatModifier("SpeedAccel", 4f));
			m_boomerModifiers.Add(m_npc.AddStatModifier("SpeedMax", 4f));
			float distance = Mathf.Abs(CommonReferences.Instance.GetPlayer().transform.position.x - transform.position.x);
			if (!m_boomerFlashed && distance < 8f)
			{
				m_boomerFlashed = true;
				m_boomerFlash = StartCoroutine(FlashBoomer());
			}
			if (m_boomerExplodes && distance < 2.5f && m_npc.IsHasLineOfSightToPlayer())
			{
				foreach (BodyPart part in new List<BodyPart>(m_npc.GetSkeletonActor().GetAllBodyParts()))
					if (part != null && part.GetComponent<SpriteRenderer>() != null) part.GetComponent<SpriteRenderer>().enabled = false;
				m_npc.Die();
				DropClothing(4, null);
				ShowStatus("The explosion scours you with strange liquid, blasting your clothes away!");
				yield break;
			}
			yield return new WaitForSeconds(0.15f);
		}
	}

	private IEnumerator FlashBoomer()
	{
		for (int flash = 0; flash < 5 && m_npc != null && !m_npc.IsDead(); flash++)
		{
			TintBoomerBody(new Color(0.8f, 0f, 0f));
			yield return new WaitForSeconds(0.1f);
			TintBoomerBody(Color.white);
			yield return new WaitForSeconds(0.1f);
		}
		TintBoomerBody(Color.white);
		m_boomerFlash = null;
	}

	private void TintBoomerBody(Color i_color)
	{
		if (m_npc == null || m_npc.GetSkeletonActor() == null) return;
		foreach (BodyPart part in m_npc.GetSkeletonActor().GetAllBodyParts())
		{
			if (part == null) continue;
			SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
			if (renderer != null) renderer.color = i_color;
		}
	}

	private static void DropClothing(int i_count, ClothingCategory? i_category)
	{
		SkeletonPlayer skeleton = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer();
		for (int dropped = 0; dropped < i_count; dropped++)
		{
			List<ClothingPiece> candidates = new List<ClothingPiece>();
			foreach (ClothingPiece piece in skeleton.GetClothingPiecesAttached())
			{
				Clothing clothing = Library.Instance.Clothes.GetClothing(piece.GetIdClothing());
				if (!i_category.HasValue || (clothing != null && clothing.GetCatergoryClothing() == i_category.Value)) candidates.Add(piece);
			}
			if (candidates.Count == 0) break;
			candidates[Random.Range(0, candidates.Count)].DropOrDestroy(false);
		}
	}

	private void Tint(Color i_color)
	{
		if (m_npc == null) return;
		foreach (SpriteRenderer renderer in m_npc.GetComponentsInChildren<SpriteRenderer>(true)) renderer.color = i_color;
	}

	private static void ShowStatus(string i_message)
	{
		if (CommonReferences.Instance == null || CommonReferences.Instance.GetManagerHud() == null) return;
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("Aberrant Zombie", i_message, StatusPlayerHudItemColor.Rape, 5f);
	}

	private void OnDestroy()
	{
		if (m_boomer != null) StopCoroutine(m_boomer);
		if (m_boomerFlash != null) StopCoroutine(m_boomerFlash);
		if (Type == LegacyShackAberrantType.Boomer) TintBoomerBody(Color.white);
	}
}
