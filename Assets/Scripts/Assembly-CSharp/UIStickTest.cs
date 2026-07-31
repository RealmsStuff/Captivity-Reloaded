using UnityEngine;

public class UIStickTest : MonoBehaviour
{
	private void LateUpdate()
	{
		Vector3 vector = CommonReferences.Instance.GetPlayer().GetPosHips();
		vector.y -= 0.5f;
		GetComponent<RectTransform>().anchoredPosition = CommonReferences.Instance.GetUtilityTools().WorldPosToCanvasPos(vector);
	}
}
