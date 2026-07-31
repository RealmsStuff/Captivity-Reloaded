using UnityEngine;

public class Generator : ButtonActivator
{
	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		base.HandleActivation(i_initiator, i_activationType);
		GetComponent<AudioSource>().volume /= 2f;
		GetComponent<AudioSource>().Play();
	}
}
