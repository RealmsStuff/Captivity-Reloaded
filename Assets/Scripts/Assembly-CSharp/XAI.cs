using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class XAI : MonoBehaviour
{
	protected NPC m_npc;

	protected XAIState m_xAIStateCurrent;

	protected Player m_player;

	protected NavNode m_nodeStart;

	protected Path m_pathCurrent;

	protected Vector2 m_destination;

	protected float m_radiusCompleteDestination = 1f;

	protected bool m_isHasPlayerJustBeenRaped;

	private float m_distanceAwait;

	protected Coroutine m_coroutineAwaitBeforeChase;

	private bool m_isLingerRape;

	protected Coroutine m_coroutineTryLingerRape;

	protected Coroutine m_coroutineTryCancelLingerRape;

	protected Coroutine m_coroutineWaitBeforeCanGetPathToPlayerAgain;

	public virtual void Initialize(NPC i_npc)
	{
		m_npc = i_npc;
		m_player = CommonReferences.Instance.GetPlayer();
		m_isLingerRape = false;
		m_distanceAwait = Random.Range(2f, 7f);
		m_npc.OnDie += OnDie;
	}

	public virtual void HandleIntelligence()
	{
		if (m_npc.IsDead())
		{
			return;
		}
		m_player = CommonReferences.Instance.GetPlayer();
		if (!m_player.IsDead())
		{
			if (m_player.GetIsBeingRaped() || m_isHasPlayerJustBeenRaped || m_player.GetStatePlayerCurrent() == StatePlayer.Labor)
			{
				if (m_player.GetIsBeingRaped())
				{
					m_isHasPlayerJustBeenRaped = true;
				}
				m_xAIStateCurrent = XAIState.Await;
				HandleCurrentState();
				return;
			}
			m_isHasPlayerJustBeenRaped = false;
		}
		if (m_player.IsDead())
		{
			m_xAIStateCurrent = XAIState.Linger;
		}
		else
		{
			m_xAIStateCurrent = XAIState.Chase;
		}
		HandleCurrentState();
	}

	private void HandleCurrentState()
	{
		switch (m_xAIStateCurrent)
		{
		case XAIState.Chase:
			m_npc.SetStateNPC(StateNPC.Chase);
			HandleStateChase();
			HandleCombat();
			break;
		case XAIState.Await:
			m_npc.SetStateNPC(StateNPC.Await);
			HandleStateAwait();
			break;
		case XAIState.Linger:
			m_npc.SetStateNPC(StateNPC.Linger);
			HandleStateLinger();
			break;
		}
	}

	protected virtual void HandleStateChase()
	{
		m_npc.FacePlayer();
		ChooseChaseDestination();
		_ = m_destination;
		MoveToDestination();
		if (m_pathCurrent != null)
		{
			HandlePath();
		}
	}

	protected virtual void ChooseChaseDestination()
	{
		if (m_pathCurrent == null || m_pathCurrent.GetPathNodeCurrent() == null)
		{
			m_pathCurrent = GetPathToPlayer(i_isForceGetPath: false);
		}
		if (m_pathCurrent != null && m_pathCurrent.GetPathNodeCurrent() != null)
		{
			m_destination = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
		}
		else
		{
			m_destination = Vector2.zero;
		}
	}

	protected abstract void MoveToDestination();

	protected abstract void HandleCombat();

	protected virtual void HandleStateAwait()
	{
		if (m_isHasPlayerJustBeenRaped && !m_player.GetIsBeingRaped() && m_coroutineAwaitBeforeChase == null)
		{
			m_coroutineAwaitBeforeChase = StartCoroutine(CoroutineAwaitBeforeChase());
		}
		if ((m_player.GetIsBeingRaped() || m_player.GetStatePlayerCurrent() == StatePlayer.Labor) && m_npc.GetPlatformCurrent() == m_player.GetPlatformCurrent())
		{
			HandleAwaitDistance();
		}
		m_npc.FacePlayer();
	}

	protected virtual void HandleAwaitDistance()
	{
		if (m_npc.GetDistanceBetweenPlayerHips() < m_distanceAwait)
		{
			m_npc.MoveAwayFromPlayer();
		}
		else if (m_npc.GetDistanceBetweenPlayerHips() > m_distanceAwait + 4f)
		{
			m_npc.MoveToPlayer();
		}
	}

	private IEnumerator CoroutineAwaitBeforeChase()
	{
		float seconds = Random.Range(0.25f, 4f);
		yield return new WaitForSeconds(seconds);
		m_isHasPlayerJustBeenRaped = false;
		m_coroutineAwaitBeforeChase = null;
	}

	private void InterruptAwaitBeforeChase()
	{
		StopCoroutine(m_coroutineAwaitBeforeChase);
		m_isHasPlayerJustBeenRaped = false;
		m_coroutineAwaitBeforeChase = null;
	}

	protected virtual void HandleStateLinger()
	{
		if (m_coroutineTryLingerRape == null)
		{
			m_coroutineTryLingerRape = StartCoroutine(CoroutineTryLingerRape());
		}
		if (m_isLingerRape && m_player.GetStatePlayerCurrent() == StatePlayer.Labor)
		{
			m_isLingerRape = false;
		}
		if (m_isLingerRape && (m_player.GetIsBeingRaped() || m_player.GetStatePlayerCurrent() == StatePlayer.Labor))
		{
			HandleStateAwait();
			return;
		}
		if (m_isLingerRape)
		{
			HandleStateChase();
			HandleCombat();
			return;
		}
		ChooseLingerDestination();
		_ = m_destination;
		MoveToDestination();
		if (m_pathCurrent != null)
		{
			HandlePath();
		}
	}

	private IEnumerator CoroutineTryLingerRape()
	{
		while (true)
		{
			if (!m_isLingerRape && Random.Range(0, 101) > 25 && (float)Random.Range(0, 101) >= 100f - m_npc.GetRaper().GetLibido01() * 100f)
			{
				StartLingerRape();
			}
			float seconds = Random.Range(0.5f, 10f);
			yield return new WaitForSeconds(seconds);
		}
	}

	private void StartLingerRape()
	{
		m_isLingerRape = true;
		m_npc.GetRaper().OnStartRape += StopLingerRape;
		m_coroutineTryCancelLingerRape = StartCoroutine(CoroutineTryCancelLingerRape());
	}

	private void StopLingerRape()
	{
		m_npc.GetRaper().OnStartRape -= StopLingerRape;
		m_isLingerRape = false;
		if (m_coroutineTryCancelLingerRape != null)
		{
			StopCoroutine(m_coroutineTryCancelLingerRape);
			m_coroutineTryCancelLingerRape = null;
		}
	}

	private IEnumerator CoroutineTryCancelLingerRape()
	{
		while (m_isLingerRape)
		{
			float seconds = 15f;
			yield return new WaitForSeconds(seconds);
			if (m_player.GetIsBeingRaped() && Random.Range(0, 101) > 25)
			{
				StopLingerRape();
			}
		}
		m_coroutineTryCancelLingerRape = null;
	}

	protected virtual void ChooseLingerDestination()
	{
		if (m_pathCurrent == null || m_pathCurrent.GetPathNodeCurrent() == null)
		{
			m_pathCurrent = GetPathToRandomAccesibleNode();
		}
		if (m_pathCurrent != null && m_pathCurrent.GetPathNodeCurrent() != null)
		{
			m_destination = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
		}
		else
		{
			m_destination = Vector2.zero;
		}
	}

	protected virtual void HandlePath()
	{
		if (Vector2.Distance(m_npc.GetPosFeet(), m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos()) < m_radiusCompleteDestination)
		{
			CompletePathNodeCurrent();
			PerformStepAction();
		}
	}

	protected virtual void CompletePathNodeCurrent()
	{
		m_pathCurrent.GetPathNodeCurrent().Complete();
	}

	protected abstract void PerformStepAction();

	protected virtual Path GetPathToPlayer(bool i_isForceGetPath)
	{
		if (!i_isForceGetPath && m_coroutineWaitBeforeCanGetPathToPlayerAgain != null)
		{
			return null;
		}
		CreateNavNodeNpcStart();
		Path path = new PathFinder().CreatePathToPlayer(m_npc);
		if (path == null)
		{
			m_coroutineWaitBeforeCanGetPathToPlayerAgain = StartCoroutine(CoroutineWaitBeforeCanGetPathToPlayerAgain());
		}
		return path;
	}

	private IEnumerator CoroutineWaitBeforeCanGetPathToPlayerAgain()
	{
		yield return new WaitForSeconds(1f);
		m_coroutineWaitBeforeCanGetPathToPlayerAgain = null;
	}

	protected virtual Path GetPathToRandomAccesibleNode()
	{
		CreateNavNodeNpcStart();
		NavMap navMap = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap();
		List<Path> list = new List<Path>();
		PathFinder pathFinder = new PathFinder();
		foreach (NavNode allNavNode in navMap.GetAllNavNodes())
		{
			Path path = pathFinder.CreatePathToNode(m_npc, allNavNode);
			if (path != null)
			{
				list.Add(path);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		int index = Random.Range(0, list.Count);
		return list[index];
	}

	public NPC GetNPC()
	{
		return m_npc;
	}

	public abstract void CreateNavNodeNpcStart();

	public NavNode GetNavNodeNpcStart()
	{
		return m_nodeStart;
	}

	protected virtual void OnDie()
	{
		m_npc.OnDie -= OnDie;
		if (m_nodeStart != null)
		{
			CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
				.DestroyNode(m_nodeStart);
		}
	}
}
