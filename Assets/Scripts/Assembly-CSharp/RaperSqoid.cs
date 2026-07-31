using System.Collections;
using UnityEngine;

public class RaperSqoid : RaperSmasher
{
	private StatusPlayerHudItem m_statusItemHypnotize;

	private Sqoid m_sqoid;

	protected override void Start()
	{
		base.Start();
		m_sqoid = (Sqoid)m_npc;
	}

	private void Electrocute()
	{
		m_player.GetSkeleton().EnableElectrocute(0f, Color.green);
		m_sqoid.PlayAudioSFX(m_sqoid.GetAudioZap());
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectElectrocute();
		StartCoroutine(CoroutineWaitDisableElectrocute());
	}

	private IEnumerator CoroutineWaitDisableElectrocute()
	{
		yield return new WaitForSeconds(1f);
		m_player.GetSkeleton().DisableElectrocute();
		yield return new WaitForSeconds(0.5f);
		m_player.GetSkeleton().DisableElectrocute();
	}

	private void EnableHypnotize()
	{
		m_player.EnableHypnotized();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_sqoid.GetAudioHypnotize());
		m_statusItemHypnotize = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("?&$aam101101_", "Wh@t 1s h4pp1n1ng,..,.?/", StatusPlayerHudItemColor.Special);
		CommonReferences.Instance.GetManagerPostProcessing().GetEffectHypnotize().weight = 1f;
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetIsRotateWithFocusedObject(i_isRotate: true);
		m_npc.GetAllInteractions()[1].Trigger(m_npc);
		base.OnEndRape += DisableHypnotize;
	}

	private void DisableHypnotize()
	{
		base.OnEndRape -= DisableHypnotize;
		m_player.DisableHypnotized();
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().DestroyStatusItem(m_statusItemHypnotize);
		CommonReferences.Instance.GetManagerPostProcessing().GetEffectHypnotize().weight = 0f;
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetIsRotateWithFocusedObject(i_isRotate: false);
	}
}
