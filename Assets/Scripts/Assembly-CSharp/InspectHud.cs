using UnityEngine;
using UnityEngine.UI;

public class InspectHud : MonoBehaviour
{
	[SerializeField]
	private GameObject m_objNoteBox;

	[SerializeField]
	private Text m_txt;

	[SerializeField]
	private GameObject m_objInspectBox;

	[SerializeField]
	private Image m_imgInspection;

	[SerializeField]
	private GameObject m_imgOverlay;

	private void Awake()
	{
		m_objNoteBox.SetActive(value: false);
		m_objInspectBox.SetActive(value: false);
		m_imgOverlay.SetActive(value: false);
		m_txt.text = "";
	}

	private void Update()
	{
		if ((m_objNoteBox.activeSelf || m_objInspectBox.activeSelf) && CommonReferences.Instance.GetManagerInput().IsControllerLastUsed() && UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame)
		{
			Close();
		}
	}

	private void ListenToCloseEvents()
	{
		CommonReferences.Instance.GetPlayer().OnGetHit += Close;
		CommonReferences.Instance.GetPlayer().OnBeingRaped += Close;
		CommonReferences.Instance.GetPlayer().OnLabor += Close;
	}

	private void StopListenToCloseEvents()
	{
		CommonReferences.Instance.GetPlayer().OnGetHit -= Close;
		CommonReferences.Instance.GetPlayer().OnBeingRaped -= Close;
		CommonReferences.Instance.GetPlayer().OnLabor -= Close;
	}

	public void ShowNote(Note i_note)
	{
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_objNoteBox.SetActive(value: true);
		m_imgOverlay.SetActive(value: true);
		m_txt.text = i_note.GetText();
		ListenToCloseEvents();
		Time.timeScale = 0f;
	}

	public void InspectObject(InspectionObject i_inspectionObj)
	{
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_objInspectBox.SetActive(value: true);
		m_imgOverlay.SetActive(value: true);
		m_imgInspection.sprite = i_inspectionObj.GetSprite();
		ListenToCloseEvents();
	}

	public void Close()
	{
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		m_objNoteBox.SetActive(value: false);
		m_txt.text = "";
		m_objInspectBox.SetActive(value: false);
		m_imgOverlay.SetActive(value: false);
		Time.timeScale = 1f;
		StopListenToCloseEvents();
	}
}
