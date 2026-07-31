using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuArchive : Menu
{
	[SerializeField]
	private UnityEngine.UI.Button m_btnRaperDefault;

	[SerializeField]
	private UnityEngine.UI.Button m_btnPlayAnimDefault;

	[SerializeField]
	private Sprite m_sprBtnRape;

	[SerializeField]
	private Sprite m_sprBtnRapeCum;

	[SerializeField]
	private Slider m_sliderZoom;

	[SerializeField]
	private GameObject m_stage;

	[SerializeField]
	private InfoNPCArchive m_infoNPCArchive;

	private List<NPC> m_rapers = new List<NPC>();

	private List<UnityEngine.UI.Button> m_btnsAnim = new List<UnityEngine.UI.Button>();

	private NPC m_raperCurrent;

	private Player m_playerCurrent;

	private void Start()
	{
		foreach (Actor allActor in Library.Instance.Actors.GetAllActors())
		{
			if (allActor is NPC)
			{
				NPC nPC = (NPC)allActor;
				if (nPC.GetIsRaper())
				{
					m_rapers.Add(nPC);
				}
			}
		}
		BuildButtonsRaper();
	}

	private void Update()
	{
		float num = m_sliderZoom.value / m_sliderZoom.maxValue * 100f;
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(m_sliderZoom.maxValue * ((100f - num) / 100f) + m_sliderZoom.minValue, 1f);
	}

	private void BuildButtonsRaper()
	{
		float y = m_btnRaperDefault.GetComponent<RectTransform>().sizeDelta.y;
		float num = 12f;
		int num2 = 0;
		foreach (NPC raper in m_rapers)
		{
			UnityEngine.UI.Button button = Object.Instantiate(m_btnRaperDefault, m_btnRaperDefault.transform.parent);
			RectTransform component = button.GetComponent<RectTransform>();
			Vector3 vector = component.anchoredPosition;
			vector.y += y * (float)(-num2);
			if (num2 != 0)
			{
				vector.y -= num * (float)num2;
			}
			component.anchoredPosition = vector;
			ApplyRaperToButton(button, raper);
			button.gameObject.SetActive(value: true);
			num2++;
		}
	}

	private void ApplyRaperToButton(UnityEngine.UI.Button i_btn, NPC i_npc)
	{
		i_btn.GetComponentInChildren<Text>().text = i_npc.GetName();
		i_btn.onClick.AddListener(delegate
		{
			LoadRaper(i_npc);
		});
	}

	private void LoadRaper(NPC i_npc)
	{
		DestroyAllNPCsInStageArchive();
		m_raperCurrent = Object.Instantiate(i_npc, m_stage.transform);
		m_raperCurrent.transform.localPosition = new Vector3(0f, 0f, m_raperCurrent.transform.localPosition.z);
		m_raperCurrent.gameObject.SetActive(value: true);
		m_raperCurrent.GetAnimator().Play("Idle");
		m_raperCurrent.Awake();
		m_raperCurrent.Start();
		m_raperCurrent.SetIsThinking(i_isThinking: true);
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(m_raperCurrent.gameObject);
		m_infoNPCArchive.SetAndShowInfoNPC(i_npc);
		BuildAnimButtons();
	}

	private void DestroyAllNPCsInStageArchive()
	{
		NPC[] componentsInChildren = m_stage.GetComponentsInChildren<NPC>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void BuildAnimButtons()
	{
		DestroyButtonsPlayAnim();
		float x = m_btnPlayAnimDefault.GetComponent<RectTransform>().sizeDelta.x;
		float num = 4f;
		string[] array = new string[4] { "Idle", "Move", "Jump", "Attack" };
		int num2 = 0;
		AnimationClip[] animationClips = m_raperCurrent.GetComponent<Animator>().runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			bool flag = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (animationClip.name == text)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				UnityEngine.UI.Button button = Object.Instantiate(m_btnPlayAnimDefault, m_btnPlayAnimDefault.transform.parent);
				RectTransform component = button.GetComponent<RectTransform>();
				Vector3 vector = component.anchoredPosition;
				vector.x += x * (float)num2;
				if (num2 != 0)
				{
					vector.x += num * (float)num2;
				}
				component.anchoredPosition = vector;
				button.gameObject.SetActive(value: true);
				ApplyAnimToBtnPlayAnim(button, animationClip);
				m_btnsAnim.Add(button);
				num2++;
			}
		}
	}

	private void ApplyAnimToBtnPlayAnim(UnityEngine.UI.Button i_btn, AnimationClip i_clip)
	{
		if (i_clip.name == "Rape")
		{
			i_btn.GetComponent<Image>().sprite = m_sprBtnRape;
		}
		if (i_clip.name == "RapeCum")
		{
			i_btn.GetComponent<Image>().sprite = m_sprBtnRapeCum;
		}
		i_btn.onClick.AddListener(delegate
		{
			PlayAnim(i_clip);
		});
	}

	private void PlayAnim(AnimationClip i_clip)
	{
		m_raperCurrent.ForceEndRape();
		if (i_clip.name == "Rape")
		{
			m_raperCurrent.StartRapeArchive();
		}
		else if (i_clip.name == "RapeCum")
		{
			m_raperCurrent.StartRapeArchive();
			m_raperCurrent.GetRaper().ForceStartCum();
		}
		else
		{
			m_raperCurrent.GetAnimator().Play(i_clip.name);
		}
	}

	private void DestroyButtonsPlayAnim()
	{
		foreach (UnityEngine.UI.Button item in m_btnsAnim)
		{
			Object.Destroy(item.gameObject);
		}
		m_btnsAnim.Clear();
	}
}
