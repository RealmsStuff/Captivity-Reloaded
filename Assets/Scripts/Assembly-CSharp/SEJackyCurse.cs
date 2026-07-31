using System.Collections.Generic;
using UnityEngine;

public class SEJackyCurse : StatusEffectTicker
{
	private float m_chanceToSpawn01;

	private float m_chanceToSpawnIncreasePerTick;

	private float m_bonusChanceToSpawn;

	private int m_countMaxSpawnedJackys;

	public SEJackyCurse(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable, float i_ticksPerSec, float i_chanceToSpawn01, float i_chanceToSpawnIncreasePerTick, int i_countMaxSpawnedJackys)
		: base(i_name, i_source, i_type, i_duration, i_isStackable, i_ticksPerSec)
	{
		m_chanceToSpawn01 = i_chanceToSpawn01;
		m_chanceToSpawnIncreasePerTick = i_chanceToSpawnIncreasePerTick;
		m_countMaxSpawnedJackys = i_countMaxSpawnedJackys;
		m_bonusChanceToSpawn = 0f;
	}

	public override void Tick()
	{
		if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.IsWaveActive() && GetJackyCountStage() < m_countMaxSpawnedJackys && !CommonReferences.Instance.GetPlayer().GetIsBeingRaped())
		{
			TrySpawnJacky();
		}
	}

	private int GetJackyCountStage()
	{
		List<NPC> allNPCs = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs();
		int num = 0;
		for (int i = 0; i < allNPCs.Count; i++)
		{
			if (allNPCs[i].GetName() == "Jacky")
			{
				num++;
			}
		}
		return num;
	}

	private void TrySpawnJacky()
	{
		if (Random.Range(0f, 1f) < 1f - (m_chanceToSpawn01 + m_bonusChanceToSpawn))
		{
			m_bonusChanceToSpawn += m_chanceToSpawnIncreasePerTick;
			return;
		}
		float num = 18f;
		bool flag = true;
		bool flag2 = true;
		int mask = LayerMask.GetMask("Platform", "Interactable");
		if ((bool)Physics2D.Raycast(m_actor.GetPos(), -m_actor.transform.right, num, mask))
		{
			flag = false;
		}
		if ((bool)Physics2D.Raycast(m_actor.GetPos(), m_actor.transform.right, num, mask))
		{
			flag2 = false;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(new Vector2(m_actor.GetPos().x - num, m_actor.GetPos().y), -m_actor.transform.up, m_actor.GetHeight() / 2f, mask);
		if (!raycastHit2D)
		{
			flag = false;
		}
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(new Vector2(m_actor.GetPos().x + num, m_actor.GetPos().y), -m_actor.transform.up, m_actor.GetHeight() / 2f, mask);
		if (!raycastHit2D2)
		{
			flag2 = false;
		}
		if (flag && flag2)
		{
			if (Random.Range(0f, 1f) < 0.5f)
			{
				SpawnJacky(raycastHit2D.point);
			}
			else
			{
				SpawnJacky(raycastHit2D2.point);
			}
			return;
		}
		if (flag)
		{
			SpawnJacky(raycastHit2D.point);
		}
		if (flag2)
		{
			SpawnJacky(raycastHit2D2.point);
		}
	}

	private void SpawnJacky(Vector2 i_pos)
	{
		m_bonusChanceToSpawn = 0f;
		Jacky obj = (Jacky)Library.Instance.Actors.GetActorDupe("Jacky");
		obj.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent();
		obj.transform.position = i_pos;
		obj.gameObject.SetActive(value: true);
		obj.Spawn(i_isFadeIn: false);
		obj.PlaceFeetOnPos(i_pos);
		obj.SetIsIgnoreWave(i_isIgnoreWave: true);
	}
}
