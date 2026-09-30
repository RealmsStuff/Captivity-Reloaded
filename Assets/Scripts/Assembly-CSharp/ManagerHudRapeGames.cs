using UnityEngine;

public class ManagerHudRapeGames : MonoBehaviour
{
	[SerializeField]
	private HudSmasher m_hudSmasher;

	private void LateUpdate()
	{
		if (m_hudSmasher.isActiveAndEnabled)
		{
			PlaceInWorldPos(m_hudSmasher.gameObject);
		}
	}

	private void PlaceInWorldPos(GameObject i_hudRapeGame)
	{
		Vector2 i_posWorld = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.transform.position;
		i_posWorld.y += CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetBodyPart()
			.GetComponent<SpriteRenderer>()
			.size.y * 1.5f;
		GetComponent<RectTransform>().anchoredPosition = CommonReferences.Instance.GetUtilityTools().WorldPosToCanvasPos(i_posWorld);
	}

	private void KeepInCorrectPosX(GameObject i_hudRapeGame)
	{
		Vector2 vector = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.transform.position;
		Vector2 anchoredPosition = RectTransformUtility.WorldToScreenPoint(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity(), vector) - CommonReferences.Instance.GetManagerHud().GetComponent<RectTransform>().sizeDelta / 2f;
		anchoredPosition.y = i_hudRapeGame.GetComponent<RectTransform>().anchoredPosition.y;
		i_hudRapeGame.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
	}

	public void ShowHudSmasher(ISmasherHudSource i_source)
	{
		m_hudSmasher.Show(i_source);
	}

	public void HideHudSmasher()
	{
		m_hudSmasher.Hide();
	}

	public HudSmasher GetHudSmasher()
	{
		return m_hudSmasher;
	}
}
