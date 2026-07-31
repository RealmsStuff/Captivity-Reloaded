using UnityEngine;

public class Screen : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public virtual void Open()
	{
		base.gameObject.SetActive(value: true);
	}

	public virtual void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
