using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StageHub : Stage
{
	[SerializeField]
	private GameObject m_bed;

	[SerializeField]
	private Transform m_playerShowcaseWardrobeParent;

	[SerializeField]
	private Note m_note;

	[SerializeField]
	private Note m_noteKeycode;

	[SerializeField]
	private Keypad m_keypad;

	[Header("Secret")]
	[SerializeField]
	private Interactable m_intLightSwitch;

	[SerializeField]
	private LightBulb m_lightBulb;

	[SerializeField]
	private List<GameObject> m_objectsToActivate;

	[SerializeField]
	private List<GameObject> m_objectsToDeActivate;

	[SerializeField]
	private AudioClip m_audioAmbienceSecret;

	private string m_codeKeypad;

	private int m_timesActivatedLightSwitch;

	public override void OpenStage()
	{
		base.OpenStage();
		m_intLightSwitch.OnActivate += OnLightSwitchActivate;
		StartCoroutine(CoroutineWaitBeforeCheckGameCompletion());
		m_codeKeypad = ManagerDB.GetCodeKeypad();
		m_keypad.SetCode(m_codeKeypad);
		string text = m_noteKeycode.GetText();
		m_noteKeycode.SetText(text.Replace("$", m_codeKeypad));
	}

	private IEnumerator CoroutineWaitBeforeCheckGameCompletion()
	{
		yield return new WaitForSeconds(0.25f);
		CheckGameCompletion();
	}

	private void CheckGameCompletion()
	{
		if (CommonReferences.Instance.GetManagerChallenge().IsAllChallengesComplete())
		{
			m_note.gameObject.SetActive(value: false);
			m_noteKeycode.gameObject.SetActive(value: true);
		}
		else
		{
			m_note.gameObject.SetActive(value: true);
			m_noteKeycode.gameObject.SetActive(value: false);
		}
	}

	public Vector2 GetPosBed()
	{
		return m_bed.transform.position;
	}

	public Transform GetParentPlayerShowcaseWardrobe()
	{
		return m_playerShowcaseWardrobeParent;
	}

	private void OnLightSwitchActivate(Interactable i_interactable)
	{
		m_timesActivatedLightSwitch++;
		int num = 25;
		if (m_timesActivatedLightSwitch < num)
		{
			return;
		}
		m_intLightSwitch.SetIsUnInteractable(i_isUnInteractable: true);
		foreach (GameObject item in m_objectsToActivate)
		{
			item.SetActive(value: true);
		}
		foreach (GameObject item2 in m_objectsToDeActivate)
		{
			item2.SetActive(value: false);
		}
		m_lightBulb.SetFlickerness01(1f);
		m_lightBulb.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().color = Color.red;
		GetLightGlobal().color = Color.red;
		CommonReferences.Instance.GetManagerAudio().StopMusic();
		CommonReferences.Instance.GetManagerAudio().PlayAudioAmbience(m_audioAmbienceSecret);
	}
}
