using System.Collections.Generic;
using UnityEngine;

public class RaperAnimation : MonoBehaviour
{
	[Header("---Animation---")]
	[SerializeField]
	private AnimationClip m_animClipRaper;

	[SerializeField]
	private AnimationClip m_animClipPlayer;

	[SerializeField]
	private int m_amountOfTimesToLoop;

	[Header("Or...")]
	[SerializeField]
	private float m_secondsDuration;

	[SerializeField]
	private bool m_isHasOral;

	[SerializeField]
	private bool m_isMutePlayer;

	[SerializeField]
	private bool m_isCameraFollowHipsNpc;

	[SerializeField]
	private bool m_isCameraRotateWithTarget;

	[Header("---Thrust---")]
	[SerializeField]
	private float m_powerThrustPleasure01;

	[SerializeField]
	private float m_powerThrustLibido01;

	[SerializeField]
	private float m_additionalPowerThrustCumPleasure01;

	[SerializeField]
	private float m_additionalPowerThrustCumLibido01;

	[SerializeField]
	private float m_litreCumPerThrust;

	[SerializeField]
	private float m_litreCumPerCumThrust;

	[Header("---Smasher---")]
	[SerializeField]
	private float m_resistanceRaper01;

	[SerializeField]
	private bool m_isUseThrustsToResist;

	[Header("---Attachment---")]
	[SerializeField]
	protected bool m_isAttachToPlayerBone;

	[SerializeField]
	protected BoneTypePlayer m_bonePlayerToAttachTo;

	[SerializeField]
	protected Vector3 m_offsetLocalPositionAttachment;

	[SerializeField]
	protected Vector3 m_offsetLocalEulerAnglesAttachment;

	[Header("---Particles---")]
	[SerializeField]
	private List<RapeParticleSystem> m_particlesThrust = new List<RapeParticleSystem>();

	[SerializeField]
	private List<RapeParticleSystem> m_particlesCumThrust = new List<RapeParticleSystem>();

	[SerializeField]
	private List<RapeParticleSystem> m_particlesUnique = new List<RapeParticleSystem>();

	[Header("---Audio---")]
	[SerializeField]
	private List<AudioClip> m_audiosRaperVoice = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioRaperVoice;

	[SerializeField]
	private List<AudioClip> m_audiosRaperLayer1 = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioRaperLayer1;

	[SerializeField]
	private List<AudioClip> m_audiosRaperLayer2 = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioRaperLayer2;

	[SerializeField]
	private List<AudioClip> m_audiosRaperLayer3 = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioRaperLayer3;

	[SerializeField]
	private List<AudioClip> m_audiosPlayerVoice = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioPlayerVoice;

	[SerializeField]
	private List<AudioClip> m_audiosPlayerLayer1 = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioPlayerLayer1;

	[SerializeField]
	private List<AudioClip> m_audiosPlayerLayer2 = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioPlayerLayer2;

	[SerializeField]
	private List<AudioClip> m_audiosPlayerLayer3 = new List<AudioClip>();

	[SerializeField]
	private int m_chanceOfPlayingAudioPlayerLayer3;

	[SerializeField]
	private List<AudioClip> m_audiosThrust = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosCum = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosUnique = new List<AudioClip>();

	public string GetNameStateAnimationRaper()
	{
		return m_animClipRaper.name;
	}

	public string GetNameStateAnimationPlayer()
	{
		return m_animClipPlayer.name;
	}

	public int GetAmountOfTimesToLoop()
	{
		return m_amountOfTimesToLoop;
	}

	public float GetDurationSecondsAnimClipRaper()
	{
		if (m_secondsDuration > 0f)
		{
			return m_secondsDuration;
		}
		if (m_amountOfTimesToLoop == 0)
		{
			return m_animClipRaper.length;
		}
		return m_animClipRaper.length * (float)m_amountOfTimesToLoop;
	}

	public bool IsHasOral()
	{
		return m_isHasOral;
	}

	public float GetPowerThrustPleasure01()
	{
		return m_powerThrustPleasure01;
	}

	public float GetPowerThrustLibido01()
	{
		return m_powerThrustLibido01;
	}

	public float GetAdditionalPowerCumThrustPleasure01()
	{
		return m_additionalPowerThrustCumPleasure01;
	}

	public float GetAdditionalPowerCumThrustLibido01()
	{
		return m_additionalPowerThrustCumLibido01;
	}

	public List<RapeParticleSystem> GetParticlesThrust()
	{
		return m_particlesThrust;
	}

	public List<RapeParticleSystem> GetParticlesCumThrust()
	{
		return m_particlesCumThrust;
	}

	public List<RapeParticleSystem> GetParticlesUnique()
	{
		return m_particlesUnique;
	}

	public List<AudioClip> GetAudiosRaperVoice()
	{
		return m_audiosRaperVoice;
	}

	public int GetChanceOfPlayingAudioRaperVoice()
	{
		return m_chanceOfPlayingAudioRaperVoice;
	}

	public List<AudioClip> GetAudiosRaperLayer1()
	{
		return m_audiosRaperLayer1;
	}

	public int GetChanceOfPlayingAudioRaperLayer1()
	{
		return m_chanceOfPlayingAudioRaperLayer1;
	}

	public List<AudioClip> GetAudiosRaperLayer2()
	{
		return m_audiosRaperLayer2;
	}

	public int GetChanceOfPlayingAudioRaperLayer2()
	{
		return m_chanceOfPlayingAudioRaperLayer2;
	}

	public List<AudioClip> GetAudiosRaperLayer3()
	{
		return m_audiosRaperLayer3;
	}

	public int GetChanceOfPlayingAudioRaperLayer3()
	{
		return m_chanceOfPlayingAudioRaperLayer3;
	}

	public List<AudioClip> GetAudiosPlayerVoice()
	{
		return m_audiosPlayerVoice;
	}

	public int GetChanceOfPlayingAudioPlayerVoice()
	{
		return m_chanceOfPlayingAudioPlayerVoice;
	}

	public List<AudioClip> GetAudiosPlayerLayer1()
	{
		return m_audiosPlayerLayer1;
	}

	public int GetChanceOfPlayingAudioPlayerLayer1()
	{
		return m_chanceOfPlayingAudioPlayerLayer1;
	}

	public List<AudioClip> GetAudiosPlayerLayer2()
	{
		return m_audiosPlayerLayer2;
	}

	public int GetChanceOfPlayingAudioPlayerLayer2()
	{
		return m_chanceOfPlayingAudioPlayerLayer2;
	}

	public List<AudioClip> GetAudiosPlayerLayer3()
	{
		return m_audiosPlayerLayer3;
	}

	public int GetChanceOfPlayingAudioPlayerLayer3()
	{
		return m_chanceOfPlayingAudioPlayerLayer3;
	}

	public List<AudioClip> GetAudiosThrust()
	{
		return m_audiosThrust;
	}

	public List<AudioClip> GetAudiosCum()
	{
		return m_audiosCum;
	}

	public List<AudioClip> GetAudiosUnique()
	{
		return m_audiosUnique;
	}

	public bool IsAttachToPlayerBone()
	{
		return m_isAttachToPlayerBone;
	}

	public BoneTypePlayer GetBonePlayerToAttachTo()
	{
		return m_bonePlayerToAttachTo;
	}

	public Vector3 GetOffsetLocalPositionAttachment()
	{
		return m_offsetLocalPositionAttachment;
	}

	public Vector3 GetOffsetLocalEulerAnglesAttachment()
	{
		return m_offsetLocalEulerAnglesAttachment;
	}

	public float GetLitreCumPerThrust()
	{
		return m_litreCumPerThrust;
	}

	public float GetLitreCumPerCumThrust()
	{
		return m_litreCumPerCumThrust;
	}

	public float GetResistanceRaper01()
	{
		return m_resistanceRaper01;
	}

	public bool IsUseThrustToResist()
	{
		return m_isUseThrustsToResist;
	}

	public bool IsMutePlayer()
	{
		return m_isMutePlayer;
	}

	public bool IsCameraFollowHipsNpc()
	{
		return m_isCameraFollowHipsNpc;
	}

	public bool IsCameraRotateWithTarget()
	{
		return m_isCameraRotateWithTarget;
	}
}
