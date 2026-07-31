using UnityEngine;
using UnityEngine.UI;

public class HeartHud : MonoBehaviour
{
	[SerializeField]
	private Image m_imgHeart;

	private bool m_isAlive;

	public void Kill()
	{
		m_isAlive = false;
		m_imgHeart.sprite = CommonReferences.Instance.GetManagerHud().GetImageHeartDead();
	}

	public void Live()
	{
		m_isAlive = true;
		m_imgHeart.sprite = CommonReferences.Instance.GetManagerHud().GetImageHeartAlive();
	}

	public Image GetImgHeart()
	{
		return m_imgHeart;
	}
}
