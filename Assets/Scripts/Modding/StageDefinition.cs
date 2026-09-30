using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StagePointDefinition
	{
		[JsonProperty("x", Required = Required.Always)]
		public float X { get; set; }

		[JsonProperty("y", Required = Required.Always)]
		public float Y { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageLayoutDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("playerSpawn")]
		public StagePointDefinition PlayerSpawn { get; set; }

		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public int PixelsPerUnit { get; set; }

		[JsonProperty("hideInheritedVisuals")]
		public bool HideInheritedVisuals { get; set; }

		[JsonProperty("preserveInheritedStageObjects")]
		public bool PreserveInheritedStageObjects { get; set; }

		[JsonProperty("reuseInheritedSpawners")]
		public bool ReuseInheritedSpawners { get; set; }

		[JsonIgnore]
		public TiledLevelDefinition TiledLevel { get; internal set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageWaveDefinition
	{
		[JsonProperty("firstWaveEnemyCount", Required = Required.Always)]
		public int FirstWaveEnemyCount { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageAudioDefinition
	{
		[JsonProperty("ambience")] public string Ambience { get; set; }
		[JsonProperty("entryMusic")] public string EntryMusic { get; set; }
		[JsonProperty("waveMusic")] public string WaveMusic { get; set; }
		[JsonProperty("waveComplete")] public string WaveComplete { get; set; }

		public bool HasAny => !string.IsNullOrWhiteSpace(Ambience) || !string.IsNullOrWhiteSpace(EntryMusic)
			|| !string.IsNullOrWhiteSpace(WaveMusic) || !string.IsNullOrWhiteSpace(WaveComplete);
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageCameraBoundsDefinition
	{
		[JsonProperty("minX", Required = Required.Always)] public float MinX { get; set; }
		[JsonProperty("minY", Required = Required.Always)] public float MinY { get; set; }
		[JsonProperty("maxX", Required = Required.Always)] public float MaxX { get; set; }
		[JsonProperty("maxY", Required = Required.Always)] public float MaxY { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageCameraDefinition
	{
		[JsonProperty("bounds")] public StageCameraBoundsDefinition Bounds { get; set; }
		[JsonProperty("unbounded")] public bool Unbounded { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageTestToolsDefinition
	{
		[JsonProperty("enabled", Required = Required.Always)] public bool Enabled { get; set; }
		[JsonProperty("enemy", Required = Required.Always)] public string Enemy { get; set; }
		[JsonProperty("spawnPosition")] public StagePointDefinition SpawnPosition { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageSpawnerDocument
	{
		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("position", Required = Required.Always)]
		public StagePointDefinition Position { get; set; }

		[JsonProperty("enemies", Required = Required.Always)]
		public List<string> Enemies { get; set; }

		[JsonProperty("selectionWeight", Required = Required.Always)]
		public float SelectionWeight { get; set; }

		[JsonProperty("minimumWave")]
		public int MinimumWave { get; set; }

		[JsonProperty("delaySeconds", Required = Required.Always)]
		public float DelaySeconds { get; set; }

		[JsonProperty("delayJitterSeconds")]
		public float DelayJitterSeconds { get; set; }

		[JsonProperty("initialDelaySeconds")]
		public float InitialDelaySeconds { get; set; }

		[JsonProperty("initialDelayJitterSeconds")]
		public float InitialDelayJitterSeconds { get; set; }

		[JsonProperty("spawnOutOfSight")]
		public bool SpawnOutOfSight { get; set; }

		[JsonProperty("enabled")]
		public bool? Enabled { get; set; }

		[JsonProperty("requiredOpenDoors")]
		public List<string> RequiredOpenDoors { get; set; } = new List<string>();

		[JsonProperty("requiredClosedDoors")]
		public List<string> RequiredClosedDoors { get; set; } = new List<string>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageDefinitionDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("displayName", Required = Required.Always)]
		public string DisplayName { get; set; }

		[JsonProperty("extends", Required = Required.Always)]
		public string Extends { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("layout", Required = Required.Always)]
		public StageLayoutDefinition Layout { get; set; }

		[JsonProperty("waves", Required = Required.Always)]
		public StageWaveDefinition Waves { get; set; }

		[JsonProperty("audio")]
		public StageAudioDefinition Audio { get; set; }

		[JsonProperty("camera")]
		public StageCameraDefinition Camera { get; set; }

		[JsonProperty("testTools")]
		public StageTestToolsDefinition TestTools { get; set; }

		[JsonProperty("spawners", Required = Required.Always)]
		public List<StageSpawnerDocument> Spawners { get; set; }
	}

	public sealed class StageSpawnerDefinition
	{
		public string Id { get; }
		public StagePointDefinition Position { get; }
		public IReadOnlyList<ContentId> Enemies { get; }
		public float SelectionWeight { get; }
		public int MinimumWave { get; }
		public float DelaySeconds { get; }
		public float DelayJitterSeconds { get; }
		public float InitialDelaySeconds { get; }
		public float InitialDelayJitterSeconds { get; }
		public bool SpawnOutOfSight { get; }
		public bool Enabled { get; }
		public IReadOnlyList<string> RequiredOpenDoors { get; }
		public IReadOnlyList<string> RequiredClosedDoors { get; }

		internal StageSpawnerDefinition(StageSpawnerDocument i_document, IReadOnlyList<ContentId> i_enemies)
		{
			Id = i_document.Id;
			Position = i_document.Position;
			Enemies = i_enemies;
			SelectionWeight = i_document.SelectionWeight;
			MinimumWave = i_document.MinimumWave;
			DelaySeconds = i_document.DelaySeconds;
			DelayJitterSeconds = i_document.DelayJitterSeconds;
			InitialDelaySeconds = i_document.InitialDelaySeconds;
			InitialDelayJitterSeconds = i_document.InitialDelayJitterSeconds;
			SpawnOutOfSight = i_document.SpawnOutOfSight;
			Enabled = i_document.Enabled ?? true;
			RequiredOpenDoors = i_document.RequiredOpenDoors ?? new List<string>();
			RequiredClosedDoors = i_document.RequiredClosedDoors ?? new List<string>();
		}
	}

	public sealed class StageDefinition
	{
		public const string RuntimeTemplatePath = "stage/mod-template";

		public ContentId Id { get; }
		public ContentId Extends { get; }
		public bool UsesRuntimeTemplate => Extends.Namespace == "core" && Extends.Path == RuntimeTemplatePath;
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public StageLayoutDefinition Layout { get; }
		public StageWaveDefinition Waves { get; }
		public StageAudioDefinition Audio { get; }
		public StageCameraDefinition Camera { get; }
		public StageTestToolsDefinition TestTools { get; }
		public string AssetRoot { get; internal set; }
		public IReadOnlyList<StageSpawnerDefinition> Spawners { get; }

		internal StageDefinition(ContentId i_id, ContentId i_extends, string i_packId, string i_source,
			StageDefinitionDocument i_document, IReadOnlyList<StageSpawnerDefinition> i_spawners)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Layout = i_document.Layout;
			Waves = i_document.Waves;
			Audio = i_document.Audio;
			Camera = i_document.Camera;
			TestTools = i_document.TestTools;
			Spawners = i_spawners;
		}
	}

	public sealed class StageDefinitionLoadResult
	{
		public StageDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class StageDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;

		public static StageDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			StageDefinitionLoadResult result = new StageDefinitionLoadResult();
			StageDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<StageDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "stage.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "stage.null", "Stage definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "stage", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'stage'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("stage/", StringComparison.Ordinal))
				Error(result, "id", "Stage ID must use the defining pack namespace and a stage/ path.", i_source);
			if (!ContentId.TryParse(document.Extends, out ContentId extends) || extends.Namespace != "core" || !extends.Path.StartsWith("stage/", StringComparison.Ordinal) || extends.Path == "stage/hub")
				Error(result, "extends", "V1 extends must reference a non-hub Core stage content ID.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);

			ValidateLayout(document.Layout, result.Report, i_source);
			bool usesRuntimeTemplate = extends.Namespace == "core" && extends.Path == StageDefinition.RuntimeTemplatePath;
			if (usesRuntimeTemplate && (document.Layout == null || document.Layout.Type != "tiledJson"))
				Error(result, "runtime-template-layout", "core:stage/mod-template requires a tiledJson layout.", i_source);
			if (usesRuntimeTemplate && document.Layout != null
				&& (document.Layout.PreserveInheritedStageObjects || document.Layout.ReuseInheritedSpawners))
				Error(result, "runtime-template-inheritance",
					"The stage-independent runtime template cannot preserve inherited objects or spawners.", i_source);
			ValidateWaves(document.Waves, result.Report, i_source);
			ValidateAudio(document.Audio, result.Report, i_source);
			ValidateCamera(document.Camera, result.Report, i_source);
			ValidateTestTools(document.TestTools, result.Report, i_source);
			List<StageSpawnerDefinition> spawners = ValidateSpawners(document.Spawners, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new StageDefinition(id, extends, i_packId, i_source, document, spawners);
			return result;
		}

		private static void ValidateTestTools(StageTestToolsDefinition i_tools, ValidationReport io_report, string i_source)
		{
			if (i_tools == null) return;
			if (!i_tools.Enabled) io_report.Add(ValidationSeverity.Error, "stage.test-tools.enabled", "testTools must be omitted unless enabled is true.", i_source);
			if (!ContentId.TryParse(i_tools.Enemy, out ContentId enemy) || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "stage.test-tools.enemy", "testTools.enemy must be an enemy content ID.", i_source);
			if (i_tools.SpawnPosition != null) ValidatePoint(i_tools.SpawnPosition, "stage.test-tools.spawn-position", io_report, i_source);
		}

		private static void ValidateCamera(StageCameraDefinition i_camera, ValidationReport io_report, string i_source)
		{
			if (i_camera == null) return;
			StageCameraBoundsDefinition bounds = i_camera.Bounds;
			if (i_camera.Unbounded && bounds == null) return;
			if (bounds == null || !IsFiniteBounded(bounds.MinX) || !IsFiniteBounded(bounds.MinY)
				|| !IsFiniteBounded(bounds.MaxX) || !IsFiniteBounded(bounds.MaxY)
				|| bounds.MaxX <= bounds.MinX || bounds.MaxY <= bounds.MinY || i_camera.Unbounded)
				io_report.Add(ValidationSeverity.Error, "stage.camera.bounds", "Camera bounds require finite min/max coordinates with max greater than min.", i_source);
		}

		private static void ValidateAudio(StageAudioDefinition i_audio, ValidationReport io_report, string i_source)
		{
			if (i_audio == null) return;
			if (!i_audio.HasAny)
			{
				io_report.Add(ValidationSeverity.Error, "stage.audio.empty", "audio must replace at least one stage audio slot.", i_source);
				return;
			}
			ValidateAudioPath(i_audio.Ambience, "ambience", io_report, i_source);
			ValidateAudioPath(i_audio.EntryMusic, "entry-music", io_report, i_source);
			ValidateAudioPath(i_audio.WaveMusic, "wave-music", io_report, i_source);
			ValidateAudioPath(i_audio.WaveComplete, "wave-complete", io_report, i_source);
		}

		private static void ValidateAudioPath(string i_path, string i_slot, ValidationReport io_report, string i_source)
		{
			if (string.IsNullOrWhiteSpace(i_path)) return;
			string extension = System.IO.Path.GetExtension(i_path).ToLowerInvariant();
			if (!ModPath.IsSafeRelativePath(i_path) || (extension != ".wav" && extension != ".ogg"))
				io_report.Add(ValidationSeverity.Error, "stage.audio." + i_slot, i_slot + " must be a safe pack-relative WAV or OGG path.", i_source);
		}

		private static void ValidateLayout(StageLayoutDefinition i_layout, ValidationReport io_report, string i_source)
		{
			if (i_layout == null)
			{
				io_report.Add(ValidationSeverity.Error, "stage.layout", "layout is required.", i_source);
				return;
			}
			if (string.Equals(i_layout.Type, "coreStageLayout", StringComparison.Ordinal))
			{
				ValidatePoint(i_layout.PlayerSpawn, "stage.layout.player-spawn", io_report, i_source);
				if (i_layout.HideInheritedVisuals)
					io_report.Add(ValidationSeverity.Error, "stage.layout.hide-inherited-visuals", "hideInheritedVisuals is only available for tiledJson layouts.", i_source);
				if (i_layout.PreserveInheritedStageObjects)
					io_report.Add(ValidationSeverity.Error, "stage.layout.preserve-inherited-stage-objects", "preserveInheritedStageObjects is only available for tiledJson layouts.", i_source);
				if (i_layout.ReuseInheritedSpawners)
					io_report.Add(ValidationSeverity.Error, "stage.layout.reuse-inherited-spawners", "reuseInheritedSpawners is only available for tiledJson compatibility ports.", i_source);
			}
			else if (string.Equals(i_layout.Type, "tiledJson", StringComparison.Ordinal))
			{
				if (!ModPath.IsSafeRelativePath(i_layout.Path) || !i_layout.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
					io_report.Add(ValidationSeverity.Error, "stage.layout.path", "Tiled layout path must be a safe pack-relative .json file.", i_source);
				if (i_layout.PixelsPerUnit < 1 || i_layout.PixelsPerUnit > 512)
					io_report.Add(ValidationSeverity.Error, "stage.layout.pixels-per-unit", "pixelsPerUnit must be between 1 and 512.", i_source);
				if (i_layout.PlayerSpawn != null)
					io_report.Add(ValidationSeverity.Error, "stage.layout.player-spawn", "tiledJson layouts define player-spawn in the Tiled map.", i_source);
				if (i_layout.ReuseInheritedSpawners && !i_layout.PreserveInheritedStageObjects)
					io_report.Add(ValidationSeverity.Error, "stage.layout.reuse-inherited-spawners", "reuseInheritedSpawners requires preserveInheritedStageObjects.", i_source);
			}
			else io_report.Add(ValidationSeverity.Error, "stage.layout.type", "layout.type must be 'coreStageLayout' or 'tiledJson'.", i_source);
		}

		private static void ValidateWaves(StageWaveDefinition i_waves, ValidationReport io_report, string i_source)
		{
			if (i_waves == null || i_waves.FirstWaveEnemyCount < 1 || i_waves.FirstWaveEnemyCount > 1000)
				io_report.Add(ValidationSeverity.Error, "stage.waves.first-wave-enemy-count", "firstWaveEnemyCount must be between 1 and 1000.", i_source);
		}

		private static List<StageSpawnerDefinition> ValidateSpawners(List<StageSpawnerDocument> i_spawners, ValidationReport io_report, string i_source)
		{
			List<StageSpawnerDefinition> definitions = new List<StageSpawnerDefinition>();
			if (i_spawners == null || i_spawners.Count == 0 || i_spawners.Count > 256)
			{
				io_report.Add(ValidationSeverity.Error, "stage.spawners", "A stage requires between 1 and 256 spawners.", i_source);
				return definitions;
			}
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (StageSpawnerDocument spawner in i_spawners)
			{
				if (spawner == null)
				{
					io_report.Add(ValidationSeverity.Error, "stage.spawner.null", "Spawner entries cannot be null.", i_source);
					continue;
				}
				if (!ContentId.TryParse("local:" + spawner.Id, out _) || spawner.Id.Contains("/") || !ids.Add(spawner.Id))
					io_report.Add(ValidationSeverity.Error, "stage.spawner.id", "Spawner IDs must be unique lowercase path segments: " + spawner.Id, i_source);
				ValidatePoint(spawner.Position, "stage.spawner.position", io_report, i_source);

				List<ContentId> enemies = new List<ContentId>();
				if (spawner.Enemies == null || spawner.Enemies.Count == 0 || spawner.Enemies.Count > 64)
					io_report.Add(ValidationSeverity.Error, "stage.spawner.enemies", "Each spawner requires between 1 and 64 enemy IDs.", i_source);
				else
				{
					HashSet<ContentId> uniqueEnemies = new HashSet<ContentId>();
					foreach (string enemy in spawner.Enemies)
					{
						if (!ContentId.TryParse(enemy, out ContentId enemyId) || !enemyId.Path.StartsWith("enemy/", StringComparison.Ordinal))
							io_report.Add(ValidationSeverity.Error, "stage.spawner.enemy-id", "Spawner enemy references must use enemy/ content IDs: " + enemy, i_source);
						else if (!uniqueEnemies.Add(enemyId))
							io_report.Add(ValidationSeverity.Error, "stage.spawner.duplicate-enemy", "Spawner contains a duplicate enemy ID: " + enemyId, i_source);
						else enemies.Add(enemyId);
					}
				}

				ValidateRange(spawner.SelectionWeight, 0.001f, 1000f, "selection-weight", io_report, i_source);
				if (spawner.MinimumWave < 0 || spawner.MinimumWave > 10000)
					io_report.Add(ValidationSeverity.Error, "stage.spawner.minimum-wave", "minimumWave must be between 0 and 10000.", i_source);
				ValidateRange(spawner.DelaySeconds, 0.02f, 300f, "delay-seconds", io_report, i_source);
				ValidateRange(spawner.DelayJitterSeconds, 0f, 300f, "delay-jitter-seconds", io_report, i_source);
				ValidateRange(spawner.InitialDelaySeconds, 0f, 300f, "initial-delay-seconds", io_report, i_source);
				ValidateRange(spawner.InitialDelayJitterSeconds, 0f, 300f, "initial-delay-jitter-seconds", io_report, i_source);
				if (spawner.DelayJitterSeconds > spawner.DelaySeconds)
					io_report.Add(ValidationSeverity.Error, "stage.spawner.delay-jitter", "delayJitterSeconds cannot exceed delaySeconds.", i_source);
				if (spawner.InitialDelayJitterSeconds > spawner.InitialDelaySeconds)
					io_report.Add(ValidationSeverity.Error, "stage.spawner.initial-delay-jitter", "initialDelayJitterSeconds cannot exceed initialDelaySeconds.", i_source);
				ValidateDoorRequirements(spawner.RequiredOpenDoors, "requiredOpenDoors", io_report, i_source);
				ValidateDoorRequirements(spawner.RequiredClosedDoors, "requiredClosedDoors", io_report, i_source);
				if (spawner.RequiredOpenDoors != null && spawner.RequiredClosedDoors != null)
				{
					HashSet<string> open = new HashSet<string>(spawner.RequiredOpenDoors, StringComparer.Ordinal);
					foreach (string door in spawner.RequiredClosedDoors)
						if (open.Contains(door)) io_report.Add(ValidationSeverity.Error, "stage.spawner.door-conflict",
							"Spawner cannot require the same door to be both open and closed: " + door, i_source);
				}
				definitions.Add(new StageSpawnerDefinition(spawner, enemies));
			}
			return definitions;
		}

		private static void ValidateDoorRequirements(List<string> i_doors, string i_property,
			ValidationReport io_report, string i_source)
		{
			if (i_doors == null || i_doors.Count > 32)
			{
				io_report.Add(ValidationSeverity.Error, "stage.spawner.door-count", i_property + " must contain at most 32 door names.", i_source);
				return;
			}
			HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
			foreach (string door in i_doors)
				if (!ContentId.TryParse("local:" + door, out _) || door.Contains("/") || !unique.Add(door))
					io_report.Add(ValidationSeverity.Error, "stage.spawner.door-id", i_property + " entries must be unique lowercase path segments: " + door, i_source);
		}

		private static void ValidatePoint(StagePointDefinition i_point, string i_code, ValidationReport io_report, string i_source)
		{
			if (i_point == null || !IsFiniteBounded(i_point.X) || !IsFiniteBounded(i_point.Y))
				io_report.Add(ValidationSeverity.Error, i_code, "Coordinates must be finite and between -100000 and 100000.", i_source);
		}

		private static bool IsFiniteBounded(float i_value)
		{
			return !float.IsNaN(i_value) && !float.IsInfinity(i_value) && i_value >= -100000f && i_value <= 100000f;
		}

		private static void ValidateRange(float i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (float.IsNaN(i_value) || float.IsInfinity(i_value) || i_value < i_min || i_value > i_max)
				io_report.Add(ValidationSeverity.Error, "stage.spawner." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void Error(StageDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "stage." + i_code, i_message, i_source);
		}
	}
}
