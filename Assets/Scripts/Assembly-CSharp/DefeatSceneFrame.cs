using System.Collections.Generic;
using UnityEngine;

public class DefeatSceneFrame : MonoBehaviour
{
	[SerializeField]
	private Sprite m_sprFrame;

	[SerializeField]
	private string m_txtFrame;

	[SerializeField]
	private List<AudioClip> m_audiosOpenFrame;

	[SerializeField]
	private List<AudioClip> m_audiosRepeat;

	[SerializeField]
	private float m_secsBetweenAudiosRepeat;

	[SerializeField]
	private Vector2 m_amountMoveImgRepeat01;

	[SerializeField]
	private float m_secsBetweenMoveImgRepeat;

	[SerializeField]
	private float m_amountToScaleImg01;

	public Sprite GetSpriteFrame()
	{
		return m_sprFrame;
	}

	public string GetTxtFrame()
	{
		return m_txtFrame;
	}

	public List<AudioClip> GetAudiosOpenFrame()
	{
		return m_audiosOpenFrame;
	}

	public List<AudioClip> GetAudiosRepeat()
	{
		return m_audiosRepeat;
	}

	public float GetSecsBetweenAudiosRepeat()
	{
		return m_secsBetweenAudiosRepeat;
	}

	public Vector2 GetDirMoveImgRepeat()
	{
		return m_amountMoveImgRepeat01 * 50f;
	}

	public float GetSecsBetweenImgRepeat()
	{
		return m_secsBetweenMoveImgRepeat;
	}

	public float GetAmountToScaleImg()
	{
		return m_amountToScaleImg01 / 10f;
	}
}
