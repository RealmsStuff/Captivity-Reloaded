using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerOptions : MonoBehaviour
{
	[Header("---Input")]
	[SerializeField]
	private GameObject m_parentOptions;

	[SerializeField]
	private GameObject m_innerOptions;

	[SerializeField]
	private GameObject m_innerResetWindow;

	[SerializeField]
	private InputBox m_inputBoxDefault;

	[SerializeField]
	private Image m_imgDisableInput;

	[SerializeField]
	private List<KeyCode> m_keyCodesIllegalToSet = new List<KeyCode>();

	private List<InputButtonXGame> m_buttonsOriginal;

	private List<InputBox> m_inputBoxes = new List<InputBox>();

	private InputBox m_inputBoxSelected;

	[Header("---Volume")]
	[SerializeField]
	private Slider m_sliderMaster;

	[SerializeField]
	private Text m_txtVolumeMaster;

	[SerializeField]
	private Slider m_sliderMusic;

	[SerializeField]
	private Text m_txtVolumeMusic;

	[SerializeField]
	private Slider m_sliderAmbience;

	[SerializeField]
	private Text m_txtVolumeAmbience;

	[SerializeField]
	private Slider m_sliderVoice;

	[SerializeField]
	private Text m_txtVolumeVoice;

	[SerializeField]
	private Slider m_sliderSFX;

	[SerializeField]
	private Text m_txtVolumeSFX;

	[SerializeField]
	private Slider m_sliderHitsound;

	[SerializeField]
	private Text m_txtVolumeHitsound;

	[SerializeField]
	private Dropdown m_dropDownDifficulty;

	[SerializeField]
	private Toggle m_toggleGunFlash;

	private Dropdown m_dropDownDisplayMode;

	private Dropdown m_dropDownResolution;

	private Text m_lblDisplayMode;

	private Text m_lblResolution;

	private List<GameDisplayResolution> m_displayResolutions = new List<GameDisplayResolution>();

	private RectTransform m_optionsRect;

	private RectTransform m_optionsContainerRect;

	private Vector2 m_optionsContainerSize = new Vector2(-1f, -1f);

	private bool m_isOpen;

	private void Update()
	{
		if (!m_isOpen)
		{
			return;
		}
		ConfigureOptionsLayout();
		m_txtVolumeMaster.text = m_sliderMaster.value.ToString();
		m_txtVolumeMusic.text = m_sliderMusic.value.ToString();
		m_txtVolumeAmbience.text = m_sliderAmbience.value.ToString();
		m_txtVolumeVoice.text = m_sliderVoice.value.ToString();
		m_txtVolumeSFX.text = m_sliderSFX.value.ToString();
		m_txtVolumeHitsound.text = m_sliderHitsound.value.ToString();
		if (m_inputBoxSelected != null)
		{
			List<KeyCode> anyKey = CommonReferences.Instance.GetManagerInput().GetAnyKey();
			if (anyKey.Count > 0 && !m_keyCodesIllegalToSet.Contains(anyKey[0]))
			{
				SetInputBoxKey(m_inputBoxSelected, anyKey[0]);
			}
		}
		CommonReferences.Instance.GetManagerAudio().SetVolume("Master", (int)m_sliderMaster.value);
		CommonReferences.Instance.GetManagerAudio().SetVolume("Music", (int)m_sliderMusic.value);
		CommonReferences.Instance.GetManagerAudio().SetVolume("Ambience", (int)m_sliderAmbience.value);
		CommonReferences.Instance.GetManagerAudio().SetVolume("Voice", (int)m_sliderVoice.value);
		CommonReferences.Instance.GetManagerAudio().SetVolume("SFX", (int)m_sliderSFX.value);
		CommonReferences.Instance.GetManagerAudio().SetVolume("Hitsound", (int)m_sliderHitsound.value);
	}

	private void BuildInputBoxes()
	{
		ClearInputBoxes();
		List<InputButtonXGame> inputButtons = ManagerDB.GetInputButtons();
		for (int i = 0; i < inputButtons.Count; i++)
		{
			InputBox inputBox = UnityEngine.Object.Instantiate(m_inputBoxDefault, m_inputBoxDefault.transform.parent);
			inputBox.Initialize(inputButtons[i]);
			inputBox.gameObject.SetActive(value: true);
			m_inputBoxes.Add(inputBox);
		}
	}

	private void ClearInputBoxes()
	{
		for (int i = 0; i < m_inputBoxes.Count; i++)
		{
			UnityEngine.Object.Destroy(m_inputBoxes[i].gameObject);
		}
		m_inputBoxes.Clear();
		m_inputBoxDefault.gameObject.SetActive(value: false);
	}

	public void SelectInputBox(InputBox i_inputBox)
	{
		m_inputBoxSelected = i_inputBox;
		m_inputBoxSelected.SetToListenInput();
		m_imgDisableInput.raycastTarget = true;
	}

	private void SetInputBoxKey(InputBox i_inputBox, KeyCode i_keyCode)
	{
		ManagerDB.SetInputButton(m_inputBoxSelected.GetNameButton(), i_keyCode);
		BuildInputBoxes();
		m_imgDisableInput.raycastTarget = false;
	}

	public void ResetInput()
	{
		ManagerDB.ResetInput();
		CommonReferences.Instance.GetManagerInput().SetButtonsToDefault();
		BuildInputBoxes();
	}

	private void BuildDropDownDifficulty()
	{
		m_dropDownDifficulty.options.Clear();
		List<string> list = new List<string>();
		int value = 0;
		for (int i = 0; i < Enum.GetValues(typeof(Difficulty)).Length; i++)
		{
			string text = Enum.GetNames(typeof(Difficulty))[i];
			list.Add(text);
			if (text == PlayerPrefs.GetString("Difficulty"))
			{
				value = i;
			}
		}
		m_dropDownDifficulty.AddOptions(list);
		m_dropDownDifficulty.value = value;
	}

	private void BuildDisplayOptions()
	{
		CreateDisplayControls();
		m_dropDownDisplayMode.options.Clear();
		m_dropDownDisplayMode.AddOptions(new List<string>(DisplaySettings.GetDisplayModeNames()));
		m_displayResolutions = DisplaySettings.GetSupportedResolutions();
		m_dropDownResolution.options.Clear();
		List<string> list = new List<string>();
		for (int i = 0; i < m_displayResolutions.Count; i++)
		{
			list.Add(m_displayResolutions[i].ToString());
		}
		m_dropDownResolution.AddOptions(list);
		m_dropDownDisplayMode.value = (int)DisplaySettings.GetSavedMode();
		GameDisplayResolution savedResolution = DisplaySettings.GetSavedResolution();
		int value = 0;
		for (int j = 0; j < m_displayResolutions.Count; j++)
		{
			if (m_displayResolutions[j].Width == savedResolution.Width && m_displayResolutions[j].Height == savedResolution.Height)
			{
				value = j;
				break;
			}
		}
		m_dropDownResolution.value = value;
		m_dropDownDisplayMode.RefreshShownValue();
		m_dropDownResolution.RefreshShownValue();
		UpdateResolutionAvailability(m_dropDownDisplayMode.value);
	}

	private void CreateDisplayControls()
	{
		if (m_dropDownDisplayMode != null)
		{
			return;
		}
		Transform parent = m_dropDownDifficulty.transform.parent;
		m_dropDownDisplayMode = UnityEngine.Object.Instantiate(m_dropDownDifficulty, parent);
		m_dropDownDisplayMode.gameObject.name = "dropDown_displayMode";
		m_dropDownDisplayMode.onValueChanged = new Dropdown.DropdownEvent();
		m_dropDownDisplayMode.onValueChanged.AddListener(UpdateResolutionAvailability);
		m_dropDownResolution = UnityEngine.Object.Instantiate(m_dropDownDifficulty, parent);
		m_dropDownResolution.gameObject.name = "dropDown_resolution";
		m_dropDownResolution.onValueChanged = new Dropdown.DropdownEvent();
		Text component = parent.Find("lbl_difficulty").GetComponent<Text>();
		m_lblDisplayMode = UnityEngine.Object.Instantiate(component, parent);
		m_lblDisplayMode.gameObject.name = "lbl_displayMode";
		m_lblDisplayMode.text = "Display mode";
		m_lblResolution = UnityEngine.Object.Instantiate(component, parent);
		m_lblResolution.gameObject.name = "lbl_resolution";
		m_lblResolution.text = "Resolution";
		ConfigureDropDownText(m_dropDownDisplayMode);
		ConfigureDropDownText(m_dropDownResolution);
		Text component2 = m_toggleGunFlash.GetComponentInChildren<Text>(includeInactive: true);
		if (component2 != null)
		{
			component2.resizeTextForBestFit = true;
			component2.resizeTextMinSize = 12;
			component2.resizeTextMaxSize = component2.fontSize;
		}
		ConfigureOptionsLayout();
	}

	private static void ConfigureDropDownText(Dropdown i_dropDown)
	{
		Text[] componentsInChildren = i_dropDown.GetComponentsInChildren<Text>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].resizeTextForBestFit = true;
			componentsInChildren[i].resizeTextMinSize = 12;
			componentsInChildren[i].resizeTextMaxSize = componentsInChildren[i].fontSize;
		}
	}

	private void UpdateResolutionAvailability(int i_displayMode)
	{
		bool flag = i_displayMode == (int)GameDisplayMode.BorderedFullscreen;
		if (flag)
		{
			GameDisplayResolution nativeResolution = DisplaySettings.GetNativeResolution();
			for (int i = 0; i < m_displayResolutions.Count; i++)
			{
				if (m_displayResolutions[i].Width == nativeResolution.Width && m_displayResolutions[i].Height == nativeResolution.Height)
				{
					m_dropDownResolution.value = i;
					break;
				}
			}
		}
		else if (i_displayMode == (int)GameDisplayMode.Windowed && m_displayResolutions.Count > 0)
		{
			GameDisplayResolution safeWindowedResolution = DisplaySettings.GetSafeWindowedResolution(m_displayResolutions[m_dropDownResolution.value]);
			for (int j = 0; j < m_displayResolutions.Count; j++)
			{
				if (m_displayResolutions[j].Width == safeWindowedResolution.Width && m_displayResolutions[j].Height == safeWindowedResolution.Height)
				{
					m_dropDownResolution.value = j;
					break;
				}
			}
		}
		m_dropDownResolution.interactable = !flag;
	}

	private void ConfigureOptionsLayout()
	{
		if (m_optionsRect == null)
		{
			m_optionsRect = m_innerOptions.GetComponent<RectTransform>();
			m_optionsContainerRect = (m_optionsRect != null) ? (m_optionsRect.parent as RectTransform) : null;
		}
		if (m_optionsRect == null || m_optionsContainerRect == null)
		{
			return;
		}
		if (m_dropDownDisplayMode != null)
		{
			RectTransform component = m_innerOptions.transform.Find("lbl_difficulty").GetComponent<RectTransform>();
			ConfigureRect(m_lblDisplayMode.rectTransform, new Vector2(165f, -72f), new Vector2(270f, 32f));
			ConfigureRect(m_lblResolution.rectTransform, new Vector2(465f, -72f), new Vector2(270f, 32f));
			ConfigureRect(m_dropDownDisplayMode.GetComponent<RectTransform>(), new Vector2(165f, -112f), new Vector2(270f, 42f));
			ConfigureRect(m_dropDownResolution.GetComponent<RectTransform>(), new Vector2(465f, -112f), new Vector2(270f, 42f));
			ConfigureRect(component, new Vector2(165f, -164f), new Vector2(270f, 32f));
			ConfigureRect(m_dropDownDifficulty.GetComponent<RectTransform>(), new Vector2(165f, -204f), new Vector2(270f, 42f));
			ConfigureRect(m_toggleGunFlash.GetComponent<RectTransform>(), new Vector2(465f, -204f), new Vector2(270f, 42f));
		}
		Vector2 size = m_optionsContainerRect.rect.size;
		if (m_optionsContainerSize == size)
		{
			return;
		}
		float num = Mathf.Max(size.x - 64f, 1f) / 1280f;
		float num2 = Mathf.Max(size.y - 64f, 1f) / 840f;
		float num3 = Mathf.Min(1f, Mathf.Min(num, num2));
		m_optionsRect.anchorMin = new Vector2(0.5f, 0.5f);
		m_optionsRect.anchorMax = new Vector2(0.5f, 0.5f);
		m_optionsRect.pivot = new Vector2(0.5f, 0.5f);
		m_optionsRect.anchoredPosition = Vector2.zero;
		m_optionsRect.sizeDelta = new Vector2(1280f, 840f);
		m_optionsRect.localScale = new Vector3(num3, num3, 1f);
		m_optionsContainerSize = size;
	}

	private static void ConfigureRect(RectTransform i_rectTransform, Vector2 i_position, Vector2 i_size)
	{
		i_rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		i_rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		i_rectTransform.pivot = new Vector2(0.5f, 0.5f);
		i_rectTransform.anchoredPosition = i_position;
		i_rectTransform.sizeDelta = i_size;
		i_rectTransform.localScale = Vector3.one;
	}

	private void BuildToggleReduceGunFlash()
	{
		if (PlayerPrefs.GetInt("IsReduceGunFlash") == 1)
		{
			m_toggleGunFlash.isOn = true;
		}
		else
		{
			m_toggleGunFlash.isOn = false;
		}
	}

	public void Cancel()
	{
		for (int i = 0; i < m_buttonsOriginal.Count; i++)
		{
			ManagerDB.SetInputButton(m_buttonsOriginal[i].GetName(), m_buttonsOriginal[i].GetKeyCode());
		}
		CommonReferences.Instance.GetManagerInput().SetButtonsToSavedButtons();
		GetComponentInParent<ScreenTitle>().CloseOptions();
		CommonReferences.Instance.GetManagerAudio().SetVolumesToSaved();
	}

	public void Ok()
	{
		PlayerPrefs.SetInt("VolumeMaster", (int)m_sliderMaster.value);
		PlayerPrefs.SetInt("VolumeMusic", (int)m_sliderMusic.value);
		PlayerPrefs.SetInt("VolumeAmbience", (int)m_sliderAmbience.value);
		PlayerPrefs.SetInt("VolumeVoice", (int)m_sliderVoice.value);
		PlayerPrefs.SetInt("VolumeSFX", (int)m_sliderSFX.value);
		PlayerPrefs.SetInt("VolumeHitsound", (int)m_sliderHitsound.value);
		PlayerPrefs.SetString("Difficulty", m_dropDownDifficulty.options[m_dropDownDifficulty.value].text);
		if (m_toggleGunFlash.isOn)
		{
			PlayerPrefs.SetInt("IsReduceGunFlash", 1);
		}
		else
		{
			PlayerPrefs.SetInt("IsReduceGunFlash", 0);
		}
		if (m_displayResolutions.Count > 0)
		{
			DisplaySettings.SaveAndApply((GameDisplayMode)m_dropDownDisplayMode.value, m_displayResolutions[m_dropDownResolution.value]);
		}
		CommonReferences.Instance.GetManagerAudio().SetVolumesToSaved();
		GetComponentInParent<ScreenTitle>().CloseOptions();
	}

	public void Open()
	{
		if (!ManagerDB.IsInputFilled())
		{
			ManagerDB.ResetInput();
			CommonReferences.Instance.GetManagerInput().SetButtonsToDefault();
		}
		m_buttonsOriginal = ManagerDB.GetInputButtons();
		m_sliderMaster.value = PlayerPrefs.GetInt("VolumeMaster");
		m_sliderMusic.value = PlayerPrefs.GetInt("VolumeMusic");
		m_sliderAmbience.value = PlayerPrefs.GetInt("VolumeAmbience");
		m_sliderVoice.value = PlayerPrefs.GetInt("VolumeVoice");
		m_sliderSFX.value = PlayerPrefs.GetInt("VolumeSFX");
		m_sliderHitsound.value = PlayerPrefs.GetInt("VolumeHitsound");
		BuildInputBoxes();
		BuildDropDownDifficulty();
		BuildToggleReduceGunFlash();
		BuildDisplayOptions();
		ConfigureOptionsLayout();
		m_parentOptions.SetActive(value: true);
		m_isOpen = true;
	}

	public void OpenResetSaveWindow()
	{
		m_innerOptions.SetActive(value: false);
		m_innerResetWindow.SetActive(value: true);
	}

	public void CloseResetSaveWindow()
	{
		m_innerOptions.SetActive(value: true);
		m_innerResetWindow.SetActive(value: false);
	}

	public void ResetSave()
	{
		ManagerDB.ResetSave();
		CloseResetSaveWindow();
		Cancel();
	}

	public void Close()
	{
		m_isOpen = false;
		m_parentOptions.SetActive(value: false);
	}
}
