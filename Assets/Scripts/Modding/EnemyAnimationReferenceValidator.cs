using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CaptivityReloaded.Modding
{
	public static class EnemyAnimationReferenceValidator
	{
		public static ValidationReport Validate(NormalizedEnemyAnimationDocument i_document,
			IEnumerable<string> i_bones, IEnumerable<string> i_regions, string i_packRoot, string i_source = null)
		{
			ValidationReport report = new ValidationReport();
			if (i_document == null)
			{
				report.Add(ValidationSeverity.Error, "enemy-animation.reference-document", "No enemy animation document was supplied.", i_source);
				return report;
			}

			HashSet<string> bones = i_bones == null ? null : new HashSet<string>(i_bones.Where(i_value => !string.IsNullOrWhiteSpace(i_value)), StringComparer.Ordinal);
			HashSet<string> regions = i_regions == null ? null : new HashSet<string>(i_regions.Where(i_value => !string.IsNullOrWhiteSpace(i_value)), StringComparer.Ordinal);
			foreach (NormalizedNumericTrack track in i_document.Tracks ?? new List<NormalizedNumericTrack>())
				ValidateTarget(track?.Target, bones, "enemy-animation.reference-bone", "Track references a missing rig bone: ", report, i_source);

			foreach (NormalizedObjectTrack track in i_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
			{
				ValidateTarget(track?.Target, bones, "enemy-animation.reference-bone", "Sprite track references a missing rig bone: ", report, i_source);
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>())
				{
					if (key == null) continue;
					if (string.IsNullOrWhiteSpace(key.Asset))
					{
						if (regions != null && !regions.Contains(key.Name ?? string.Empty))
							report.Add(ValidationSeverity.Error, "enemy-animation.reference-region", "Sprite key references a missing atlas region: " + (key.Name ?? "<empty>"), i_source);
					}
					else ValidateFile(key.Asset, ".png", i_packRoot, "enemy-animation.reference-sprite-file", "Sprite key asset", report, i_source);
				}
			}

			foreach (EnemyAnimationEventDefinition animationEvent in i_document.Events ?? new List<EnemyAnimationEventDefinition>())
			{
				if (animationEvent == null) continue;
				if (animationEvent.Type == "sound") ValidateFile(animationEvent.File, null, i_packRoot, "enemy-animation.reference-audio-file", "Sound event asset", report, i_source);
				if (animationEvent.Type != "spriteEffect") continue;
				if (!string.IsNullOrWhiteSpace(animationEvent.Bone) && bones != null && !bones.Contains(animationEvent.Bone))
					report.Add(ValidationSeverity.Error, "enemy-animation.reference-bone", "Sprite effect references a missing rig bone: " + animationEvent.Bone, i_source);
				if (regions != null && !regions.Contains(animationEvent.Region ?? string.Empty))
					report.Add(ValidationSeverity.Error, "enemy-animation.reference-region", "Sprite effect references a missing atlas region: " + (animationEvent.Region ?? "<empty>"), i_source);
			}

			HashSet<string> effectIds = new HashSet<string>((i_document.Effects ?? new List<NormalizedEffectDefinition>())
				.Where(i_effect => i_effect != null && !string.IsNullOrWhiteSpace(i_effect.Id)).Select(i_effect => i_effect.Id), StringComparer.Ordinal);
			foreach (NormalizedEffectDefinition effect in i_document.Effects ?? new List<NormalizedEffectDefinition>())
			{
				if (effect == null) continue;
				if (effect.Target != null && effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal))
					ValidateTarget(effect.Target, bones, "enemy-animation.reference-bone", "Effect '" + effect.Id + "' references a missing rig bone: ", report, i_source);
				if (!string.IsNullOrWhiteSpace(effect.Texture)) ValidateFile(effect.Texture, ".png", i_packRoot, "enemy-animation.reference-effect-texture", "Effect '" + effect.Id + "' texture", report, i_source);
			}
			foreach (NormalizedEffectTrigger trigger in i_document.EffectTriggers ?? new List<NormalizedEffectTrigger>())
				foreach (string effectId in trigger?.Effects ?? new List<string>())
					if (!effectIds.Contains(effectId)) report.Add(ValidationSeverity.Error, "enemy-animation.reference-effect", "Effect trigger references an unknown effect: " + effectId, i_source);
			return report;
		}

		public static int RepairSafeReferences(NormalizedEnemyAnimationDocument io_document,
			IEnumerable<string> i_bones, IEnumerable<string> i_regions)
		{
			if (io_document == null) return 0;
			HashSet<string> bones = new HashSet<string>(i_bones ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
			HashSet<string> regions = new HashSet<string>(i_regions ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
			string fallbackBone = bones.OrderBy(i_value => i_value, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
			string fallbackRegion = regions.OrderBy(i_value => i_value, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
			int changes = 0;
			foreach (NormalizedNumericTrack track in io_document.Tracks ?? new List<NormalizedNumericTrack>()) changes += RepairTarget(track, fallbackBone, bones);
			foreach (NormalizedObjectTrack track in io_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
			{
				changes += RepairTarget(track, fallbackBone, bones);
				if (fallbackRegion == null) continue;
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>())
					if (key != null && string.IsNullOrWhiteSpace(key.Asset) && !regions.Contains(key.Name ?? string.Empty)) { key.Name = fallbackRegion; changes++; }
			}
			foreach (EnemyAnimationEventDefinition animationEvent in io_document.Events ?? new List<EnemyAnimationEventDefinition>())
			{
				if (animationEvent == null || animationEvent.Type != "spriteEffect") continue;
				if (!string.IsNullOrWhiteSpace(animationEvent.Bone) && fallbackBone != null && !bones.Contains(animationEvent.Bone)) { animationEvent.Bone = fallbackBone; changes++; }
				if (fallbackRegion != null && !regions.Contains(animationEvent.Region ?? string.Empty)) { animationEvent.Region = fallbackRegion; changes++; }
			}
			foreach (NormalizedEffectDefinition effect in io_document.Effects ?? new List<NormalizedEffectDefinition>()) changes += RepairTarget(effect, fallbackBone, bones);
			HashSet<string> ids = new HashSet<string>((io_document.Effects ?? new List<NormalizedEffectDefinition>()).Where(i_effect => i_effect != null).Select(i_effect => i_effect.Id), StringComparer.Ordinal);
			foreach (NormalizedEffectTrigger trigger in io_document.EffectTriggers ?? new List<NormalizedEffectTrigger>())
				if (trigger?.Effects != null) changes += trigger.Effects.RemoveAll(i_id => !ids.Contains(i_id));
			changes += (io_document.EffectTriggers ?? new List<NormalizedEffectTrigger>()).RemoveAll(i_trigger => i_trigger == null || i_trigger.Effects == null || i_trigger.Effects.Count == 0);
			return changes;
		}

		private static void ValidateTarget(string i_target, HashSet<string> i_bones, string i_code, string i_message, ValidationReport io_report, string i_source)
		{
			if (i_bones == null || string.IsNullOrWhiteSpace(i_target)) return;
			int slash = i_target.IndexOf('/'); string bone = slash < 0 ? i_target : i_target.Substring(slash + 1);
			if (!i_bones.Contains(bone)) io_report.Add(ValidationSeverity.Error, i_code, i_message + (string.IsNullOrEmpty(bone) ? "<empty>" : bone), i_source);
		}

		private static void ValidateFile(string i_relativePath, string i_requiredExtension, string i_packRoot,
			string i_code, string i_label, ValidationReport io_report, string i_source)
		{
			if (string.IsNullOrWhiteSpace(i_packRoot) || string.IsNullOrWhiteSpace(i_relativePath) || !ModPath.IsSafeRelativePath(i_relativePath)) return;
			if (i_requiredExtension != null && !i_relativePath.EndsWith(i_requiredExtension, StringComparison.OrdinalIgnoreCase)) return;
			string root = Path.GetFullPath(i_packRoot);
			string candidate = Path.GetFullPath(Path.Combine(root, i_relativePath.Replace('/', Path.DirectorySeparatorChar)));
			if (!candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(candidate))
				io_report.Add(ValidationSeverity.Error, i_code, i_label + " is missing: " + i_relativePath, i_source);
		}

		private static int RepairTarget(NormalizedNumericTrack io_track, string i_fallback, HashSet<string> i_bones)
		{
			if (io_track == null || i_fallback == null || TargetExists(io_track.Target, i_bones)) return 0;
			io_track.Target = TargetPrefix(io_track.Target) + i_fallback; return 1;
		}

		private static int RepairTarget(NormalizedObjectTrack io_track, string i_fallback, HashSet<string> i_bones)
		{
			if (io_track == null || i_fallback == null || TargetExists(io_track.Target, i_bones)) return 0;
			io_track.Target = "sprite/" + i_fallback; return 1;
		}

		private static int RepairTarget(NormalizedEffectDefinition io_effect, string i_fallback, HashSet<string> i_bones)
		{
			if (io_effect == null || i_fallback == null || io_effect.Target == null || !io_effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal) || TargetExists(io_effect.Target, i_bones)) return 0;
			io_effect.Target = "enemy-bone/" + i_fallback; return 1;
		}

		private static bool TargetExists(string i_target, HashSet<string> i_bones)
		{
			if (string.IsNullOrWhiteSpace(i_target)) return false; int slash = i_target.IndexOf('/');
			return i_bones.Contains(slash < 0 ? i_target : i_target.Substring(slash + 1));
		}

		private static string TargetPrefix(string i_target)
		{
			return i_target != null && i_target.StartsWith("sprite/", StringComparison.Ordinal) ? "sprite/" : "bone/";
		}
	}
}
