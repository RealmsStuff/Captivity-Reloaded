using UnityEngine;

public class Menu : MonoBehaviour
{
	public virtual void Open()
	{
		Show();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
