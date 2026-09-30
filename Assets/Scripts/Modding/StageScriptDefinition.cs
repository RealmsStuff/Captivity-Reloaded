using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageScriptDefinitionDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("stage", Required = Required.Always)] public string Stage { get; set; }
		[JsonProperty("variables")] public Dictionary<string, float> Variables { get; set; } = new Dictionary<string, float>();
		[JsonProperty("machines")] public List<StageScriptMachineDocument> Machines { get; set; } = new List<StageScriptMachineDocument>();
		[JsonProperty("sequences", Required = Required.Always)] public List<StageScriptSequenceDocument> Sequences { get; set; } = new List<StageScriptSequenceDocument>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageScriptMachineDocument
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("objectId", Required = Required.Always)] public string ObjectId { get; set; }
		[JsonProperty("initialState", Required = Required.Always)] public string InitialState { get; set; }
		[JsonProperty("states", Required = Required.Always)] public List<StageScriptMachineStateDocument> States { get; set; } = new List<StageScriptMachineStateDocument>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageScriptMachineStateDocument
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("actions", Required = Required.Always)] public List<StageScriptActionDocument> Actions { get; set; } = new List<StageScriptActionDocument>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageScriptSequenceDocument
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("trigger", Required = Required.Always)] public string Trigger { get; set; }
		[JsonProperty("signal")] public string Signal { get; set; }
		[JsonProperty("objectId")] public string ObjectId { get; set; }
		[JsonProperty("state")] public string State { get; set; }
		[JsonProperty("enemy")] public string Enemy { get; set; }
		[JsonProperty("value")] public float Value { get; set; }
		[JsonProperty("seconds")] public float Seconds { get; set; }
		[JsonProperty("once")] public bool Once { get; set; }
		[JsonProperty("conditions")] public List<StageScriptConditionDocument> Conditions { get; set; } = new List<StageScriptConditionDocument>();
		[JsonProperty("actions", Required = Required.Always)] public List<StageScriptActionDocument> Actions { get; set; } = new List<StageScriptActionDocument>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageScriptConditionDocument
	{
		[JsonProperty("source")] public string Source { get; set; }
		[JsonProperty("variable")] public string Variable { get; set; }
		[JsonProperty("objectId")] public string ObjectId { get; set; }
		[JsonProperty("enemy")] public string Enemy { get; set; }
		[JsonProperty("operator", Required = Required.Always)] public string Operator { get; set; }
		[JsonProperty("value", Required = Required.Always)] public float Value { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageScriptActionDocument
	{
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("seconds")] public float Seconds { get; set; }
		[JsonProperty("message")] public string Message { get; set; }
		[JsonProperty("variable")] public string Variable { get; set; }
		[JsonProperty("value")] public float Value { get; set; }
		[JsonProperty("signal")] public string Signal { get; set; }
		[JsonProperty("objectId")] public string ObjectId { get; set; }
		[JsonProperty("destinationId")] public string DestinationId { get; set; }
		[JsonProperty("active")] public bool Active { get; set; }
		[JsonProperty("state")] public string State { get; set; }
		[JsonProperty("amount")] public int Amount { get; set; }
		[JsonProperty("animation")] public string Animation { get; set; }
		[JsonProperty("controller")] public string Controller { get; set; }
		[JsonProperty("sequence")] public string Sequence { get; set; }
		[JsonProperty("status")] public string Status { get; set; }
		[JsonProperty("durationSeconds")] public float DurationSeconds { get; set; }
		[JsonProperty("ticksPerSecond")] public float TicksPerSecond { get; set; }
		[JsonProperty("chance")] public float Chance { get; set; }
		[JsonProperty("chanceIncrease")] public float ChanceIncrease { get; set; }
		[JsonProperty("maxActive")] public int MaxActive { get; set; }
		[JsonProperty("x")] public float X { get; set; }
		[JsonProperty("y")] public float Y { get; set; }
		[JsonProperty("conditions")] public List<StageScriptConditionDocument> Conditions { get; set; } = new List<StageScriptConditionDocument>();
		[JsonProperty("actions")] public List<StageScriptActionDocument> Actions { get; set; } = new List<StageScriptActionDocument>();
	}

	public sealed class StageScriptDefinition
	{
		public ContentId Id { get; }
		public ContentId Stage { get; }
		public string PackId { get; }
		public string Source { get; }
		public IReadOnlyDictionary<string, float> Variables { get; }
		public IReadOnlyList<StageScriptMachineDocument> Machines { get; }
		public IReadOnlyList<StageScriptSequenceDocument> Sequences { get; }

		public StageScriptDefinition(ContentId i_id, ContentId i_stage, string i_packId, string i_source,
			Dictionary<string, float> i_variables, List<StageScriptSequenceDocument> i_sequences,
			List<StageScriptMachineDocument> i_machines = null)
		{
			Id = i_id; Stage = i_stage; PackId = i_packId; Source = i_source ?? string.Empty;
			Variables = i_variables; Sequences = i_sequences; Machines = i_machines ?? new List<StageScriptMachineDocument>();
		}
	}

	public sealed class StageScriptLoadResult
	{
		public StageScriptDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class StageScriptParser
	{
		private static readonly HashSet<string> Triggers = new HashSet<string>(StringComparer.Ordinal) { "stage-open", "wave-start", "wave-end", "signal", "interaction", "player-enter", "player-exit", "manual", "timer", "door-state", "object-state", "enemy-count", "machine-state" };
		private static readonly HashSet<string> ConditionSources = new HashSet<string>(StringComparer.Ordinal) { "variable", "wave", "enemy-count", "door-open", "object-active" };
		private static readonly HashSet<string> Operators = new HashSet<string>(StringComparer.Ordinal) { "==", "!=", "<", "<=", ">", ">=" };
		private static readonly HashSet<string> Actions = new HashSet<string>(StringComparer.Ordinal) { "wait", "wait-for-door", "wait-for-enemy-count", "wait-for-signal", "wait-for-sequence", "wait-until", "wait-until-player-distant", "repeat", "notify", "set-variable", "add-variable", "send-signal", "run-sequence", "start-sequence", "cancel-sequence", "set-machine-state", "set-object-active", "spawn-actor", "move-object", "move-object-to", "rotate-object", "set-door", "set-light", "set-spawner-enabled", "set-interaction-enabled", "activate-interaction", "queue-spawns", "teleport-player", "set-player-input", "set-hud-visible", "set-player-facing", "remove-player-clothing", "play-player-animation", "kill-player", "apply-player-status", "camera-shake", "camera-zoom", "set-global-light", "play-audio", "stop-audio", "play-particles", "play-animation" };

		public static StageScriptLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			StageScriptLoadResult result = new StageScriptLoadResult();
			StageScriptDefinitionDocument document;
			try { document = JsonConvert.DeserializeObject<StageScriptDefinitionDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Stage script resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "stageScript") Error(result, "type", "Definition type must be 'stageScript'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("stage-script/", StringComparison.Ordinal))
				Error(result, "id", "ID must use the defining pack namespace and a stage-script/ path.", i_source);
			if (!ContentId.TryParse(document.Stage, out ContentId stage) || !stage.Path.StartsWith("stage/", StringComparison.Ordinal))
				Error(result, "stage", "stage must be a namespaced stage content ID.", i_source);
			if (document.Variables == null || document.Variables.Count > 64) Error(result, "variables", "variables must contain at most 64 entries.", i_source);
			else foreach (KeyValuePair<string, float> variable in document.Variables)
				if (!ValidName(variable.Key) || !Finite(variable.Value)) Error(result, "variable", "Variable names must be simple identifiers and values must be finite.", i_source);
			Dictionary<string, HashSet<string>> machines = ValidateMachines(document.Machines, result, i_source);
			if (document.Sequences == null || document.Sequences.Count == 0 || document.Sequences.Count > 64)
				Error(result, "sequences", "sequences must contain 1 to 64 entries.", i_source);
			else
			{
				HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
				foreach (StageScriptSequenceDocument sequence in document.Sequences)
				{
					if (sequence == null || !ValidName(sequence.Id) || !ids.Add(sequence.Id)) { Error(result, "sequence-id", "Sequence IDs must be unique simple identifiers.", i_source); continue; }
					if (!Triggers.Contains(sequence.Trigger)) Error(result, "trigger", "Unsupported trigger: " + sequence.Trigger, i_source);
					if ((sequence.Trigger == "signal" || sequence.Trigger == "interaction") && !ValidName(sequence.Signal)) Error(result, "signal", "Signal and interaction triggers require a simple target name.", i_source);
					if ((sequence.Trigger == "player-enter" || sequence.Trigger == "player-exit") && !ValidName(sequence.ObjectId)) Error(result, "volume-trigger", "player-enter and player-exit triggers require a script-trigger objectId.", i_source);
					if (sequence.Trigger == "timer" && (!Finite(sequence.Seconds) || sequence.Seconds < 0.1f || sequence.Seconds > 3600f)) Error(result, "timer-trigger", "timer triggers require seconds from 0.1 to 3600.", i_source);
					if (sequence.Trigger == "door-state" && (!ValidName(sequence.ObjectId) || (sequence.State != "open" && sequence.State != "closed"))) Error(result, "door-trigger", "door-state triggers require an objectId and open/closed state.", i_source);
					if (sequence.Trigger == "object-state" && (!ValidName(sequence.ObjectId) || (sequence.State != "active" && sequence.State != "inactive"))) Error(result, "object-trigger", "object-state triggers require an objectId and active/inactive state.", i_source);
					if (sequence.Trigger == "enemy-count" && ((sequence.State != "at-most" && sequence.State != "at-least") || !Finite(sequence.Value) || sequence.Value < 0f || sequence.Value > 10000f || !ValidEnemyIdOrEmpty(sequence.Enemy))) Error(result, "enemy-count-trigger", "enemy-count triggers require at-most/at-least, value 0 to 10000, and an optional enemy content ID.", i_source);
					if (sequence.Trigger == "machine-state" && (!ValidName(sequence.ObjectId) || !ValidName(sequence.State))) Error(result, "machine-trigger", "machine-state triggers require a machine objectId and state.", i_source);
					if (sequence.Conditions == null || sequence.Conditions.Count > 16) Error(result, "conditions", "A sequence may contain at most 16 conditions.", i_source);
					else foreach (StageScriptConditionDocument condition in sequence.Conditions)
						ValidateCondition(condition, result, i_source);
					if (sequence.Actions == null || sequence.Actions.Count == 0 || sequence.Actions.Count > 128) Error(result, "actions", "A sequence must contain 1 to 128 actions.", i_source);
					else
					{
						foreach (StageScriptActionDocument action in sequence.Actions) ValidateAction(action, result, i_source, 0);
						if (ExpandedActionCount(sequence.Actions, 4096) > 4096)
							Error(result, "action-budget", "A sequence may execute at most 4096 expanded actions.", i_source);
					}
				}
				ValidateSequenceReferences(document.Sequences, ids, result, i_source);
				ValidateMachineReferences(document.Sequences, document.Machines, machines, result, i_source);
			}
			if (result.Report.IsValid) result.Definition = new StageScriptDefinition(id, stage, i_packId, i_source, document.Variables, document.Sequences, document.Machines);
			return result;
		}

		private static void ValidateCondition(StageScriptConditionDocument i_condition, StageScriptLoadResult io_result, string i_source)
		{
			if (i_condition == null || !Operators.Contains(i_condition.Operator) || !Finite(i_condition.Value)) { Error(io_result, "condition", "Invalid stage-script condition.", i_source); return; }
			string source = string.IsNullOrEmpty(i_condition.Source) ? "variable" : i_condition.Source;
			if (!ConditionSources.Contains(source)) { Error(io_result, "condition-source", "Unsupported condition source: " + source, i_source); return; }
			if (source == "variable" && !ValidName(i_condition.Variable)) Error(io_result, "condition-variable", "Variable conditions require a simple variable name.", i_source);
			if ((source == "door-open" || source == "object-active") && !ValidName(i_condition.ObjectId)) Error(io_result, "condition-object", source + " conditions require an objectId.", i_source);
			if (source == "enemy-count" && !ValidEnemyIdOrEmpty(i_condition.Enemy)) Error(io_result, "condition-enemy", "enemy-count conditions require an optional enemy content ID.", i_source);
		}

		private static void ValidateAction(StageScriptActionDocument i_action, StageScriptLoadResult io_result, string i_source, int i_depth)
		{
			if (i_action == null || !Actions.Contains(i_action.Type)) { Error(io_result, "action", "Unsupported stage-script action.", i_source); return; }
			if (i_depth > 4) { Error(io_result, "action-depth", "Nested stage-script actions may be at most four levels deep.", i_source); return; }
			if (i_action.Type == "wait" && (!Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f)) Error(io_result, "wait", "wait seconds must be between 0 and 300.", i_source);
			if (i_action.Type == "wait-for-door" && (!ValidName(i_action.ObjectId) || (i_action.State != "open" && i_action.State != "closed") || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "wait-for-door", "wait-for-door requires a door objectId, open/closed state, and optional 0 to 300 second timeout.", i_source);
			if (i_action.Type == "wait-for-enemy-count" && (i_action.State != "at-most" && i_action.State != "at-least" || !Finite(i_action.Value) || i_action.Value < 0f || i_action.Value > 10000f || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "wait-for-enemy-count", "wait-for-enemy-count requires at-most/at-least state, value 0 to 10000, and optional 0 to 300 second timeout.", i_source);
			if (i_action.Type == "wait-for-signal" && (!ValidName(i_action.Signal) || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "wait-for-signal", "wait-for-signal requires a simple signal name and optional 0 to 300 second timeout.", i_source);
			if (i_action.Type == "wait-for-sequence" && (!ValidName(i_action.Sequence) || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "wait-for-sequence", "wait-for-sequence requires a local sequence name and optional 0 to 300 second timeout.", i_source);
			if (i_action.Type == "wait-until-player-distant" && (!ValidName(i_action.ObjectId) || (!string.IsNullOrEmpty(i_action.DestinationId) && !ValidName(i_action.DestinationId)) || !Finite(i_action.Value) || i_action.Value < 0f || i_action.Value > 1000f || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "wait-player-distance", "wait-until-player-distant requires an objectId, optional destinationId, distance value from 0 to 1000, and optional 0 to 300 second timeout.", i_source);
			if (i_action.Type == "wait-until")
			{
				if (i_action.Conditions == null || i_action.Conditions.Count == 0 || i_action.Conditions.Count > 16
					|| !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f)
					Error(io_result, "wait-until", "wait-until requires 1 to 16 conditions and an optional 0 to 300 second timeout.", i_source);
				else foreach (StageScriptConditionDocument condition in i_action.Conditions) ValidateCondition(condition, io_result, i_source);
			}
			else if (i_action.Conditions != null && i_action.Conditions.Count > 0)
				Error(io_result, "action-conditions", "Only wait-until actions may contain nested conditions.", i_source);
			if (i_action.Type == "repeat")
			{
				if (i_action.Amount < 1 || i_action.Amount > 100 || i_action.Actions == null || i_action.Actions.Count == 0 || i_action.Actions.Count > 32)
					Error(io_result, "repeat", "repeat requires amount from 1 to 100 and 1 to 32 nested actions.", i_source);
				else foreach (StageScriptActionDocument nested in i_action.Actions) ValidateAction(nested, io_result, i_source, i_depth + 1);
			}
			else if (i_action.Actions != null && i_action.Actions.Count > 0)
				Error(io_result, "nested-actions", "Only repeat actions may contain nested actions.", i_source);
			if (i_action.Type == "notify" && (string.IsNullOrWhiteSpace(i_action.Message) || i_action.Message.Length > 200)) Error(io_result, "notify", "notify message must contain 1 to 200 characters.", i_source);
			if ((i_action.Type == "set-variable" || i_action.Type == "add-variable") && (!ValidName(i_action.Variable) || !Finite(i_action.Value))) Error(io_result, "variable-action", "Variable actions require a valid variable and finite value.", i_source);
			if (i_action.Type == "send-signal" && !ValidName(i_action.Signal)) Error(io_result, "send-signal", "send-signal requires a simple signal name.", i_source);
			if ((i_action.Type == "run-sequence" || i_action.Type == "start-sequence" || i_action.Type == "cancel-sequence") && !ValidName(i_action.Sequence))
				Error(io_result, "sequence-action", i_action.Type + " requires a simple local sequence name.", i_source);
			if (i_action.Type == "set-machine-state" && (!ValidName(i_action.ObjectId) || !ValidName(i_action.State)))
				Error(io_result, "machine-action", "set-machine-state requires a local machine objectId and state.", i_source);
			if (i_action.Type == "set-object-active" && !ValidName(i_action.ObjectId)) Error(io_result, "object", "set-object-active requires a simple Tiled object ID/name.", i_source);
			if (i_action.Type == "spawn-actor" && !ValidName(i_action.ObjectId)) Error(io_result, "actor-target", "spawn-actor requires a simple actor object ID/name.", i_source);
			if (i_action.Type == "move-object" && (!ValidName(i_action.ObjectId) || !Finite(i_action.X) || !Finite(i_action.Y)
				|| i_action.X < -100000f || i_action.X > 100000f || i_action.Y < -100000f || i_action.Y > 100000f
				|| !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "move-object", "move-object requires a target, bounded stage-local x/y, and 0 to 300 seconds.", i_source);
			if (i_action.Type == "move-object-to" && (!ValidName(i_action.ObjectId) || !ValidName(i_action.DestinationId) || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "move-object-to", "move-object-to requires objectId, destinationId, and 0 to 300 seconds.", i_source);
			if (i_action.Type == "rotate-object" && (!ValidName(i_action.ObjectId) || !Finite(i_action.Value)
				|| i_action.Value < -36000f || i_action.Value > 36000f || !Finite(i_action.Seconds)
				|| i_action.Seconds < 0f || i_action.Seconds > 300f))
				Error(io_result, "rotate-object", "rotate-object requires a target, angle from -36000 to 36000, and 0 to 300 seconds.", i_source);
			if ((i_action.Type == "set-door" || i_action.Type == "set-light" || i_action.Type == "activate-interaction" || i_action.Type == "queue-spawns") && !ValidName(i_action.ObjectId)) Error(io_result, "target", i_action.Type + " requires a simple target object ID/name.", i_source);
			if (i_action.Type == "set-spawner-enabled" && !ValidName(i_action.ObjectId)) Error(io_result, "spawner-target", "set-spawner-enabled requires a simple spawner ID/name.", i_source);
			if (i_action.Type == "set-interaction-enabled" && !ValidName(i_action.ObjectId)) Error(io_result, "interaction-target", "set-interaction-enabled requires a simple interaction ID/name.", i_source);
			if (i_action.Type == "set-door" && i_action.State != "open" && i_action.State != "closed" && i_action.State != "toggle") Error(io_result, "door-state", "set-door state must be open, closed, or toggle.", i_source);
			if (i_action.Type == "set-light" && i_action.State != "on" && i_action.State != "off" && i_action.State != "toggle") Error(io_result, "light-state", "set-light state must be on, off, or toggle.", i_source);
			if (i_action.Type == "queue-spawns" && (i_action.Amount < 1 || i_action.Amount > 100)) Error(io_result, "spawn-amount", "queue-spawns amount must be between 1 and 100.", i_source);
			if ((i_action.Type == "teleport-player" || i_action.Type == "play-particles" || i_action.Type == "play-animation") && !ValidName(i_action.ObjectId)) Error(io_result, "scene-target", i_action.Type + " requires a simple target object ID/name.", i_source);
			if (i_action.Type == "set-player-facing" && i_action.State != "left" && i_action.State != "right") Error(io_result, "player-facing", "set-player-facing state must be left or right.", i_source);
			if (i_action.Type == "play-player-animation" && (!ValidName(i_action.Animation)
				|| (i_action.Controller != "current" && i_action.Controller != "finisher")))
				Error(io_result, "player-animation", "play-player-animation requires a simple animation name and controller current or finisher.", i_source);
			if (i_action.Type == "apply-player-status" && (i_action.Status != "jacky-curse"
				|| !Finite(i_action.DurationSeconds) || i_action.DurationSeconds < 0.1f || i_action.DurationSeconds > 100000f
				|| !Finite(i_action.TicksPerSecond) || i_action.TicksPerSecond < 0.01f || i_action.TicksPerSecond > 60f
				|| !Finite(i_action.Chance) || i_action.Chance < 0f || i_action.Chance > 1f
				|| !Finite(i_action.ChanceIncrease) || i_action.ChanceIncrease < 0f || i_action.ChanceIncrease > 1f
				|| i_action.MaxActive < 1 || i_action.MaxActive > 32))
				Error(io_result, "player-status", "apply-player-status currently supports jacky-curse with durationSeconds 0.1 to 100000, ticksPerSecond 0.01 to 60, chance/chanceIncrease 0 to 1, and maxActive 1 to 32.", i_source);
			if ((i_action.Type == "play-audio" || i_action.Type == "stop-audio") && !ValidName(i_action.ObjectId)) Error(io_result, "audio-target", i_action.Type + " requires a simple authored audio object ID/name.", i_source);
			if (i_action.Type == "camera-shake" && (!Finite(i_action.Value) || i_action.Value < 0f || i_action.Value > 1f || !Finite(i_action.Seconds) || i_action.Seconds < 0.01f || i_action.Seconds > 1f)) Error(io_result, "camera-shake", "camera-shake value and seconds must each be between 0 and 1.", i_source);
			if (i_action.Type == "camera-zoom" && (!Finite(i_action.Value) || i_action.Value < 1f || i_action.Value > 179f || !Finite(i_action.Seconds) || i_action.Seconds < 0f || i_action.Seconds > 10f)) Error(io_result, "camera-zoom", "camera-zoom value must be 1 to 179 and seconds 0 to 10.", i_source);
			if (i_action.Type == "set-global-light" && (!Finite(i_action.Value) || i_action.Value < 0f || i_action.Value > 10f)) Error(io_result, "global-light", "set-global-light value must be between 0 and 10.", i_source);
			if (i_action.Type == "play-animation" && !ValidName(i_action.Animation)) Error(io_result, "animation", "play-animation requires a simple animation state name.", i_source);
		}

		private static void ValidateSequenceReferences(IEnumerable<StageScriptSequenceDocument> i_sequences, HashSet<string> i_ids,
			StageScriptLoadResult io_result, string i_source)
		{
			Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			foreach (StageScriptSequenceDocument sequence in i_sequences)
			{
				if (sequence == null || !ValidName(sequence.Id)) continue;
				List<string> edges = new List<string>();
				graph[sequence.Id] = edges;
				foreach (StageScriptActionDocument action in EnumerateActions(sequence.Actions))
				{
					if (action == null || (action.Type != "run-sequence" && action.Type != "start-sequence" && action.Type != "cancel-sequence" && action.Type != "wait-for-sequence")) continue;
					if (ValidName(action.Sequence) && !i_ids.Contains(action.Sequence))
						Error(io_result, "sequence-reference", "Sequence '" + sequence.Id + "' refers to missing local sequence '" + action.Sequence + "'.", i_source);
					if ((action.Type == "run-sequence" || action.Type == "start-sequence" || action.Type == "wait-for-sequence") && ValidName(action.Sequence) && i_ids.Contains(action.Sequence))
						edges.Add(action.Sequence);
				}
			}
			Dictionary<string, int> states = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (string id in graph.Keys)
				if (HasSequenceCycle(id, graph, states)) { Error(io_result, "sequence-cycle", "run-sequence/start-sequence/wait-for-sequence references may not form a cycle.", i_source); break; }
		}

		private static IEnumerable<StageScriptActionDocument> EnumerateActions(IEnumerable<StageScriptActionDocument> i_actions)
		{
			foreach (StageScriptActionDocument action in i_actions ?? new List<StageScriptActionDocument>())
			{
				yield return action;
				if (action == null || action.Type != "repeat") continue;
				foreach (StageScriptActionDocument nested in EnumerateActions(action.Actions)) yield return nested;
			}
		}

		private static Dictionary<string, HashSet<string>> ValidateMachines(List<StageScriptMachineDocument> i_machines,
			StageScriptLoadResult io_result, string i_source)
		{
			Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
			if (i_machines == null || i_machines.Count > 32) { Error(io_result, "machines", "machines may contain at most 32 entries.", i_source); return result; }
			foreach (StageScriptMachineDocument machine in i_machines)
			{
				if (machine == null || !ValidName(machine.Id) || !ValidName(machine.ObjectId) || result.ContainsKey(machine.Id)) { Error(io_result, "machine", "Machine IDs must be unique, and machine IDs/objectIds must be simple identifiers.", i_source); continue; }
				HashSet<string> states = new HashSet<string>(StringComparer.Ordinal); result.Add(machine.Id, states);
				if (machine.States == null || machine.States.Count == 0 || machine.States.Count > 16) { Error(io_result, "machine-states", "A machine requires 1 to 16 states.", i_source); continue; }
				foreach (StageScriptMachineStateDocument state in machine.States)
				{
					if (state == null || !ValidName(state.Id) || !states.Add(state.Id)) { Error(io_result, "machine-state", "Machine state IDs must be unique simple identifiers.", i_source); continue; }
					if (state.Actions == null || state.Actions.Count == 0 || state.Actions.Count > 32) Error(io_result, "machine-state-actions", "A machine state requires 1 to 32 actions.", i_source);
					else { foreach (StageScriptActionDocument action in state.Actions) ValidateAction(action, io_result, i_source, 0); if (ExpandedActionCount(state.Actions, 4096) > 4096) Error(io_result, "action-budget", "A machine state may execute at most 4096 expanded actions.", i_source); }
				}
				if (!states.Contains(machine.InitialState)) Error(io_result, "machine-initial-state", "Machine initialState must name one of its states.", i_source);
			}
			return result;
		}

		private static void ValidateMachineReferences(IEnumerable<StageScriptSequenceDocument> i_sequences,
			IEnumerable<StageScriptMachineDocument> i_machineDocuments, Dictionary<string, HashSet<string>> i_machines,
			StageScriptLoadResult io_result, string i_source)
		{
			List<StageScriptActionDocument> actions = new List<StageScriptActionDocument>();
			foreach (StageScriptSequenceDocument sequence in i_sequences ?? new List<StageScriptSequenceDocument>())
				if (sequence != null) actions.AddRange(EnumerateActions(sequence.Actions));
			foreach (StageScriptMachineDocument machine in i_machineDocuments ?? new List<StageScriptMachineDocument>())
				if (machine != null)
					foreach (StageScriptMachineStateDocument state in machine.States ?? new List<StageScriptMachineStateDocument>())
						if (state != null) actions.AddRange(EnumerateActions(state.Actions));
			foreach (StageScriptActionDocument action in actions)
				if (action != null && action.Type == "set-machine-state" && (!i_machines.TryGetValue(action.ObjectId, out HashSet<string> states) || !states.Contains(action.State)))
					Error(io_result, "machine-reference", "set-machine-state refers to a missing machine or state: " + action.ObjectId + "/" + action.State, i_source);
			foreach (StageScriptSequenceDocument sequence in i_sequences ?? new List<StageScriptSequenceDocument>())
				if (sequence != null && sequence.Trigger == "machine-state" && (!i_machines.TryGetValue(sequence.ObjectId, out HashSet<string> states) || !states.Contains(sequence.State)))
					Error(io_result, "machine-reference", "machine-state trigger refers to a missing machine or state: " + sequence.ObjectId + "/" + sequence.State, i_source);
		}

		private static int ExpandedActionCount(IEnumerable<StageScriptActionDocument> i_actions, int i_limit)
		{
			long total = 0;
			foreach (StageScriptActionDocument action in i_actions ?? new List<StageScriptActionDocument>())
			{
				total++;
				if (action != null && action.Type == "repeat")
					total += (long)Math.Max(0, action.Amount) * ExpandedActionCount(action.Actions, i_limit);
				if (total > i_limit) return i_limit + 1;
			}
			return (int)total;
		}

		private static bool HasSequenceCycle(string i_id, Dictionary<string, List<string>> i_graph, Dictionary<string, int> io_states)
		{
			io_states.TryGetValue(i_id, out int state);
			if (state == 1) return true;
			if (state == 2) return false;
			io_states[i_id] = 1;
			if (i_graph.TryGetValue(i_id, out List<string> edges))
				foreach (string edge in edges) if (HasSequenceCycle(edge, i_graph, io_states)) return true;
			io_states[i_id] = 2;
			return false;
		}

		private static bool ValidName(string i_value)
		{
			if (string.IsNullOrEmpty(i_value) || i_value.Length > 64 || !(char.IsLetter(i_value[0]) || i_value[0] == '_')) return false;
			for (int i = 1; i < i_value.Length; i++) if (!(char.IsLetterOrDigit(i_value[i]) || i_value[i] == '_' || i_value[i] == '-')) return false;
			return true;
		}
		private static bool Finite(float i_value) => !float.IsNaN(i_value) && !float.IsInfinity(i_value);
		private static bool ValidEnemyIdOrEmpty(string i_value)
		{
			if (string.IsNullOrEmpty(i_value)) return true;
			return ContentId.TryParse(i_value, out ContentId id) && id.Path.StartsWith("enemy/", StringComparison.Ordinal);
		}
		private static void Error(StageScriptLoadResult io_result, string i_code, string i_message, string i_source) => io_result.Report.Add(ValidationSeverity.Error, "stage-script." + i_code, i_message, i_source);
	}
}
