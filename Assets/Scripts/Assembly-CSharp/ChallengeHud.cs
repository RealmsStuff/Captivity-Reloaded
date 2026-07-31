using UnityEngine;

public class ChallengeHud : MonoBehaviour
{
	[SerializeField]
	private ChallengeCompletionItemHud m_challengeItemDefault;

	private void Awake()
	{
		m_challengeItemDefault.gameObject.SetActive(value: false);
	}

	public void CompleteChallenge(Challenge i_challenge)
	{
		ChallengeCompletionItemHud challengeCompletionItemHud = Object.Instantiate(m_challengeItemDefault, m_challengeItemDefault.transform.parent);
		challengeCompletionItemHud.gameObject.SetActive(value: true);
		challengeCompletionItemHud.Initialize(i_challenge);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/ChallengeComplete"));
	}

	public void Clear()
	{
		ChallengeCompletionItemHud[] componentsInChildren = m_challengeItemDefault.transform.parent.GetComponentsInChildren<ChallengeCompletionItemHud>(includeInactive: false);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
	}
}
