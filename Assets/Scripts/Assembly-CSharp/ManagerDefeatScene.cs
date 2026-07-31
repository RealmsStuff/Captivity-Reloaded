using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ManagerDefeatScene : MonoBehaviour
{
	public delegate void DelOnEndDefeatScene();

	[SerializeField]
	private Image m_imgFrame;

	[SerializeField]
	private GameObject m_txtBoxFrame;

	[SerializeField]
	private Image m_imgBlackOverlay;

	[SerializeField]
	private Transform m_pointMiddle;

	private DefeatScene m_defeatSceneCurrent;

	private int m_numFrameCurrent;

	private bool m_isShowingScene;

	private float m_distanceBetweenCenterAndImage;

	private float m_distanceXBetweenCenterAndImage;

	private float m_distanceYBetweenCenterAndImage;

	private Vector2 m_posAnchoredMiddle;

	public event DelOnEndDefeatScene OnEndDefeatScene;

	private void Start()
	{
		m_imgFrame.gameObject.SetActive(value: false);
		m_txtBoxFrame.gameObject.SetActive(value: false);
		m_posAnchoredMiddle = m_pointMiddle.GetComponent<RectTransform>().anchoredPosition;
	}

	private void Update()
	{
		m_distanceBetweenCenterAndImage = Vector2.Distance(m_imgFrame.GetComponent<RectTransform>().anchoredPosition, m_posAnchoredMiddle);
		m_distanceXBetweenCenterAndImage = m_posAnchoredMiddle.x - m_imgFrame.GetComponent<RectTransform>().anchoredPosition.x;
		m_distanceYBetweenCenterAndImage = m_posAnchoredMiddle.y - m_imgFrame.GetComponent<RectTransform>().anchoredPosition.y;
		FollowObject();
		if (m_isShowingScene && CommonReferences.Instance.GetManagerInput().IsButtonDown(InputButton.Fire))
		{
			AdvanceFrame();
		}
	}

	public void OpenScene(DefeatScene i_defeatScene)
	{
		StartCoroutine(CoroutineOpenScene(i_defeatScene));
	}

	private IEnumerator CoroutineOpenScene(DefeatScene i_defeatScene)
	{
		m_defeatSceneCurrent = i_defeatScene;
		m_numFrameCurrent = 1;
		yield return StartCoroutine(CoroutineAnimateBlackOverlay(i_isIn: true));
		m_imgFrame.gameObject.SetActive(value: true);
		m_txtBoxFrame.SetActive(value: true);
		OpenFrame(m_numFrameCurrent);
		m_isShowingScene = true;
		StartCoroutine(CoroutineAnimateBlackOverlay(i_isIn: false));
	}

	private void AdvanceFrame()
	{
		m_numFrameCurrent++;
		if (m_numFrameCurrent > m_defeatSceneCurrent.GetNumOfFrames())
		{
			EndScene();
		}
		else
		{
			OpenFrame(m_numFrameCurrent);
		}
	}

	private void OpenFrame(int i_numFrame)
	{
		m_numFrameCurrent = i_numFrame;
		DefeatSceneFrame frame = m_defeatSceneCurrent.GetFrame(i_numFrame);
		m_imgFrame.sprite = frame.GetSpriteFrame();
		m_txtBoxFrame.GetComponentInChildren<Text>().text = frame.GetTxtFrame();
		if (frame.GetAudiosOpenFrame().Count > 0)
		{
			foreach (AudioClip item in frame.GetAudiosOpenFrame())
			{
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(item);
			}
		}
		if (frame.GetAudiosRepeat().Count > 0)
		{
			StartCoroutine(CoroutineAudioRepeat());
		}
		if (frame.GetDirMoveImgRepeat() != Vector2.zero)
		{
			float amountToScaleImg = frame.GetAmountToScaleImg();
			m_imgFrame.GetComponent<RectTransform>().localScale = new Vector3(1f + amountToScaleImg, 1f + amountToScaleImg, 1f);
			StartCoroutine(CoroutineMoveImgRepeat());
		}
		else
		{
			m_imgFrame.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private IEnumerator CoroutineAudioRepeat()
	{
		int l_numFramePlayingAudio = m_numFrameCurrent;
		while (l_numFramePlayingAudio == m_numFrameCurrent)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFXRandom(m_defeatSceneCurrent.GetFrame(m_numFrameCurrent).GetAudiosRepeat(), 100f);
			yield return new WaitForSecondsRealtime(m_defeatSceneCurrent.GetFrame(m_numFrameCurrent).GetSecsBetweenAudiosRepeat());
		}
	}

	private IEnumerator CoroutineMoveImgRepeat()
	{
		int l_numFrameCurrent = m_numFrameCurrent;
		while (l_numFrameCurrent == m_numFrameCurrent)
		{
			m_imgFrame.GetComponent<RectTransform>().anchoredPosition = m_imgFrame.GetComponent<RectTransform>().anchoredPosition - m_defeatSceneCurrent.GetFrame(m_numFrameCurrent).GetDirMoveImgRepeat();
			yield return new WaitForSecondsRealtime(m_defeatSceneCurrent.GetFrame(m_numFrameCurrent).GetSecsBetweenImgRepeat());
		}
	}

	private void FollowObject()
	{
		Vector2 anchoredPosition = m_imgFrame.GetComponent<RectTransform>().anchoredPosition;
		if (anchoredPosition.x > m_posAnchoredMiddle.x)
		{
			anchoredPosition.x += m_distanceXBetweenCenterAndImage / 5f;
		}
		if (anchoredPosition.x < m_posAnchoredMiddle.x)
		{
			anchoredPosition.x += m_distanceXBetweenCenterAndImage / 5f;
		}
		if (anchoredPosition.y > m_posAnchoredMiddle.y)
		{
			anchoredPosition.y += m_distanceYBetweenCenterAndImage / 5f;
		}
		if (anchoredPosition.y < m_posAnchoredMiddle.y)
		{
			anchoredPosition.y += m_distanceYBetweenCenterAndImage / 5f;
		}
		m_imgFrame.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
	}

	private void EndScene()
	{
		StartCoroutine(CoroutineEndScene());
	}

	private IEnumerator CoroutineEndScene()
	{
		yield return StartCoroutine(CoroutineAnimateBlackOverlay(i_isIn: true));
		m_isShowingScene = false;
		m_imgFrame.gameObject.SetActive(value: false);
		m_txtBoxFrame.gameObject.SetActive(value: false);
		if (this.OnEndDefeatScene != null)
		{
			this.OnEndDefeatScene();
		}
		yield return StartCoroutine(CoroutineAnimateBlackOverlay(i_isIn: false));
	}

	public bool GetIsShowingScene()
	{
		return m_isShowingScene;
	}

	private IEnumerator CoroutineAnimateBlackOverlay(bool i_isIn)
	{
		float l_tranparencyFrom = ((!i_isIn) ? 1 : 0);
		float l_tranparencyTo = (i_isIn ? 1 : 0);
		float l_timeToMove = 1.5f;
		float l_timeCurrent = 0f;
		m_imgBlackOverlay.gameObject.SetActive(value: true);
		m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, l_tranparencyFrom);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.unscaledDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, a);
			yield return new WaitForEndOfFrame();
		}
		_ = i_isIn;
	}
}
