using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Raper : MonoBehaviour
{
	public delegate void StartRapeDel();

	public delegate void EndRapeDel();

	public delegate void AdvanceRaperAnimationDel();

	public delegate void ThrustDel();

	public delegate void CumThrustDel();

	public delegate void PlayerVoiceDel();

	[SerializeField]
	protected List<RaperAnimation> m_raperAnimations = new List<RaperAnimation>();

	protected RaperAnimation m_raperAnimationCurrent;

	[Header("---Options---")]
	[SerializeField]
	protected bool m_isShowVictimWeapon;

	[SerializeField]
	protected bool m_isDropVictimWeapon;

	[SerializeField]
	protected Vector2 m_forceOnVictimOnRapeEnd;

	[SerializeField]
	protected bool m_isKinematicPlayer;

	[SerializeField]
	protected bool m_isKinematicRaper;

	[SerializeField]
	protected bool m_isPositionToPlayer;

	[SerializeField]
	protected bool m_isCenterYRaper;

	[SerializeField]
	protected bool m_isCenterYPlayer;

	[SerializeField]
	protected bool m_isInverseFlipX;

	[SerializeField]
	protected bool m_isDisableAnimatorLayersRaper;

	[SerializeField]
	protected List<Light> m_lightsRape = new List<Light>();

	[SerializeField]
	protected Collider2D m_colliderRape;

	[SerializeField]
	protected bool m_isDestroySelfAfterRape;

	[Header("---Pregnancy---")]
	[SerializeField]
	protected string m_nameFetus;

	[SerializeField]
	protected List<Actor> m_actorsToInsert = new List<Actor>();

	[SerializeField]
	protected bool m_isFetusOneWhole;

	[SerializeField]
	protected float m_durationPregnant;

	[SerializeField]
	protected float m_offsetRandomDurationPregnant01;

	[SerializeField]
	protected bool m_isFetusToInsertRandomChosen;

	[SerializeField]
	protected bool m_isImpregnateOnCumThrust;

	[SerializeField]
	protected bool m_isEgg;

	[SerializeField]
	protected Egg m_egg;

	[Header("---Player Speech Bubbles---")]
	[SerializeField]
	protected bool m_isPlayerHeadFacingLeft;

	[SerializeField]
	protected bool m_isUseProbabilityForSpeechCumTrust;

	[SerializeField]
	protected List<string> m_textsSpeechThrust0to24;

	[SerializeField]
	protected List<string> m_textsSpeechThrust25to49;

	[SerializeField]
	protected List<string> m_textsSpeechThrust50to74;

	[SerializeField]
	protected List<string> m_textsSpeechThrust75to100;

	[SerializeField]
	protected List<string> m_textsSpeechCumThrust0to24;

	[SerializeField]
	protected List<string> m_textsSpeechCumThrust25to49;

	[SerializeField]
	protected List<string> m_textsSpeechCumThrust50to74;

	[SerializeField]
	protected List<string> m_textsSpeechCumThrust75to100;

	[SerializeField]
	protected List<string> m_textsSpeechImpregnation0to24;

	[SerializeField]
	protected List<string> m_textsSpeechImpregnation25to49;

	[SerializeField]
	protected List<string> m_textsSpeechImpregnation50to74;

	[SerializeField]
	protected List<string> m_textsSpeechImpregnation75to100;

	[SerializeField]
	protected List<string> m_textsSpeechPlayerOrgasm;

	private RuntimeAnimatorController m_animatorPlayerOriginal;

	private Dictionary<Bone, Vector3[]> m_posePlayerOld = new Dictionary<Bone, Vector3[]>();

	private float m_probabilityCreateSpeechBubble01 = 0.5f;

	private List<ParticleSystem> m_particlesThrustDuplicates = new List<ParticleSystem>();

	protected bool m_isUseCumDurationFromAnim;

	protected Player m_player;

	protected NPC m_npc;

	protected bool m_isRaping;

	protected bool m_isCanRape = true;

	protected float m_lengthRapeAnim;

	protected ManagerAudio m_managerAudio;

	protected float m_posZOriginal;

	protected bool m_isPlayerOrgasmDuringRape;

	protected float m_litreCumTaken;

	private bool m_isPlayerMuteBeforeRape;

	protected float m_libido01 = 1f;

	private bool m_isArchiveMode;

	private DefeatScene m_defeatScene;

	protected Coroutine m_coroutinePlayAnimations;

	public event StartRapeDel OnStartRape;

	public event EndRapeDel OnEndRape;

	public event AdvanceRaperAnimationDel OnAdvanceRaperAnimation;

	public event ThrustDel OnThrust;

	public event CumThrustDel OnCumThrust;

	public event PlayerVoiceDel OnPlayerVoice;

	protected virtual void Start()
	{
		m_managerAudio = CommonReferences.Instance.GetManagerAudio();
		if ((bool)GetComponent<NPC>())
		{
			m_npc = GetComponent<NPC>();
		}
		else
		{
			m_npc = GetComponentInParent<NPC>();
		}
		m_player = CommonReferences.Instance.GetPlayer();
		if (m_colliderRape != null)
		{
			m_colliderRape.gameObject.SetActive(value: false);
			m_colliderRape.enabled = false;
		}
		if (m_lightsRape != null)
		{
			foreach (Light item in m_lightsRape)
			{
				item.gameObject.SetActive(value: false);
				item.enabled = false;
			}
		}
		if (m_isImpregnateOnCumThrust)
		{
			OnCumThrust += Impregnate;
		}
		StartCoroutine(CoroutineUpdateLibido());
		m_isDropVictimWeapon = false;
	}

	public virtual void BeginRape()
	{
		if (!m_player.GetIsCanBeRaped() && !m_player.IsExposing())
		{
			return;
		}
		m_posZOriginal = base.transform.position.z;
		Vector3 position = base.transform.position;
		position.z += -0.1f;
		base.transform.position = position;
		if (!m_isCanRape)
		{
			return;
		}
		m_isCanRape = false;
		m_isRaping = true;
		m_isPlayerMuteBeforeRape = m_player.IsMute();
		m_npc.SetIsCanAttack(i_isCanAttack: false);
		m_npc.SetIsThinking(i_isThinking: false);
		m_npc.SetIsInvulnerable(i_isInvulnerable: true, i_isAffectSkeleton: false);
		m_npc.GetAnimator().speed = 1f;
		m_npc.GetSkeletonActor().SetToRape();
		m_player.SetRaperRaping(this);
		m_player.SetIsBeingRaped(i_isBeingRaped: true);
		m_player.InterruptReload();
		m_litreCumTaken = 0f;
		if (m_npc.GetStateActorCurrent() == StateActor.Ragdoll)
		{
			m_npc.GetSkeletonActor().DisableRagdoll();
		}
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_player.StopMoving();
		if (m_isInverseFlipX)
		{
			if (m_npc.GetIsFacingLeft())
			{
				m_npc.SetIsFacingLeft(i_isFacingLeft: false);
			}
			else
			{
				m_npc.SetIsFacingLeft(i_isFacingLeft: true);
			}
		}
		m_player.SetIsFacingLeft(m_npc.GetIsFacingLeft());
		m_player.GetSkeletonPlayer().ResetArmSorting();
		m_animatorPlayerOriginal = m_player.GetAnimator().runtimeAnimatorController;
		m_player.GetAnimator().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Raper/RapeAnimator");
		if (m_isDropVictimWeapon)
		{
			m_player.DropEquippedEquippable(0.25f);
		}
		else if (m_isShowVictimWeapon)
		{
			m_player.ShowEquippedWeapon();
		}
		else
		{
			m_player.HideEquippedWeapon();
		}
		m_npc.StopMoving();
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().CenterCamera();
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetSmoothLevel(SmoothLevel.None);
		CommonReferences.Instance.GetManagerHud().RapeHud();
		if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
		{
			CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
		}
		if (m_isPositionToPlayer)
		{
			m_npc.transform.parent = m_player.transform;
			m_npc.transform.localPosition = new Vector3(0f, m_npc.transform.localPosition.y, m_npc.transform.localPosition.z);
			if (m_isCenterYRaper)
			{
				m_npc.transform.localPosition = new Vector3(m_npc.transform.localPosition.x, 0f, m_npc.transform.localPosition.z);
			}
			else
			{
				m_npc.PlaceFeetOnPos(new Vector2(m_npc.GetPos().x, m_player.GetPosFeet().y));
			}
		}
		else
		{
			m_player.transform.parent = m_npc.transform;
			m_player.transform.localPosition = new Vector3(0f, m_player.transform.localPosition.y, m_player.transform.localPosition.z);
			if (m_isCenterYPlayer)
			{
				m_player.transform.localPosition = new Vector3(m_player.transform.localPosition.x, 0f, m_player.transform.localPosition.z);
			}
			else
			{
				m_player.PlaceFeetOnPos(new Vector2(m_player.GetPos().x, m_npc.GetPosFeet().y));
			}
		}
		if (m_isKinematicPlayer)
		{
			m_player.GetRigidbody2D().isKinematic = true;
		}
		if (m_isKinematicRaper)
		{
			m_npc.GetRigidbody2D().isKinematic = true;
		}
		if (m_colliderRape != null)
		{
			m_colliderRape.gameObject.SetActive(value: true);
			m_colliderRape.enabled = true;
		}
		if (m_lightsRape != null)
		{
			foreach (Light item in m_lightsRape)
			{
				item.gameObject.SetActive(value: true);
				item.enabled = true;
			}
		}
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetFOVOriginal() / 2f, 0.25f);
		if (this.OnStartRape != null)
		{
			this.OnStartRape();
		}
		m_npc.GetAnimator().speed = 1f;
		if (m_isDisableAnimatorLayersRaper)
		{
			int layerCount = m_npc.GetAnimator().layerCount;
			for (int i = 0; i < layerCount; i++)
			{
				if (i != 0)
				{
					m_npc.GetAnimator().SetLayerWeight(i, 0f);
				}
			}
		}
		if (!m_player.IsDead())
		{
			StartCoroutine(CoroutineFreezeBeforeStartRape());
		}
		PlayAnimations();
		m_isPlayerOrgasmDuringRape = false;
		if (m_textsSpeechPlayerOrgasm.Count > 0)
		{
			m_player.OnOrgasm += CreateSpeechBubblePlayerOrgasm;
		}
		m_probabilityCreateSpeechBubble01 = 0f;
		StartCoroutine(CoroutineProbilitySpeechBubble());
		m_player.GetSkeletonPlayer().GetManagerFacePlayer().ApplyRaper(this);
		if (m_npc.GetId() != -1)
		{
			ManagerDB.Rape(m_npc);
		}
	}

	public virtual void BeginRapeArchive()
	{
		m_isArchiveMode = true;
		m_posZOriginal = base.transform.position.z;
		Vector3 position = base.transform.position;
		position.z += -0.1f;
		base.transform.position = position;
		m_isCanRape = false;
		m_isRaping = true;
		m_npc.SetIsCanAttack(i_isCanAttack: false);
		m_npc.SetIsThinking(i_isThinking: false);
		m_npc.SetIsInvulnerable(i_isInvulnerable: true, i_isAffectSkeleton: false);
		m_npc.GetAnimator().speed = 1f;
		m_player.SetRaperRaping(this);
		m_player.SetIsBeingRaped(i_isBeingRaped: true);
		m_player.StopMoving();
		m_player.gameObject.SetActive(value: false);
		m_npc.StopMoving();
		if (m_colliderRape != null)
		{
			m_colliderRape.gameObject.SetActive(value: true);
			m_colliderRape.enabled = true;
		}
		m_npc.GetComponent<SpriteRenderer>().sortingOrder = 39;
		if (this.OnStartRape != null)
		{
			this.OnStartRape();
		}
		DisableAllVisualSystemsInChildren();
		m_npc.GetAnimator().speed = 1f;
		StartRapePlayerArchive();
	}

	protected virtual IEnumerator CoroutineFreezeBeforeStartRape()
	{
		Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(0.5f);
		Time.timeScale = 1f;
		HandleRape();
	}

	private IEnumerator CoroutineFlashRed()
	{
		while (true)
		{
			m_npc.GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0.5f, 1f);
			yield return new WaitForSecondsRealtime(0.08f);
			m_npc.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
			yield return new WaitForSecondsRealtime(0.08f);
		}
	}

	private void PlayAnimations()
	{
		m_coroutinePlayAnimations = StartCoroutine(CoroutinePlayAnimations());
	}

	private IEnumerator CoroutinePlayAnimations()
	{
		foreach (RaperAnimation raperAnimation in m_raperAnimations)
		{
			m_npc.GetAnimator().Play(raperAnimation.GetNameStateAnimationRaper());
			m_player.GetAnimator().Play(raperAnimation.GetNameStateAnimationPlayer());
			m_raperAnimationCurrent = raperAnimation;
			if (raperAnimation.IsCameraFollowHipsNpc())
			{
				CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(m_npc.GetSkeleton().GetBone("hips").gameObject);
			}
			else
			{
				CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(m_player.GetPosMiddleFace());
			}
			if (raperAnimation.IsAttachToPlayerBone())
			{
				PlaceRaperOnPlayerBone();
			}
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetIsRotateWithFocusedObject(raperAnimation.IsCameraRotateWithTarget());
			if (!m_isPlayerMuteBeforeRape)
			{
				m_player.SetIsMute(raperAnimation.IsMutePlayer());
			}
			this.OnAdvanceRaperAnimation?.Invoke();
			float l_timeToWait = raperAnimation.GetDurationSecondsAnimClipRaper();
			yield return new WaitForSecondsRealtime(0.1f);
			l_timeToWait -= 0.1f;
			Timer timer = new Timer(l_timeToWait);
			yield return timer.CoroutinePlayAndWaitForEnd();
		}
		EndRape();
	}

	private void PlaceRaperOnPlayerBone()
	{
		m_npc.transform.parent = m_player.GetSkeletonPlayer().GetBone(m_raperAnimationCurrent.GetBonePlayerToAttachTo()).transform;
		m_npc.transform.localPosition = m_raperAnimationCurrent.GetOffsetLocalPositionAttachment();
		m_npc.transform.localEulerAngles = m_raperAnimationCurrent.GetOffsetLocalEulerAnglesAttachment();
	}

	protected abstract void HandleRape();

	private void StartRapePlayerArchive()
	{
		m_npc.GetAnimator().Play("Rape");
	}

	private void PlayAudio(AudioClip i_audio)
	{
		m_managerAudio.PlayAudioSFX(i_audio);
	}

	private void PlayRandomAudioPlayerLayer1()
	{
		if (m_raperAnimationCurrent.GetAudiosPlayerLayer1().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosPlayerLayer1(), m_raperAnimationCurrent.GetChanceOfPlayingAudioPlayerLayer1());
		}
	}

	private void PlayRandomAudioPlayerLayer2()
	{
		if (m_raperAnimationCurrent.GetAudiosPlayerLayer2().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosPlayerLayer2(), m_raperAnimationCurrent.GetChanceOfPlayingAudioPlayerLayer2());
		}
	}

	private void PlayRandomAudioPlayerLayer3()
	{
		if (m_raperAnimationCurrent.GetAudiosPlayerLayer3().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosPlayerLayer3(), m_raperAnimationCurrent.GetChanceOfPlayingAudioPlayerLayer3());
		}
	}

	private void PlayRandomAudioRaper1()
	{
		if (m_raperAnimationCurrent.GetAudiosRaperLayer1().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosRaperLayer1(), m_raperAnimationCurrent.GetChanceOfPlayingAudioRaperLayer1());
		}
	}

	private void PlayRandomAudioRaper2()
	{
		if (m_raperAnimationCurrent.GetAudiosRaperLayer2().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosRaperLayer2(), m_raperAnimationCurrent.GetChanceOfPlayingAudioRaperLayer2());
		}
	}

	private void PlayRandomAudioRaper3()
	{
		if (m_raperAnimationCurrent.GetAudiosRaperLayer3().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosRaperLayer3(), m_raperAnimationCurrent.GetChanceOfPlayingAudioRaperLayer3());
		}
	}

	private void PlayRandomAudioRaperVoice()
	{
		if (m_raperAnimationCurrent.GetAudiosRaperVoice().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosRaperVoice(), m_raperAnimationCurrent.GetChanceOfPlayingAudioRaperVoice());
		}
	}

	private void PlayRandomAudioPlayerVoice()
	{
		if (m_raperAnimationCurrent.GetAudiosPlayerVoice().Count > 0 && Random.Range(0f, 100f) <= (float)m_raperAnimationCurrent.GetChanceOfPlayingAudioPlayerVoice())
		{
			int index = Random.Range(0, m_raperAnimationCurrent.GetAudiosPlayerVoice().Count);
			m_player.PlayAudioVoice(m_raperAnimationCurrent.GetAudiosPlayerVoice()[index]);
			this.OnPlayerVoice?.Invoke();
		}
	}

	private void PlayRandomAudioThrust()
	{
		if (m_raperAnimationCurrent.GetAudiosThrust().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosThrust(), 100f);
		}
	}

	private void PlayRandomAudioCum()
	{
		if (m_raperAnimationCurrent.GetAudiosCum().Count > 0)
		{
			m_managerAudio.PlayAudioSFXRandom(m_raperAnimationCurrent.GetAudiosCum(), 100f);
		}
	}

	private void PlayAudioUnique(int i_numAudio)
	{
		i_numAudio--;
		if (m_raperAnimationCurrent.GetAudiosUnique().Count == 0)
		{
			Debug.Log("m_audiosUnique is empty, not playing audioUnique[" + i_numAudio + "]");
		}
		else if (i_numAudio > m_raperAnimationCurrent.GetAudiosUnique().Count)
		{
			Debug.Log("Index for audio unique not right: " + (i_numAudio + 1));
		}
		else if (m_raperAnimationCurrent.GetAudiosUnique()[i_numAudio] != null)
		{
			m_managerAudio.PlayAudioSFX(m_raperAnimationCurrent.GetAudiosUnique()[i_numAudio]);
		}
	}

	protected virtual void Thrust(int i_power0to3)
	{
		if (!m_isRaping)
		{
			return;
		}
		if (this.OnThrust != null)
		{
			this.OnThrust();
		}
		PlayerGainPleasure();
		PlayRandomAudioThrust();
		PlayRandomAudioPlayerVoice();
		PlayRandomAudioPlayerLayer1();
		PlayRandomAudioPlayerLayer2();
		PlayRandomAudioPlayerLayer3();
		PlayRandomAudioRaperVoice();
		PlayRandomAudioRaper1();
		PlayRandomAudioRaper2();
		PlayRandomAudioRaper3();
		float i_shakeAmount = 0f;
		switch (i_power0to3)
		{
		case 0:
			i_shakeAmount = 0.15f;
			break;
		case 1:
			i_shakeAmount = 0.25f;
			break;
		case 2:
			i_shakeAmount = 0.6f;
			break;
		case 3:
			i_shakeAmount = 0.75f;
			break;
		}
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().Shake(i_shakeAmount, 0.4f);
		if (m_raperAnimationCurrent.GetParticlesThrust().Count > 0)
		{
			foreach (RapeParticleSystem item in m_raperAnimationCurrent.GetParticlesThrust())
			{
				ParticleSystem particleSystem = CommonReferences.Instance.GetUtilityTools().CreateParticleDuplicateAndPlay(item.GetParticleSystem(), item.GetParentBone());
				particleSystem.transform.localPosition = Vector3.zero;
				particleSystem.transform.localEulerAngles = Vector3.zero;
				m_particlesThrustDuplicates.Add(particleSystem);
				CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, particleSystem.main.duration + 5f);
			}
		}
		CreateSpeechBubbleThrust();
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectThrust(m_player.GetLibidoCurrent());
		if (m_raperAnimationCurrent.IsHasOral())
		{
			m_player.GetSkeletonPlayer().HandleOralThrust();
		}
		if (m_raperAnimationCurrent.GetLitreCumPerThrust() > 0f)
		{
			ManagerDB.AddLitreCumTaken(m_npc, m_raperAnimationCurrent.GetLitreCumPerThrust());
		}
	}

	private IEnumerator CoroutineProbilitySpeechBubble()
	{
		while (true)
		{
			m_probabilityCreateSpeechBubble01 += 0.1f;
			if (m_probabilityCreateSpeechBubble01 > 1f)
			{
				m_probabilityCreateSpeechBubble01 = 1f;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void CreateSpeechBubbleThrust()
	{
		if (Random.Range(0f, 1f) < 1f - m_probabilityCreateSpeechBubble01)
		{
			return;
		}
		m_probabilityCreateSpeechBubble01 -= 0.2f;
		if (m_probabilityCreateSpeechBubble01 < 0f)
		{
			m_probabilityCreateSpeechBubble01 = 0f;
		}
		List<string> list = new List<string>();
		int num = 0;
		if (m_isPlayerOrgasmDuringRape || m_player.IsDead())
		{
			list = m_textsSpeechThrust75to100;
		}
		else
		{
			if (m_player.GetPleasureCurrent() >= 0f)
			{
				list = m_textsSpeechThrust0to24;
				num = 0;
			}
			if (m_player.GetPleasureCurrent() >= 25f)
			{
				list = m_textsSpeechThrust25to49;
				num = 1;
			}
			if (m_player.GetPleasureCurrent() >= 50f)
			{
				list = m_textsSpeechThrust50to74;
				num = 2;
			}
			if (m_player.GetPleasureCurrent() >= 75f)
			{
				list = m_textsSpeechThrust75to100;
				num = 3;
			}
		}
		if (list.Count < 1)
		{
			return;
		}
		SpeechBubbleTextColor i_textColor = SpeechBubbleTextColor.Pleasure1;
		if (m_isPlayerOrgasmDuringRape || m_player.IsDead())
		{
			i_textColor = SpeechBubbleTextColor.Pleasure4;
		}
		else
		{
			switch (num)
			{
			case 0:
				i_textColor = SpeechBubbleTextColor.Pleasure1;
				break;
			case 1:
				i_textColor = SpeechBubbleTextColor.Pleasure2;
				break;
			case 2:
				i_textColor = SpeechBubbleTextColor.Pleasure3;
				break;
			case 3:
				i_textColor = SpeechBubbleTextColor.Pleasure4;
				break;
			}
		}
		int index = Random.Range(0, list.Count);
		CommonReferences.Instance.GetManagerHud().CreateSpeechBubble(list[index], i_textColor, GetPosSpawnSpeechBubble(), GetIsSpeechBubbleLeft());
	}

	private Vector2 GetPosSpawnSpeechBubble()
	{
		Vector2 result = m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Head).transform.position;
		if (GetIsSpeechBubbleLeft())
		{
			result.x -= 1.5f;
		}
		else
		{
			result.x += 1.5f;
		}
		return result;
	}

	public bool GetIsSpeechBubbleLeft()
	{
		if (m_player.GetIsFacingLeft())
		{
			if (m_isPlayerHeadFacingLeft)
			{
				return false;
			}
			return true;
		}
		return m_isPlayerHeadFacingLeft;
	}

	private void CreateSpeechBubblePlayerOrgasm()
	{
		m_isPlayerOrgasmDuringRape = true;
		int index = Random.Range(0, m_textsSpeechPlayerOrgasm.Count);
		CommonReferences.Instance.GetManagerHud().CreateSpeechBubble(m_textsSpeechPlayerOrgasm[index], SpeechBubbleTextColor.Orgasm, GetPosSpawnSpeechBubble(), GetIsSpeechBubbleLeft());
	}

	protected virtual void CumThrust(int i_power0to3)
	{
		if (!m_isRaping)
		{
			return;
		}
		if (this.OnCumThrust != null)
		{
			this.OnCumThrust();
		}
		PlayerGainCumPleasure();
		PlayRandomAudioThrust();
		PlayRandomAudioCum();
		PlayRandomAudioPlayerVoice();
		PlayRandomAudioPlayerLayer1();
		PlayRandomAudioPlayerLayer2();
		PlayRandomAudioPlayerLayer3();
		PlayRandomAudioRaperVoice();
		PlayRandomAudioRaper1();
		PlayRandomAudioRaper2();
		PlayRandomAudioRaper3();
		float i_shakeAmount = 0f;
		switch (i_power0to3)
		{
		case 0:
			i_shakeAmount = 0.3f;
			break;
		case 1:
			i_shakeAmount = 0.5f;
			break;
		case 2:
			i_shakeAmount = 0.75f;
			break;
		case 3:
			i_shakeAmount = 1f;
			break;
		}
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().Shake(i_shakeAmount, 0.4f);
		if (m_raperAnimationCurrent.GetParticlesCumThrust().Count > 0)
		{
			foreach (RapeParticleSystem item in m_raperAnimationCurrent.GetParticlesCumThrust())
			{
				ParticleSystem particleSystem = CommonReferences.Instance.GetUtilityTools().CreateParticleDuplicateAndPlay(item.GetParticleSystem(), item.GetParentBone());
				particleSystem.transform.localPosition = Vector3.zero;
				particleSystem.transform.localEulerAngles = Vector3.zero;
				m_particlesThrustDuplicates.Add(particleSystem);
				CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, particleSystem.main.duration + 5f);
			}
		}
		CreateSpeechBubbleCumThrust();
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectThrust(m_player.GetLibidoCurrent());
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectCum();
		if (m_raperAnimationCurrent.IsHasOral())
		{
			m_player.GetSkeletonPlayer().HandleOralThrust();
		}
		if (m_raperAnimationCurrent.GetLitreCumPerCumThrust() > 0f)
		{
			ManagerDB.AddLitreCumTaken(m_npc, m_raperAnimationCurrent.GetLitreCumPerCumThrust());
		}
	}

	private void CreateSpeechBubbleCumThrust()
	{
		if (m_isUseProbabilityForSpeechCumTrust)
		{
			if (Random.Range(0f, 1f) < 1f - m_probabilityCreateSpeechBubble01)
			{
				return;
			}
			m_probabilityCreateSpeechBubble01 -= 0.2f;
			if (m_probabilityCreateSpeechBubble01 < 0f)
			{
				m_probabilityCreateSpeechBubble01 = 0f;
			}
		}
		List<string> list = new List<string>();
		int num = 0;
		if (m_isPlayerOrgasmDuringRape || m_player.IsDead())
		{
			list = m_textsSpeechCumThrust75to100;
		}
		else
		{
			if (m_player.GetPleasureCurrent() >= 0f)
			{
				list = m_textsSpeechCumThrust0to24;
				num = 0;
			}
			if (m_player.GetPleasureCurrent() >= 25f)
			{
				list = m_textsSpeechCumThrust25to49;
				num = 1;
			}
			if (m_player.GetPleasureCurrent() >= 50f)
			{
				list = m_textsSpeechCumThrust50to74;
				num = 2;
			}
			if (m_player.GetPleasureCurrent() >= 75f)
			{
				list = m_textsSpeechCumThrust75to100;
				num = 3;
			}
		}
		if (list.Count < 1)
		{
			return;
		}
		SpeechBubbleTextColor i_textColor = SpeechBubbleTextColor.Pleasure1;
		if (m_isPlayerOrgasmDuringRape || m_player.IsDead())
		{
			i_textColor = SpeechBubbleTextColor.Pleasure4;
		}
		else
		{
			switch (num)
			{
			case 0:
				i_textColor = SpeechBubbleTextColor.Pleasure1;
				break;
			case 1:
				i_textColor = SpeechBubbleTextColor.Pleasure2;
				break;
			case 2:
				i_textColor = SpeechBubbleTextColor.Pleasure3;
				break;
			case 3:
				i_textColor = SpeechBubbleTextColor.Pleasure4;
				break;
			}
		}
		int index = Random.Range(0, list.Count);
		CommonReferences.Instance.GetManagerHud().CreateSpeechBubble(list[index], i_textColor, GetPosSpawnSpeechBubble(), GetIsSpeechBubbleLeft());
	}

	private void PlayParticleUnique(int i_numParticleSystem)
	{
		i_numParticleSystem--;
		if (m_raperAnimationCurrent.GetParticlesUnique().Count == 0)
		{
			Debug.Log("m_particlesUnique is empty, not playing m_particlesUnique[" + i_numParticleSystem + "]");
		}
		else if (i_numParticleSystem > m_raperAnimationCurrent.GetParticlesUnique().Count)
		{
			Debug.Log("Index for m_particlesUnique not right: " + (i_numParticleSystem + 1));
		}
		else if (m_raperAnimationCurrent.GetParticlesUnique()[i_numParticleSystem] != null)
		{
			ParticleSystem particleSystem = CommonReferences.Instance.GetUtilityTools().CreateParticleDuplicateAndPlay(m_raperAnimationCurrent.GetParticlesUnique()[i_numParticleSystem].GetParticleSystem(), m_raperAnimationCurrent.GetParticlesUnique()[i_numParticleSystem].GetParentBone());
			particleSystem.transform.localPosition = Vector3.zero;
			particleSystem.transform.localEulerAngles = Vector3.zero;
			m_particlesThrustDuplicates.Add(particleSystem);
			CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, particleSystem.main.duration + 5f);
		}
	}

	private void PlayerGainPleasure()
	{
		m_player.GainLibido(m_raperAnimationCurrent.GetPowerThrustLibido01() * 10f);
		m_player.GainPleasure(m_raperAnimationCurrent.GetPowerThrustPleasure01() * 2f);
		m_player.DamageStrength((m_raperAnimationCurrent.GetPowerThrustLibido01() + m_raperAnimationCurrent.GetPowerThrustPleasure01()) * 5f);
	}

	private void PlayerGainCumPleasure()
	{
		m_player.GainLibido(m_raperAnimationCurrent.GetPowerThrustLibido01() * 10f + m_raperAnimationCurrent.GetAdditionalPowerCumThrustLibido01() * 10f);
		m_player.GainPleasure(m_raperAnimationCurrent.GetPowerThrustPleasure01() * 2f + m_raperAnimationCurrent.GetAdditionalPowerCumThrustPleasure01() * 2f);
		m_player.DamageStrength((m_raperAnimationCurrent.GetPowerThrustLibido01() + m_raperAnimationCurrent.GetAdditionalPowerCumThrustLibido01() + m_raperAnimationCurrent.GetPowerThrustPleasure01() + m_raperAnimationCurrent.GetAdditionalPowerCumThrustPleasure01()) * 5f);
	}

	protected virtual void EndRape()
	{
		if (m_isArchiveMode)
		{
			EndRapeArchive();
			return;
		}
		if (this.OnEndRape != null)
		{
			this.OnEndRape();
		}
		StopCoroutine(m_coroutinePlayAnimations);
		m_npc.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
			.transform;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, m_posZOriginal);
		m_npc.transform.localEulerAngles = Vector3.zero;
		m_player.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
			.transform;
		m_isRaping = false;
		m_player.SetIsBeingRaped(i_isBeingRaped: false);
		m_player.SetIsMute(m_isPlayerMuteBeforeRape);
		if (m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform.localScale.x == -1f)
		{
			Vector3 position = m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform.position;
			if (m_player.GetIsFacingLeft())
			{
				m_player.SetIsFacingLeft(i_isFacingLeft: false);
			}
			else
			{
				m_player.SetIsFacingLeft(i_isFacingLeft: true);
			}
			m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform.position = position;
		}
		m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform.localScale = new Vector3(1f, 1f, 1f);
		m_npc.SetState(StateActor.Idle);
		m_npc.GetAnimator().Play("Idle");
		m_npc.GetSkeletonActor().SetToDefault();
		m_npc.GetSkeleton().SetBodyPartSortOrdersToOriginal();
		if (GetIsCloseToWallLeft() || GetIsCloseToWallRight())
		{
			if (GetIsCloseToWallLeft())
			{
				m_player.SetPosX(m_player.GetPos().x + 1f);
			}
			if (GetIsCloseToWallRight())
			{
				m_player.SetPosX(m_player.GetPos().x - 1f);
			}
		}
		if (m_colliderRape != null)
		{
			m_colliderRape.gameObject.SetActive(value: false);
			m_colliderRape.enabled = false;
		}
		if (m_lightsRape != null)
		{
			foreach (Light item in m_lightsRape)
			{
				item.gameObject.SetActive(value: false);
				item.enabled = false;
			}
		}
		if (m_isKinematicPlayer)
		{
			m_player.GetRigidbody2D().isKinematic = false;
		}
		if (m_isKinematicRaper)
		{
			m_npc.GetRigidbody2D().isKinematic = false;
		}
		if (m_isDisableAnimatorLayersRaper)
		{
			int layerCount = m_npc.GetAnimator().layerCount;
			for (int i = 0; i < layerCount; i++)
			{
				if (i != 0)
				{
					m_npc.GetAnimator().SetLayerWeight(i, 1f);
				}
			}
		}
		if (!m_isDropVictimWeapon && !m_isShowVictimWeapon)
		{
			m_player.ShowEquippedWeapon();
		}
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(m_player.gameObject);
		CommonReferences.Instance.GetManagerAudio().SetVolumesToDefault();
		if (m_player.IsDead())
		{
			CommonReferences.Instance.GetManagerHud().DeadHud();
		}
		else
		{
			CommonReferences.Instance.GetManagerHud().ShowHud();
		}
		m_posePlayerOld = m_player.GetSkeleton().CopyCurrentPose();
		m_player.GetAnimator().runtimeAnimatorController = m_animatorPlayerOriginal;
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		if (m_forceOnVictimOnRapeEnd != Vector2.zero)
		{
			if (m_player.GetIsFacingLeft())
			{
				m_player.GetRigidbody2D().AddForce(new Vector2(0f - m_forceOnVictimOnRapeEnd.x, m_forceOnVictimOnRapeEnd.y), ForceMode2D.Impulse);
			}
			else
			{
				m_player.GetRigidbody2D().AddForce(new Vector2(m_forceOnVictimOnRapeEnd.x, m_forceOnVictimOnRapeEnd.y), ForceMode2D.Impulse);
			}
		}
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetFOVOriginal(), 0.5f);
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetSmoothLevel(SmoothLevel.Low);
		RecoverVictimFromRape();
		m_npc.SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: false);
		m_npc.GetRigidbody2D().isKinematic = false;
		m_player.GetComponent<Collider2D>().enabled = true;
		m_player.GetSkeleton().ApplyPose(m_posePlayerOld);
		m_posePlayerOld.Clear();
		m_player.OnOrgasm -= CreateSpeechBubblePlayerOrgasm;
		if (base.isActiveAndEnabled)
		{
			m_npc.SetIsThinking(i_isThinking: true);
			m_npc.SetIsCanAttack(i_isCanAttack: true);
			m_isCanRape = true;
			RecoverFromRape();
		}
		if (m_isDestroySelfAfterRape)
		{
			Object.Destroy(base.gameObject);
		}
		LowerLibidoSpecies();
		m_npc.GetId();
		_ = -1;
	}

	private void EndRapeArchive()
	{
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, m_posZOriginal);
		m_isRaping = false;
		m_player.SetIsBeingRaped(i_isBeingRaped: false);
		m_npc.SetState(StateActor.Idle);
		m_npc.GetAnimator().Play("Idle");
		if (m_colliderRape != null)
		{
			m_colliderRape.gameObject.SetActive(value: false);
			m_colliderRape.enabled = false;
		}
		EnableAllVisualSystemsInChildren();
		StopThrustParticles();
		m_npc.SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: false);
		if (this.OnEndRape != null)
		{
			this.OnEndRape();
		}
		if (base.isActiveAndEnabled)
		{
			m_npc.SetIsThinking(i_isThinking: true);
			m_isCanRape = true;
		}
	}

	private void LowerLibidoSpecies()
	{
		foreach (NPC allNPC in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs())
		{
			if (allNPC.GetName() == m_npc.GetName() && (bool)allNPC.GetRaper())
			{
				allNPC.GetRaper().SetLibido01(0f);
			}
		}
	}

	private IEnumerator CoroutineUpdateLibido()
	{
		float l_increaseAmountMax = 0.5f;
		float l_increaseAmountMin = 0.001f;
		float l_increaseDelay = 15f;
		yield return new WaitForSeconds(l_increaseDelay);
		while (true)
		{
			if (m_libido01 < 1f)
			{
				float num = l_increaseAmountMax / (float)GetNumOfSpecies();
				if (num <= 0f)
				{
					num = l_increaseAmountMin;
				}
				m_libido01 += num;
				if (m_libido01 > 1f)
				{
					m_libido01 = 1f;
				}
				yield return new WaitForSeconds(l_increaseDelay);
			}
			else
			{
				yield return new WaitForFixedUpdate();
			}
		}
	}

	private int GetNumOfSpecies()
	{
		int num = 0;
		foreach (NPC allNPC in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs())
		{
			if (allNPC.GetName() == m_npc.GetName())
			{
				num++;
			}
		}
		return num;
	}

	private bool GetIsCloseToWallLeft()
	{
		int mask = LayerMask.GetMask("Platform", "Interactable");
		if ((bool)Physics2D.Raycast(m_player.GetPos(), -Vector2.right, 1f, mask))
		{
			return true;
		}
		return false;
	}

	private bool GetIsCloseToWallRight()
	{
		int mask = LayerMask.GetMask("Platform", "Interactable");
		if ((bool)Physics2D.Raycast(m_player.GetPos(), Vector2.right, 1f, mask))
		{
			return true;
		}
		return false;
	}

	protected abstract void RecoverFromRape();

	protected virtual void RecoverVictimFromRape()
	{
		m_player.RecoverFromRape();
	}

	private void Impregnate()
	{
		if (m_actorsToInsert.Count < 1)
		{
			Debug.Log("No fetuses assigned to insert!");
			return;
		}
		float num = m_durationPregnant * m_offsetRandomDurationPregnant01;
		float num2 = m_durationPregnant + Random.Range(0f - num, num);
		if (m_isEgg)
		{
			Egg egg = Object.Instantiate(m_egg);
			egg.Initialize(m_actorsToInsert, m_nameFetus);
			m_player.AddEgg(m_npc, egg, num2, m_nameFetus);
			CreateSpeechBubbleImpregnate();
			return;
		}
		if (m_isFetusToInsertRandomChosen)
		{
			Actor i_actorToMakeFetus = m_actorsToInsert[Random.Range(0, m_actorsToInsert.Count)];
			m_player.CreateAndAddFetus(m_npc, i_actorToMakeFetus, num2, m_nameFetus);
		}
		else
		{
			m_player.CreateAndAddFetus(m_npc, m_actorsToInsert, num2, m_nameFetus);
		}
		CreateSpeechBubbleImpregnate();
	}

	private void KOPlayer()
	{
		m_player.GetSkeletonPlayer().GetManagerFacePlayer().KO();
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectRagdoll();
		Library.Instance.Actors.TriggerInteraction("Choked Unconscious", m_npc);
	}

	private void DropPlayerWeapon()
	{
	}

	private void CreateSpeechBubbleImpregnate()
	{
		List<string> list = new List<string>();
		int num = 0;
		if (m_isPlayerOrgasmDuringRape || m_player.IsDead())
		{
			list = m_textsSpeechImpregnation75to100;
		}
		else
		{
			if (m_player.GetPleasureCurrent() >= 0f)
			{
				list = m_textsSpeechImpregnation0to24;
				num = 0;
			}
			if (m_player.GetPleasureCurrent() >= 25f)
			{
				list = m_textsSpeechImpregnation25to49;
				num = 1;
			}
			if (m_player.GetPleasureCurrent() >= 50f)
			{
				list = m_textsSpeechImpregnation50to74;
				num = 2;
			}
			if (m_player.GetPleasureCurrent() >= 75f)
			{
				list = m_textsSpeechImpregnation75to100;
				num = 3;
			}
		}
		if (list.Count < 1)
		{
			return;
		}
		SpeechBubbleTextColor i_textColor = SpeechBubbleTextColor.Pleasure1;
		if (m_isPlayerOrgasmDuringRape || m_player.IsDead())
		{
			i_textColor = SpeechBubbleTextColor.Pleasure4;
		}
		else
		{
			switch (num)
			{
			case 0:
				i_textColor = SpeechBubbleTextColor.Pleasure1;
				break;
			case 1:
				i_textColor = SpeechBubbleTextColor.Pleasure2;
				break;
			case 2:
				i_textColor = SpeechBubbleTextColor.Pleasure3;
				break;
			case 3:
				i_textColor = SpeechBubbleTextColor.Pleasure4;
				break;
			}
		}
		int index = Random.Range(0, list.Count);
		CommonReferences.Instance.GetManagerHud().CreateSpeechBubble(list[index], i_textColor, GetPosSpawnSpeechBubble(), GetIsSpeechBubbleLeft());
	}

	private void StopThrustParticles()
	{
		foreach (ParticleSystem particlesThrustDuplicate in m_particlesThrustDuplicates)
		{
			if (particlesThrustDuplicate != null)
			{
				particlesThrustDuplicate.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
			}
		}
	}

	private void EnableAllVisualSystemsInChildren()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			bool flag = true;
			foreach (RapeParticleSystem item in m_raperAnimationCurrent.GetParticlesThrust())
			{
				if (particleSystem == item)
				{
					flag = false;
				}
			}
			foreach (RapeParticleSystem item2 in m_raperAnimationCurrent.GetParticlesCumThrust())
			{
				if (particleSystem == item2)
				{
					flag = false;
				}
			}
			foreach (RapeParticleSystem item3 in m_raperAnimationCurrent.GetParticlesUnique())
			{
				if (particleSystem == item3)
				{
					flag = false;
				}
			}
			if (flag)
			{
				particleSystem.gameObject.SetActive(value: true);
			}
		}
		Light[] componentsInChildren2 = GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren2)
		{
			if (!m_lightsRape.Contains(light) && !m_lightsRape.Contains(light))
			{
				light.gameObject.SetActive(value: true);
			}
		}
	}

	private void DisableAllVisualSystemsInChildren()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			bool flag = true;
			foreach (RapeParticleSystem item in m_raperAnimationCurrent.GetParticlesThrust())
			{
				if (particleSystem == item)
				{
					flag = false;
				}
			}
			foreach (RapeParticleSystem item2 in m_raperAnimationCurrent.GetParticlesCumThrust())
			{
				if (particleSystem == item2)
				{
					flag = false;
				}
			}
			foreach (RapeParticleSystem item3 in m_raperAnimationCurrent.GetParticlesUnique())
			{
				if (particleSystem == item3)
				{
					flag = false;
				}
			}
			if (flag)
			{
				particleSystem.gameObject.SetActive(value: false);
			}
		}
		Light[] componentsInChildren2 = GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren2)
		{
			if (!m_lightsRape.Contains(light) && !m_lightsRape.Contains(light))
			{
				light.gameObject.SetActive(value: false);
			}
		}
	}

	public bool GetIsRaping()
	{
		return m_isRaping;
	}

	public bool GetIsCanRape()
	{
		return m_isCanRape;
	}

	protected string GetNameAnimRapeNPC()
	{
		return "Rape";
	}

	protected string GetNameAnimRapeCumNPC()
	{
		return "RapeCum";
	}

	protected string GetNameAnimRapePlayer()
	{
		return m_npc.GetName() + "Rape";
	}

	protected string GetNameAnimRapeCumPlayer()
	{
		return m_npc.GetName() + "RapeCum";
	}

	public void ForceStartCum()
	{
		StopAllCoroutines();
	}

	public void ForceEndRape()
	{
		StopAllCoroutines();
		EndRape();
	}

	public List<Actor> GetFetusesToInsert()
	{
		return m_actorsToInsert;
	}

	public DefeatScene GetDefeatScene()
	{
		return m_defeatScene;
	}

	public bool GetIsHasOral()
	{
		return m_raperAnimationCurrent.IsHasOral();
	}

	public NPC GetNPC()
	{
		return m_npc;
	}

	public float GetLibido01()
	{
		return m_libido01;
	}

	public void SetLibido01(float i_libido01)
	{
		m_libido01 = i_libido01;
	}

	public RaperAnimation GetRaperAnimationCurrent()
	{
		return m_raperAnimationCurrent;
	}
}
