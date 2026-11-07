using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	// The numbers behind a downedFinisher phase's reactions (EnemyFinisherReactionsDefinition), matching Raper:
	// Thrust/CumThrust's camera shakes, PlayerGainPleasure/PlayerGainCumPleasure, and the chance rolls of its sound
	// layers. ModularEnemyFinisher applies them.
	public static class FinisherReactions
	{
		// Raper.Thrust and Raper.CumThrust shake the camera this hard for ShakeSeconds, by power 0..3.
		private static readonly float[] ThrustShakes = { 0.15f, 0.25f, 0.6f, 0.75f };
		private static readonly float[] CumThrustShakes = { 0.3f, 0.5f, 0.75f, 1f };
		public const float ShakeSeconds = 0.4f;

		public struct Gains
		{
			// Before the rule profile's rape libido multiplier (ExternalRuleProfileFactory.ApplyRapeLibidoGain).
			public float Libido;
			public float Pleasure;
			public float Strength;
		}

		// A cue's amount as a Core thrust power: rounded and clamped to 0..3, 0 when missing.
		public static int Power(float? i_amount)
		{
			if (!i_amount.HasValue || float.IsNaN(i_amount.Value)) return 0;
			int power = (int)System.Math.Round(i_amount.Value);
			return power < 0 ? 0 : power > 3 ? 3 : power;
		}

		public static float Shake(int i_power, bool i_cum)
		{
			int power = i_power < 0 ? 0 : i_power > 3 ? 3 : i_power;
			return i_cum ? CumThrustShakes[power] : ThrustShakes[power];
		}

		// PlayerGainPleasure (i_cum null) or PlayerGainCumPleasure (i_cum the cum thrust's additional powers).
		public static Gains GetGains(EnemyFinisherThrustDefinition i_thrust, EnemyFinisherThrustDefinition i_cum)
		{
			float libido = (i_thrust?.Libido ?? 0f) + (i_cum?.Libido ?? 0f);
			float pleasure = (i_thrust?.Pleasure ?? 0f) + (i_cum?.Pleasure ?? 0f);
			return new Gains { Libido = libido * 10f, Pleasure = pleasure * 2f, Strength = (libido + pleasure) * 5f };
		}

		public static float StruggleDamage(EnemyFinisherThrustDefinition i_thrust, EnemyFinisherThrustDefinition i_cum)
		{
			return (i_thrust?.StruggleDamage ?? 0f) + (i_cum?.StruggleDamage ?? 0f);
		}

		// ManagerAudio.PlayAudioSFXRandom: plays when the roll (0..1) is within the chance (a set without one always
		// plays), then picks one of the sounds with the second roll (0..1).
		public static bool TryPick(IList<string> i_sounds, float? i_chance, float i_roll, float i_pickRoll, out string o_sound)
		{
			o_sound = null;
			if (i_sounds == null || i_sounds.Count == 0 || i_roll > (i_chance ?? 1f)) return false;
			int index = (int)(i_pickRoll * i_sounds.Count);
			o_sound = i_sounds[index < 0 ? 0 : index >= i_sounds.Count ? i_sounds.Count - 1 : index];
			return !string.IsNullOrEmpty(o_sound);
		}

		// Every sound file a definition's finisher reactions can play, for preloading.
		public static IEnumerable<string> GetSoundPaths(EnemyBehaviorModuleDefinition i_module)
		{
			foreach (EnemyFinisherPhaseDefinition phase in i_module?.Phases ?? new List<EnemyFinisherPhaseDefinition>())
			{
				EnemyFinisherReactionsDefinition reactions = phase?.Reactions;
				if (reactions == null) continue;
				foreach (string path in Sounds(reactions.Thrust?.Sounds)) yield return path;
				foreach (string path in Sounds(reactions.CumThrust?.Sounds)) yield return path;
				foreach (string path in Sounds(reactions.PlayerVoice?.Sounds)) yield return path;
				foreach (string path in Sounds(reactions.EnemyVoice?.Sounds)) yield return path;
				foreach (EnemyFinisherSoundSetDefinition layer in reactions.Layers ?? new List<EnemyFinisherSoundSetDefinition>())
					foreach (string path in Sounds(layer?.Sounds)) yield return path;
				foreach (string path in Sounds(reactions.UniqueSounds)) yield return path;
			}
		}

		private static IEnumerable<string> Sounds(List<string> i_sounds)
		{
			foreach (string sound in i_sounds ?? new List<string>())
				if (!string.IsNullOrEmpty(sound)) yield return sound;
		}
	}
}
