using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ManagerAudio : MonoBehaviour
{
	[SerializeField]
	private AudioSource m_audioSourceSFX;

	[SerializeField]
	private AudioSource m_audioSourceHitsound;

	[SerializeField]
	private AudioSource m_audioSourceMusic;

	[SerializeField]
	private AudioSource m_audioSourceAmbience;

	[SerializeField]
	private AudioClip m_audioPickUp;

	private void Start()
	{
		SetVolumesToDefault();
		SetVolumesToSaved();
		m_audioSourceSFX.outputAudioMixerGroup = GetAudioMixer().FindMatchingGroups("SFX")[0];
		m_audioSourceHitsound.outputAudioMixerGroup = GetAudioMixer().FindMatchingGroups("Hitsound")[0];
		m_audioSourceMusic.outputAudioMixerGroup = GetAudioMixer().FindMatchingGroups("Music")[0];
		m_audioSourceAmbience.outputAudioMixerGroup = GetAudioMixer().FindMatchingGroups("Ambience")[0];
	}

	public void PlayAudioMusic(AudioClip i_audio)
	{
		if (i_audio == null)
		{
			Debug.Log("Audioclip music is null!");
			return;
		}
		if (i_audio != m_audioSourceMusic.clip)
		{
			m_audioSourceMusic.clip = i_audio;
		}
		m_audioSourceMusic.Play();
	}

	public void PlayAudioAmbience(AudioClip i_audio)
	{
		if (i_audio == null)
		{
			Debug.Log("Audioclip ambience is null!");
			return;
		}
		if (i_audio != m_audioSourceAmbience.clip)
		{
			m_audioSourceAmbience.clip = i_audio;
		}
		m_audioSourceAmbience.Play();
	}

	public void StopAudioAmbience()
	{
		m_audioSourceAmbience.Stop();
	}

	public void PlayAudioSFX(AudioClip i_audio)
	{
		if (i_audio == null)
		{
			Debug.Log("Audioclip sfx is null!");
			return;
		}
		AudioSource audioSource = DuplicateAndAddAudioSourceToGameObject(m_audioSourceSFX);
		audioSource.clip = i_audio;
		audioSource.Play();
		StartCoroutine(CoroutineWaitUntilAudioSFXIsOverThenDelete(audioSource));
	}

	public void PlayAudioSFX(AudioClip i_audio, bool i_isPlayAsync)
	{
		if (i_audio == null)
		{
			Debug.Log("Audioclip sfx is null!");
		}
		else if (i_isPlayAsync)
		{
			AudioSource audioSource = DuplicateAndAddAudioSourceToGameObject(m_audioSourceSFX);
			audioSource.clip = i_audio;
			audioSource.Play();
			StartCoroutine(CoroutineWaitUntilAudioSFXIsOverThenDelete(audioSource));
		}
		else
		{
			m_audioSourceSFX.PlayOneShot(i_audio);
		}
	}

	public void PlayAudioHitsound(float l_dmgMultiplier, bool i_isKill)
	{
		if (i_isKill)
		{
			m_audioSourceHitsound.PlayOneShot(Resources.Load<AudioClip>("Audio/Combat/Killsound"));
		}
		else
		{
			m_audioSourceHitsound.PlayOneShot(Resources.Load<AudioClip>("Audio/Combat/Hitsound"));
		}
		if (l_dmgMultiplier > 1f)
		{
			m_audioSourceHitsound.pitch = 0.8f;
		}
		if (l_dmgMultiplier == 1f)
		{
			m_audioSourceHitsound.pitch = 1f;
		}
		if (l_dmgMultiplier < 1f && l_dmgMultiplier > 0f)
		{
			m_audioSourceHitsound.pitch = 1.2f;
		}
		if (l_dmgMultiplier == 0f)
		{
			m_audioSourceHitsound.pitch = 1.5f;
		}
		if (i_isKill)
		{
			m_audioSourceHitsound.pitch = 1f;
		}
	}

	public void PlayAudioSFXRandom(List<AudioClip> l_audios, float i_chanceOfPlaying)
	{
		if (l_audios.Count != 0 && Random.Range(0f, 100f) <= i_chanceOfPlaying)
		{
			int index = Random.Range(0, l_audios.Count);
			AudioSource audioSource = DuplicateAndAddAudioSourceToGameObject(m_audioSourceSFX);
			audioSource.clip = l_audios[index];
			audioSource.Play();
			StartCoroutine(CoroutineWaitUntilAudioSFXIsOverThenDelete(audioSource));
		}
	}

	public void PlayAudioSFXRandom(List<AudioClip> l_audios, float i_chanceOfPlaying, bool i_isPlayAsync)
	{
		if (l_audios.Count != 0 && Random.Range(0f, 100f) <= i_chanceOfPlaying)
		{
			int index = Random.Range(0, l_audios.Count);
			if (i_isPlayAsync)
			{
				AudioSource audioSource = DuplicateAndAddAudioSourceToGameObject(m_audioSourceSFX);
				audioSource.clip = l_audios[index];
				audioSource.Play();
				StartCoroutine(CoroutineWaitUntilAudioSFXIsOverThenDelete(audioSource));
			}
			else
			{
				m_audioSourceSFX.PlayOneShot(l_audios[index]);
			}
		}
	}

	private AudioSource DuplicateAndAddAudioSourceToGameObject(AudioSource i_audioSource)
	{
		AudioSource audioSource = i_audioSource.gameObject.AddComponent<AudioSource>();
		audioSource.enabled = i_audioSource.enabled;
		audioSource.loop = i_audioSource.loop;
		audioSource.mute = i_audioSource.mute;
		audioSource.panStereo = i_audioSource.panStereo;
		audioSource.pitch = i_audioSource.pitch;
		audioSource.priority = i_audioSource.priority;
		audioSource.rolloffMode = i_audioSource.rolloffMode;
		audioSource.spatialBlend = i_audioSource.spatialBlend;
		audioSource.spatialize = i_audioSource.spatialize;
		audioSource.spatializePostEffects = i_audioSource.spatializePostEffects;
		audioSource.spread = i_audioSource.spread;
		audioSource.volume = i_audioSource.volume;
		audioSource.clip = i_audioSource.clip;
		audioSource.outputAudioMixerGroup = i_audioSource.outputAudioMixerGroup;
		return audioSource;
	}

	private IEnumerator CoroutineWaitUntilAudioSFXIsOverThenDelete(AudioSource i_audioSource)
	{
		yield return new WaitForSeconds(i_audioSource.clip.length);
		Object.Destroy(i_audioSource);
	}

	public void PlayAudioPickUp()
	{
		m_audioSourceSFX.PlayOneShot(m_audioPickUp);
	}

	public void SetVolumesToDefault()
	{
		SetVolumesToSaved();
	}

	public void SetVolumeMusicAndAmbienceToMute()
	{
		SetVolume("Music", 0);
		SetVolume("Ambience", 0);
	}

	public void StopMusic()
	{
		m_audioSourceMusic.Stop();
	}

	public void SetVolumesToSaved()
	{
		SetVolume("Master", PlayerPrefs.GetInt("VolumeMaster"));
		SetVolume("Music", PlayerPrefs.GetInt("VolumeMusic"));
		SetVolume("Ambience", PlayerPrefs.GetInt("VolumeAmbience"));
		SetVolume("Voice", PlayerPrefs.GetInt("VolumeVoice"));
		SetVolume("SFX", PlayerPrefs.GetInt("VolumeSFX"));
		SetVolume("Hitsound", PlayerPrefs.GetInt("VolumeHitsound"));
	}

	public void SetVolume(string i_nameAudio, int i_volumeLevel)
	{
		int num = 4 * i_volumeLevel + 40 - 80;
		if (i_volumeLevel == 0)
		{
			num = -80;
		}
		GetAudioMixer().FindMatchingGroups(i_nameAudio)[0].audioMixer.SetFloat("Volume" + i_nameAudio, num);
	}

	public AudioSource CreateAndAddAudioSourceSFX(GameObject i_objectToAddTo)
	{
		GameObject obj = new GameObject("AudioSourceSFX");
		obj.transform.parent = i_objectToAddTo.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.AddComponent<AudioSource>();
		AudioSource component = obj.GetComponent<AudioSource>();
		component.spatialBlend = 1f;
		component.minDistance = 6f;
		component.maxDistance = 32f;
		component.rolloffMode = AudioRolloffMode.Linear;
		component.dopplerLevel = 0f;
		component.outputAudioMixerGroup = GetAudioMixer().FindMatchingGroups("SFX")[0];
		return component;
	}

	public AudioSource CreateAndAddAudioSourceVoice(GameObject i_objectToAddTo)
	{
		GameObject obj = new GameObject("AudioSourceVoice");
		obj.transform.parent = i_objectToAddTo.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.AddComponent<AudioSource>();
		AudioSource component = obj.GetComponent<AudioSource>();
		component.spatialBlend = 1f;
		component.minDistance = 6f;
		component.maxDistance = 32f;
		component.rolloffMode = AudioRolloffMode.Linear;
		component.dopplerLevel = 0f;
		component.outputAudioMixerGroup = GetAudioMixer().FindMatchingGroups("Voice")[0];
		return component;
	}

	public AudioMixer GetAudioMixer()
	{
		return Resources.Load<AudioMixer>("Audio/AudioMixer");
	}
}
