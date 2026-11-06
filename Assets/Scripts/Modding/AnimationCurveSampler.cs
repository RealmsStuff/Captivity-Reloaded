using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	// Plays a normalized clip straight from its authored curves. Each runtime track is evaluated at the exact
	// playback time on the same linear or Hermite segment the baked frames are sampled from, so the clip keeps
	// every key and the curve between keys whatever its key count and the display's frame rate. Sprite keys
	// switch at their own times. The baked frames stay on the clip for everything else that reads them.
	// One sampler is shared by every actor playing the clip; each actor samples through its own Cursor.
	public sealed class AnimationCurveSampler
	{
		private enum Channel { X, Y, Rotation, ScaleX, ScaleY, ColorR, ColorG, ColorB, ColorA, SortingOrder }

		private sealed class Track
		{
			public int Bone;
			public Channel Channel;
			public float[] Times, Values, InTangents, OutTangents;
			public bool[] Curved; // segment ending at the key: Hermite when both of its tangents are set
		}

		private sealed class SpriteTrack
		{
			public int Bone;
			public float[] Times;
			public string[] Names;
			public Sprite[] Sprites;
		}

		// Where an actor is in the clip: the segment each track sampled last, which normal forward playback
		// reuses or steps once from, and the poses it hands out.
		public sealed class Cursor
		{
			internal readonly AnimationCurveSampler Owner;
			internal readonly int[] Segments;
			internal readonly int[] SpriteKeys;
			internal readonly EnemyBonePoseDefinition[] Poses;
			internal readonly Dictionary<string, EnemyBonePoseDefinition> Output = new Dictionary<string, EnemyBonePoseDefinition>(StringComparer.Ordinal);
			internal readonly bool[] Present;

			internal Cursor(AnimationCurveSampler i_owner)
			{
				Owner = i_owner;
				Segments = new int[i_owner.m_tracks.Length];
				SpriteKeys = new int[i_owner.m_sprites.Length];
				Poses = new EnemyBonePoseDefinition[i_owner.m_bones.Length];
				Present = new bool[i_owner.m_bones.Length];
				for (int index = 0; index < Poses.Length; index++) Poses[index] = new EnemyBonePoseDefinition();
			}
		}

		private readonly string[] m_bones;
		private readonly Track[] m_tracks;
		private readonly SpriteTrack[] m_sprites;

		public float DurationSeconds { get; }
		public int TrackCount { get { return m_tracks.Length; } }

		// i_tracks are the clip's runtime numeric tracks (bone/ transform and sprite/ color and sorting tracks);
		// i_includeSpriteBone filters the sprite/ object tracks the clip plays, as its frame bake does.
		public AnimationCurveSampler(IEnumerable<NormalizedNumericTrack> i_tracks, IEnumerable<NormalizedObjectTrack> i_objectTracks,
			float i_durationSeconds, Func<string, bool> i_includeSpriteBone = null, IReadOnlyDictionary<string, Sprite> i_runtimeSprites = null)
		{
			DurationSeconds = i_durationSeconds;
			List<string> bones = new List<string>();
			Dictionary<string, int> boneIndex = new Dictionary<string, int>(StringComparer.Ordinal);
			int IndexOf(string i_bone)
			{
				if (!boneIndex.TryGetValue(i_bone, out int index)) { index = bones.Count; bones.Add(i_bone); boneIndex.Add(i_bone, index); }
				return index;
			}
			List<Track> tracks = new List<Track>();
			foreach (NormalizedNumericTrack source in i_tracks ?? new NormalizedNumericTrack[0])
			{
				if (source?.Keys == null || source.Keys.Count == 0 || !TryGetChannel(source.Property, out Channel channel)) continue;
				int count = source.Keys.Count;
				Track track = new Track
				{
					Bone = IndexOf(source.Target.Substring(source.Target.IndexOf('/') + 1)), Channel = channel,
					Times = new float[count], Values = new float[count], InTangents = new float[count], OutTangents = new float[count], Curved = new bool[count]
				};
				for (int index = 0; index < count; index++)
				{
					NormalizedNumericKey key = source.Keys[index];
					track.Times[index] = key.Time;
					track.Values[index] = key.Value;
					track.InTangents[index] = key.InTangent ?? 0f;
					track.OutTangents[index] = key.OutTangent ?? 0f;
					track.Curved[index] = index > 0 && source.Keys[index - 1].OutTangent.HasValue && key.InTangent.HasValue;
				}
				tracks.Add(track);
			}
			List<SpriteTrack> sprites = new List<SpriteTrack>();
			foreach (NormalizedObjectTrack source in i_objectTracks ?? new NormalizedObjectTrack[0])
			{
				if (source?.Target == null || source.Property != "sprite" || !source.Target.StartsWith("sprite/", StringComparison.Ordinal)) continue;
				string bone = source.Target.Substring("sprite/".Length);
				if (i_includeSpriteBone != null && !i_includeSpriteBone(bone)) continue;
				List<NormalizedObjectKey> keys = source.Keys ?? new List<NormalizedObjectKey>();
				if (keys.Count == 0) continue;
				SpriteTrack track = new SpriteTrack
				{
					Bone = IndexOf(bone), Times = new float[keys.Count], Names = new string[keys.Count], Sprites = new Sprite[keys.Count]
				};
				for (int index = 0; index < keys.Count; index++)
				{
					track.Times[index] = keys[index].Time;
					track.Names[index] = keys[index].Name;
					if (!string.IsNullOrEmpty(keys[index].Asset) && i_runtimeSprites != null && i_runtimeSprites.TryGetValue(keys[index].Asset, out Sprite sprite))
						track.Sprites[index] = sprite;
				}
				sprites.Add(track);
			}
			m_bones = bones.ToArray();
			m_tracks = tracks.ToArray();
			m_sprites = sprites.ToArray();
		}

		public Cursor CreateCursor() { return new Cursor(this); }

		// Every bone|region pair the clip's sprite keys can show, for building the actor's sprite set.
		public IEnumerable<KeyValuePair<string, string>> GetRegions()
		{
			foreach (SpriteTrack track in m_sprites)
				foreach (string name in track.Names)
					if (!string.IsNullOrEmpty(name)) yield return new KeyValuePair<string, string>(m_bones[track.Bone], name);
		}

		// The clip's pose at i_time (seconds into the clip, already wrapped or clamped by the caller): one pose per
		// bone, holding only the properties the clip animates. The dictionary and its poses belong to the cursor
		// and are reused by its next Sample.
		public IReadOnlyDictionary<string, EnemyBonePoseDefinition> Sample(float i_time, Cursor io_cursor)
		{
			if (io_cursor == null || io_cursor.Owner != this) throw new ArgumentException("The cursor belongs to another clip.", nameof(io_cursor));
			for (int index = 0; index < io_cursor.Poses.Length; index++)
			{
				EnemyBonePoseDefinition pose = io_cursor.Poses[index];
				pose.X = pose.Y = pose.Rotation = pose.ScaleX = pose.ScaleY = pose.ColorR = pose.ColorG = pose.ColorB = pose.ColorA = null;
				pose.RuntimeSortingOrder = null; pose.Region = null; pose.RuntimeSpriteName = null; pose.RuntimeSprite = null;
				io_cursor.Present[index] = false;
			}
			for (int index = 0; index < m_tracks.Length; index++)
			{
				Track track = m_tracks[index];
				float value = Evaluate(track, i_time, ref io_cursor.Segments[index]);
				EnemyBonePoseDefinition pose = io_cursor.Poses[track.Bone];
				io_cursor.Present[track.Bone] = true;
				switch (track.Channel)
				{
				case Channel.X: pose.X = value; break;
				case Channel.Y: pose.Y = value; break;
				case Channel.Rotation: pose.Rotation = value; break;
				case Channel.ScaleX: pose.ScaleX = value; break;
				case Channel.ScaleY: pose.ScaleY = value; break;
				case Channel.ColorR: pose.ColorR = value; break;
				case Channel.ColorG: pose.ColorG = value; break;
				case Channel.ColorB: pose.ColorB = value; break;
				case Channel.ColorA: pose.ColorA = value; break;
				case Channel.SortingOrder: pose.RuntimeSortingOrder = (int)Math.Round(value); break;
				}
			}
			for (int index = 0; index < m_sprites.Length; index++)
			{
				SpriteTrack track = m_sprites[index];
				int key = Select(track.Times, i_time, ref io_cursor.SpriteKeys[index]);
				if (key < 0) continue; // before the track's first key: the sprite is left as it is
				EnemyBonePoseDefinition pose = io_cursor.Poses[track.Bone];
				io_cursor.Present[track.Bone] = true;
				pose.Region = track.Names[key];
				pose.RuntimeSpriteName = track.Names[key];
				pose.RuntimeSprite = track.Sprites[key];
			}
			io_cursor.Output.Clear();
			for (int index = 0; index < m_bones.Length; index++)
				if (io_cursor.Present[index]) io_cursor.Output.Add(m_bones[index], io_cursor.Poses[index]);
			return io_cursor.Output;
		}

		// Same segments and formula as the frame bake's evaluator. A time on a key belongs to the segment
		// ending there, so two keys at one time (a deliberate step) switch values just after it.
		private static float Evaluate(Track i_track, float i_time, ref int io_segment)
		{
			float[] times = i_track.Times;
			int last = times.Length - 1;
			if (last == 0 || i_time <= times[0]) return i_track.Values[0];
			if (i_time >= times[last]) return i_track.Values[last];
			int upper = io_segment;
			if (!Contains(times, upper, i_time))
			{
				// Playback mostly moves forward a little: try the next segment before searching.
				upper++;
				if (!Contains(times, upper, i_time)) upper = LowerBound(times, i_time);
				io_segment = upper;
			}
			float leftValue = i_track.Values[upper - 1], rightValue = i_track.Values[upper];
			float duration = times[upper] - times[upper - 1], t = duration <= 0f ? 1f : (i_time - times[upper - 1]) / duration;
			if (!i_track.Curved[upper]) return leftValue + (rightValue - leftValue) * t;
			float t2 = t * t, t3 = t2 * t;
			return (2f * t3 - 3f * t2 + 1f) * leftValue + (t3 - 2f * t2 + t) * duration * i_track.OutTangents[upper - 1]
				+ (-2f * t3 + 3f * t2) * rightValue + (t3 - t2) * duration * i_track.InTangents[upper];
		}

		private static bool Contains(float[] i_times, int i_upper, float i_time)
		{
			return i_upper >= 1 && i_upper < i_times.Length && i_times[i_upper - 1] < i_time && i_time <= i_times[i_upper];
		}

		// The first key at or after i_time, for a time strictly inside the track.
		private static int LowerBound(float[] i_times, float i_time)
		{
			int low = 1, high = i_times.Length - 1;
			while (low < high)
			{
				int middle = (low + high) >> 1;
				if (i_times[middle] < i_time) low = middle + 1; else high = middle;
			}
			return low;
		}

		// The last sprite key at or before i_time, or -1.
		private static int Select(float[] i_times, float i_time, ref int io_key)
		{
			int key = io_key;
			bool Holds(int i_key) => i_key >= 0 && i_key < i_times.Length && i_times[i_key] <= i_time && (i_key + 1 == i_times.Length || i_times[i_key + 1] > i_time);
			if (Holds(key)) return key;
			if (Holds(key + 1)) return io_key = key + 1;
			int low = 0, high = i_times.Length; // first key after i_time
			while (low < high)
			{
				int middle = (low + high) >> 1;
				if (i_times[middle] <= i_time) low = middle + 1; else high = middle;
			}
			io_key = Mathf.Max(0, low - 1);
			return low - 1;
		}

		private static bool TryGetChannel(string i_property, out Channel o_channel)
		{
			switch (i_property)
			{
			case "position.x": o_channel = Channel.X; return true;
			case "position.y": o_channel = Channel.Y; return true;
			case "rotation.z": o_channel = Channel.Rotation; return true;
			case "scale.x": o_channel = Channel.ScaleX; return true;
			case "scale.y": o_channel = Channel.ScaleY; return true;
			case "color.r": o_channel = Channel.ColorR; return true;
			case "color.g": o_channel = Channel.ColorG; return true;
			case "color.b": o_channel = Channel.ColorB; return true;
			case "color.a": o_channel = Channel.ColorA; return true;
			case "sortingOrder": o_channel = Channel.SortingOrder; return true;
			default: o_channel = Channel.X; return false;
			}
		}
	}
}
