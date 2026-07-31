using System.Collections;
using UnityEngine;

public class Trapper : Walker
{
	[SerializeField]
	private Trap m_trap;

	private int m_chancePlaceTrap = 20;

	private bool m_isPlacingTrap;

	private bool m_isCanPlaceTrap;

	private float m_durationPlaceTrap;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAITrapper>();
		m_xAI.Initialize(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(CoroutineWaitBeforeCanPlaceTraps());
	}

	protected override void RetrieveAnimationInfos(RuntimeAnimatorController i_runtimeAnimatorController)
	{
		base.RetrieveAnimationInfos(i_runtimeAnimatorController);
		AnimationClip[] animationClips = i_runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name == "PlaceTrap")
			{
				m_durationPlaceTrap = animationClip.length;
			}
		}
	}

	private IEnumerator CoroutineWaitBeforeCanPlaceTraps()
	{
		yield return new WaitForSeconds(Random.Range(1, 3));
		m_isCanPlaceTrap = true;
	}

	public void TryPlaceTrap()
	{
		if (Random.Range(0, 101) >= 100 - m_chancePlaceTrap && !IsCloseToAnotherTrapOrCloseToAnotherTrapperWhoIsPlacingATrap() && m_stateActorCurrent != StateActor.Jumping)
		{
			StartPlaceTrap();
		}
		else
		{
			StartCoroutine(CoroutineWaitBeforeTryPlaceTrapAgain());
		}
	}

	private bool IsCloseToAnotherTrapOrCloseToAnotherTrapperWhoIsPlacingATrap()
	{
		Trap[] componentsInChildren = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetComponentsInChildren<Trap>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (Vector2.Distance(componentsInChildren[i].transform.position, GetPos()) < 12f)
			{
				return true;
			}
		}
		Trapper[] componentsInChildren2 = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetComponentsInChildren<Trapper>();
		foreach (Trapper trapper in componentsInChildren2)
		{
			if (Vector2.Distance(trapper.GetPos(), GetPos()) < 12f && trapper.IsPlacingTrap())
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator CoroutineWaitBeforeTryPlaceTrapAgain()
	{
		m_isCanPlaceTrap = false;
		yield return new WaitForSeconds(Random.Range(2, 5));
		m_isCanPlaceTrap = true;
	}

	public void StartPlaceTrap()
	{
		m_isPlacingTrap = true;
		m_isCanPlaceTrap = false;
		StopMovingHorizontally();
		SetIsThinking(i_isThinking: false);
		m_animator.SetTrigger("PlaceTrap");
		StartCoroutine(CoroutineWaitForPlaceTrapEnd());
	}

	private IEnumerator CoroutineWaitForPlaceTrapEnd()
	{
		Timer timer = new Timer(m_durationPlaceTrap);
		yield return timer.CoroutinePlayAndWaitForEnd();
		m_isPlacingTrap = false;
		SetIsThinking(i_isThinking: true);
		StartCoroutine(CoroutinWaitForDelayAfterPlacingTrap());
	}

	public void PlaceTrap()
	{
		Trap trap = Object.Instantiate(m_trap, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		trap.transform.position = GetPosFeet();
		trap.gameObject.SetActive(value: true);
		trap.SetTrap();
	}

	private IEnumerator CoroutinWaitForDelayAfterPlacingTrap()
	{
		m_isCanPlaceTrap = false;
		yield return new WaitForSeconds(Random.Range(2, 11));
		m_isCanPlaceTrap = true;
	}

	public bool IsPlacingTrap()
	{
		return m_isPlacingTrap;
	}

	public bool IsCanPlaceTrap()
	{
		return m_isCanPlaceTrap;
	}

	public override void DropAllPickupAbles()
	{
		StageJungle stageJungle = (StageJungle)CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		if (!stageJungle.IsFetishDropped(m_pickUpablesToDrop[0]))
		{
			stageJungle.SetFetishDropped(m_pickUpablesToDrop[0]);
			base.DropAllPickupAbles();
		}
	}
}
