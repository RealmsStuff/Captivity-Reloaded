using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ManagerOverlay : MonoBehaviour
{
	[SerializeField]
	private Image m_imgOverlayDefault;

	[SerializeField]
	private Image m_imgOverlayDefaultWithHole;

	[SerializeField]
	private Image m_imgOverlayLibido;

	[SerializeField]
	private Image m_imgOverlayBlack;

	[SerializeField]
	private GameObject m_parentGeneratedOverlays;

	private Player m_player;

	private void Start()
	{
		m_player = CommonReferences.Instance.GetPlayer();
	}

	private void LateUpdate()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		UpdateHornyOverlay();
	}

	private void UpdateHornyOverlay()
	{
		m_imgOverlayLibido.color = new Color(m_imgOverlayLibido.color.r, m_imgOverlayLibido.color.g, m_imgOverlayLibido.color.b, 0.15f / m_player.GetLibidoMax() * m_player.GetLibidoCurrent());
	}

	public GameObject PlayOverlayPopup(GameObject i_overlayToPlay, bool i_isDuplicateOverlay, bool i_isDestroyOverlayAfterAnimation, float i_timeToAnimate1, float i_transparencyStart1, float i_transparencyEnd1, float i_timeToAnimate2, float i_transparencyStart2, float i_transparencyEnd2, float i_timeDelayBetweenParts)
	{
		if (!base.isActiveAndEnabled)
		{
			return null;
		}
		GameObject gameObject = null;
		gameObject = ((!i_isDuplicateOverlay) ? i_overlayToPlay.gameObject : Object.Instantiate(i_overlayToPlay.gameObject, m_parentGeneratedOverlays.transform));
		StartCoroutine(CoroutinePlayOverlayPopup(gameObject.GetComponent<Image>(), i_isDestroyOverlayAfterAnimation, i_timeToAnimate1, i_transparencyStart1, i_transparencyEnd1, i_timeToAnimate2, i_transparencyStart2, i_transparencyEnd2, i_timeDelayBetweenParts));
		return gameObject;
	}

	public GameObject PlayOverlayPopup(Color i_colorOverlay, bool i_isUseOverlayWithHole, bool i_isDestroyOverlayAfterAnimation, float i_timeToAnimate1, float i_transparencyStart1, float i_transparencyEnd1, float i_timeToAnimate2, float i_transparencyStart2, float i_transparencyEnd2, float i_timeDelayBetweenParts)
	{
		if (!base.isActiveAndEnabled)
		{
			return null;
		}
		GameObject gameObject = null;
		gameObject = ((!i_isUseOverlayWithHole) ? Object.Instantiate(m_imgOverlayDefault.gameObject, m_parentGeneratedOverlays.transform) : Object.Instantiate(m_imgOverlayDefaultWithHole.gameObject, m_parentGeneratedOverlays.transform));
		gameObject.GetComponent<Image>().color = i_colorOverlay;
		StartCoroutine(CoroutinePlayOverlayPopup(gameObject.GetComponent<Image>(), i_isDestroyOverlayAfterAnimation, i_timeToAnimate1, i_transparencyStart1, i_transparencyEnd1, i_timeToAnimate2, i_transparencyStart2, i_transparencyEnd2, i_timeDelayBetweenParts));
		return gameObject;
	}

	private IEnumerator CoroutinePlayOverlayPopup(Image i_overlayToPlay, bool i_isDestroyOverlayAfterAnimation, float i_timeToAnimate1, float i_transparencyStart1, float i_transparencyEnd1, float i_timeToAnimate2, float i_transparencyStart2, float i_transparencyEnd2, float i_timeDelayBetweenParts)
	{
		yield return CoroutinePlayOverlay(i_overlayToPlay, i_isDestroyOverlayAfterAnimation: false, i_timeToAnimate1, i_transparencyStart1, i_transparencyEnd1);
		yield return new WaitForSeconds(i_timeDelayBetweenParts);
		yield return CoroutinePlayOverlay(i_overlayToPlay, i_isDestroyOverlayAfterAnimation, i_timeToAnimate2, i_transparencyStart2, i_transparencyEnd2);
	}

	public GameObject PlayOverlayFlash(GameObject i_overlayToPlay, bool i_isDuplicateOverlay, bool i_isDestroyOverlayAfterAnimation, int i_numOfFlashes, float i_timeBetweenFlashes, float i_transparency1, float i_transparency2)
	{
		if (!base.isActiveAndEnabled)
		{
			return null;
		}
		GameObject gameObject = null;
		gameObject = ((!i_isDuplicateOverlay) ? i_overlayToPlay.gameObject : Object.Instantiate(i_overlayToPlay.gameObject, m_parentGeneratedOverlays.transform));
		StartCoroutine(CoroutinePlayOverlayFlash(gameObject.GetComponent<Image>(), i_isDestroyOverlayAfterAnimation, i_numOfFlashes, i_timeBetweenFlashes, i_transparency1, i_transparency2));
		return gameObject;
	}

	public GameObject PlayOverlayFlash(Color i_colorOverlay, bool i_isUseOverlayWithHole, bool i_isDestroyOverlayAfterAnimation, int i_numOfFlashes, float i_timeBetweenFlashes, float i_transparency1, float i_transparency2)
	{
		if (!base.isActiveAndEnabled)
		{
			return null;
		}
		GameObject gameObject = null;
		gameObject = ((!i_isUseOverlayWithHole) ? Object.Instantiate(m_imgOverlayDefault.gameObject, m_parentGeneratedOverlays.transform) : Object.Instantiate(m_imgOverlayDefaultWithHole.gameObject, m_parentGeneratedOverlays.transform));
		gameObject.GetComponent<Image>().color = i_colorOverlay;
		StartCoroutine(CoroutinePlayOverlayFlash(gameObject.GetComponent<Image>(), i_isDestroyOverlayAfterAnimation, i_numOfFlashes, i_timeBetweenFlashes, i_transparency1, i_transparency2));
		return gameObject;
	}

	private IEnumerator CoroutinePlayOverlayFlash(Image i_overlayToPlay, bool i_isDestroyOverlayAfterAnimation, int i_numOfFlashes, float i_timeBetweenFlashes, float i_transparency1, float i_transparency2)
	{
		i_overlayToPlay.gameObject.SetActive(value: true);
		i_overlayToPlay.enabled = true;
		for (int l_numCurrentFlash = 0; l_numCurrentFlash < i_numOfFlashes; l_numCurrentFlash++)
		{
			i_overlayToPlay.color = new Color(i_overlayToPlay.color.r, i_overlayToPlay.color.g, i_overlayToPlay.color.b, i_transparency1);
			yield return new WaitForSeconds(i_timeBetweenFlashes);
			i_overlayToPlay.color = new Color(i_overlayToPlay.color.r, i_overlayToPlay.color.g, i_overlayToPlay.color.b, i_transparency2);
			yield return new WaitForSeconds(i_timeBetweenFlashes);
		}
		if (i_isDestroyOverlayAfterAnimation)
		{
			Object.Destroy(i_overlayToPlay.gameObject);
		}
	}

	public GameObject PlayOverlay(GameObject i_overlayToPlay, bool i_isDuplicateOverlay, bool i_isDestroyOverlayAfterAnimation, float i_timeToAnimate, float i_transparencyStart, float i_transparencyEnd)
	{
		if (!base.isActiveAndEnabled)
		{
			return null;
		}
		GameObject gameObject = null;
		gameObject = ((!i_isDuplicateOverlay) ? i_overlayToPlay.gameObject : Object.Instantiate(i_overlayToPlay.gameObject, m_parentGeneratedOverlays.transform));
		StartCoroutine(CoroutinePlayOverlay(gameObject.GetComponent<Image>(), i_isDestroyOverlayAfterAnimation, i_timeToAnimate, i_transparencyStart, i_transparencyEnd));
		return gameObject;
	}

	public GameObject PlayOverlay(Color i_colorOverlay, bool i_isUseOverlayWithHole, bool i_isDestroyOverlayAfterAnimation, float i_timeToAnimate, float i_transparencyStart, float i_transparencyEnd)
	{
		GameObject gameObject = null;
		gameObject = ((!i_isUseOverlayWithHole) ? Object.Instantiate(m_imgOverlayDefault.gameObject, m_parentGeneratedOverlays.transform) : Object.Instantiate(m_imgOverlayDefaultWithHole.gameObject, m_parentGeneratedOverlays.transform));
		gameObject.GetComponent<Image>().color = i_colorOverlay;
		StartCoroutine(CoroutinePlayOverlay(gameObject.GetComponent<Image>(), i_isDestroyOverlayAfterAnimation, i_timeToAnimate, i_transparencyStart, i_transparencyEnd));
		return gameObject;
	}

	private IEnumerator CoroutinePlayOverlay(Image i_overlay, bool i_isDestroyOverlayAfterAnimation, float i_timeToAnimate, float i_transparencyStart, float i_transparencyEnd)
	{
		i_overlay.gameObject.SetActive(value: true);
		i_overlay.enabled = true;
		float l_timeCurrent = 0f;
		i_overlay.color = new Color(i_overlay.color.r, i_overlay.color.g, i_overlay.color.b, i_transparencyStart);
		while (l_timeCurrent < i_timeToAnimate)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / i_timeToAnimate;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, i_transparencyStart, i_transparencyEnd, i_time);
			i_overlay.color = new Color(i_overlay.color.r, i_overlay.color.g, i_overlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		if (i_isDestroyOverlayAfterAnimation)
		{
			Object.Destroy(i_overlay.gameObject);
		}
	}

	public IEnumerator CoroutineAnimateBlackOverlay(bool i_isShow)
	{
		m_imgOverlayBlack.gameObject.SetActive(value: true);
		float l_tranparencyFrom = (i_isShow ? 0f : 1f);
		float l_tranparencyTo = (i_isShow ? 1f : 0f);
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgOverlayBlack.color = new Color(m_imgOverlayBlack.color.r, m_imgOverlayBlack.color.g, m_imgOverlayBlack.color.b, a);
			yield return new WaitForEndOfFrame();
		}
		if (!i_isShow)
		{
			m_imgOverlayBlack.gameObject.SetActive(value: false);
		}
	}

	public void ShowOverlay()
	{
		base.gameObject.SetActive(value: true);
	}

	public void HideOverlay()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ClearGeneratedOverlays()
	{
		Image[] componentsInChildren = m_parentGeneratedOverlays.GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
	}
}
