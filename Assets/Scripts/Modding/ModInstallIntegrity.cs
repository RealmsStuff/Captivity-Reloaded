using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	public enum ModIntegrityState { Unchanged, Untracked, Modified, Unreadable }

	public sealed class ModIntegrityCheck
	{
		public ModIntegrityState State { get; internal set; }
		public string Version { get; internal set; }
		public string Fingerprint { get; internal set; }
		public string Message { get; internal set; }
		public bool NeedsConfirmation => State != ModIntegrityState.Unchanged;
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class ModInstallLedger
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; } = 1;
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("records", Required = Required.Always)] public List<ModInstallRecord> Records { get; set; } = new List<ModInstallRecord>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class ModInstallRecord
	{
		[JsonProperty("version", Required = Required.Always)] public string Version { get; set; }
		[JsonProperty("archiveSha256", Required = Required.Always)] public string ArchiveSha256 { get; set; }
		[JsonProperty("fingerprint", Required = Required.Always)] public string Fingerprint { get; set; }
	}

	/// <summary>Tracks catalog-installed file trees without writing metadata into the pack itself.</summary>
	public static class ModInstallIntegrity
	{
		private const int MaximumFiles = 4096;
		private const long MaximumBytes = 1024L * 1024L * 1024L;
		private const int MaximumRecords = 32;

		public static ModIntegrityCheck Check(string i_modsDirectory, string i_id)
		{
			ModIntegrityCheck result = new ModIntegrityCheck { State = ModIntegrityState.Unreadable,
				Message = "Installed files could not be verified." };
			if (!TryPackAndStatePaths(i_modsDirectory, i_id, out string pack, out string state)) return result;
			result.Version = ModInstallRecovery.ReadVersion(pack, i_id);
			if (result.Version == null)
			{ result.Message = "Installed manifest is missing or invalid."; return result; }
			if (!TryFingerprint(pack, out string fingerprint, out string error))
			{ result.Message = error; return result; }
			result.Fingerprint = fingerprint;
			ModInstallLedger ledger = ReadLedger(state, i_id);
			if (ledger == null)
			{
				result.State = ModIntegrityState.Untracked;
				result.Message = "This installation predates file tracking or was installed manually.";
				return result;
			}
			foreach (ModInstallRecord record in ledger.Records)
				if (record.Version == result.Version && string.Equals(record.Fingerprint, fingerprint, StringComparison.OrdinalIgnoreCase))
				{
					result.State = ModIntegrityState.Unchanged;
					result.Message = "Installed files match the catalog release.";
					return result;
				}
			result.State = ModIntegrityState.Modified;
			result.Message = "Installed files differ from every recorded catalog copy of v" + result.Version + ".";
			return result;
		}

		public static ValidationReport Record(string i_modsDirectory, string i_id, string i_version, string i_archiveSha256)
		{
			ValidationReport report = new ValidationReport();
			if (!SemanticVersion.TryParse(i_version, out _) || !Sha256Text(i_archiveSha256)
				|| !TryPackAndStatePaths(i_modsDirectory, i_id, out string pack, out string state))
			{ Warning(report, "integrity.record", "Could not record catalog-install provenance.", i_id); return report; }
			if (!TryFingerprint(pack, out string fingerprint, out string error))
			{ Warning(report, "integrity.fingerprint", error, pack); return report; }
			try
			{
				ModInstallLedger ledger = ReadLedger(state, i_id) ?? new ModInstallLedger { Id = i_id };
				ledger.Records.RemoveAll(record => record.Version == i_version
					&& string.Equals(record.Fingerprint, fingerprint, StringComparison.OrdinalIgnoreCase));
				ledger.Records.Add(new ModInstallRecord { Version = i_version,
					ArchiveSha256 = i_archiveSha256.ToLowerInvariant(), Fingerprint = fingerprint });
				while (ledger.Records.Count > MaximumRecords) ledger.Records.RemoveAt(0);
				Directory.CreateDirectory(Path.GetDirectoryName(state));
				string temporary = state + ".tmp-" + Guid.NewGuid().ToString("N");
				File.WriteAllText(temporary, JsonConvert.SerializeObject(ledger, Formatting.Indented), new UTF8Encoding(false));
				if (File.Exists(state)) File.Delete(state);
				File.Move(temporary, state);
			}
			catch (Exception exception) when (RecoverableException(exception))
			{ Warning(report, "integrity.record", exception.Message, state); }
			return report;
		}

		public static ValidationReport PreserveModifiedCopy(string i_modsDirectory, string i_id)
		{
			ValidationReport report = new ValidationReport();
			if (!TryPackAndStatePaths(i_modsDirectory, i_id, out string pack, out string state)
				|| !ModInstallRecovery.SafeDirectory(pack))
			{ Error(report, "integrity.preserve", "Installed pack cannot be safely copied.", pack); return report; }
			string parent = Directory.GetParent(Path.GetFullPath(i_modsDirectory))?.FullName;
			if (parent == null) { Error(report, "integrity.preserve", "Mods directory has no safe parent.", i_modsDirectory); return report; }
			string version = ModInstallRecovery.ReadVersion(pack, i_id) ?? "unknown";
			string modifiedRoot = Path.GetFullPath(Path.Combine(parent, ".mod-modified"));
			if (Directory.Exists(modifiedRoot) && !ModInstallRecovery.SafeDirectory(modifiedRoot))
			{ Error(report, "integrity.preserve", "Modified-copy storage is a reparse point.", modifiedRoot); return report; }
			string destination = Path.GetFullPath(Path.Combine(modifiedRoot,
				i_id + "-v" + SafeName(version) + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N")));
			if (!AssetPatchDiscovery.IsInside(modifiedRoot, parent) || !AssetPatchDiscovery.IsInside(destination, modifiedRoot)
				|| Directory.Exists(destination) || File.Exists(destination))
			{ Error(report, "integrity.preserve", "Could not allocate safe modified-copy storage.", destination); return report; }
			try
			{
				Directory.CreateDirectory(destination);
				CopyTree(pack, destination);
				report.Add(ValidationSeverity.Info, "integrity.preserved", "Preserved locally modified copy at " + destination, destination);
			}
			catch (Exception exception) when (RecoverableException(exception))
			{
				Error(report, "integrity.preserve", exception.Message + " Partial copy may remain at " + destination, destination);
			}
			return report;
		}

		private static void CopyTree(string i_source, string i_destination)
		{
			List<string> files = EnumerateSafeFiles(i_source, out long totalBytes);
			foreach (string source in files)
			{
				string relative = source.Substring(i_source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string destination = Path.GetFullPath(Path.Combine(i_destination, relative));
				if (!AssetPatchDiscovery.IsInside(destination, i_destination)) throw new InvalidDataException("Modified-copy path escaped its destination.");
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
				File.Copy(source, destination, false);
			}
		}

		private static bool TryFingerprint(string i_root, out string o_fingerprint, out string o_error)
		{
			o_fingerprint = null; o_error = null;
			try
			{
				List<string> files = EnumerateSafeFiles(i_root, out long totalBytes);
				using (SHA256 sha = SHA256.Create())
				{
					byte[] buffer = new byte[65536];
					foreach (string file in files)
					{
						string relative = file.Substring(i_root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
							.Replace(Path.DirectorySeparatorChar, '/');
						byte[] name = Encoding.UTF8.GetBytes(relative);
						byte[] header = new byte[4 + name.Length + 8];
						Buffer.BlockCopy(BitConverter.GetBytes(name.Length), 0, header, 0, 4);
						Buffer.BlockCopy(name, 0, header, 4, name.Length);
						long length = new FileInfo(file).Length;
						Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 4 + name.Length, 8);
						sha.TransformBlock(header, 0, header.Length, null, 0);
						using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							int read;
							while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
								sha.TransformBlock(buffer, 0, read, null, 0);
						}
					}
					sha.TransformFinalBlock(new byte[0], 0, 0);
					o_fingerprint = BitConverter.ToString(sha.Hash).Replace("-", string.Empty).ToLowerInvariant();
				}
				return true;
			}
			catch (Exception exception) when (RecoverableException(exception))
			{ o_error = exception.Message; return false; }
		}

		private static List<string> EnumerateSafeFiles(string i_root, out long o_totalBytes)
		{
			o_totalBytes = 0;
			string root = Path.GetFullPath(i_root);
			if (!ModInstallRecovery.SafeDirectory(root)) throw new InvalidDataException("Pack root is missing or is a reparse point.");
			List<string> files = new List<string>();
			Stack<string> pending = new Stack<string>(); pending.Push(root);
			while (pending.Count > 0)
			{
				string directory = pending.Pop();
				foreach (string childDirectory in Directory.GetDirectories(directory))
				{
					string full = Path.GetFullPath(childDirectory);
					if (!AssetPatchDiscovery.IsInside(full, root) || !ModInstallRecovery.SafeDirectory(full))
						throw new InvalidDataException("Pack contains an unsafe directory or reparse point.");
					pending.Push(full);
				}
				foreach (string childFile in Directory.GetFiles(directory))
				{
					string full = Path.GetFullPath(childFile);
					FileInfo info = new FileInfo(full);
					if (!AssetPatchDiscovery.IsInside(full, root) || (info.Attributes & FileAttributes.ReparsePoint) != 0)
						throw new InvalidDataException("Pack contains an unsafe file or reparse point.");
					if (info.Length < 0 || o_totalBytes > MaximumBytes - info.Length)
						throw new InvalidDataException("Pack exceeds the 1 GiB integrity limit.");
					o_totalBytes += info.Length;
					files.Add(full);
					if (files.Count > MaximumFiles) throw new InvalidDataException("Pack exceeds the 4096-file integrity limit.");
				}
			}
			files.Sort((left, right) => string.Compare(Relative(root, left), Relative(root, right), StringComparison.Ordinal));
			return files;
		}

		private static string Relative(string i_root, string i_file)
		{ return i_file.Substring(i_root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/'); }

		private static ModInstallLedger ReadLedger(string i_path, string i_id)
		{
			try
			{
				if (!File.Exists(i_path) || (new FileInfo(i_path).Attributes & FileAttributes.ReparsePoint) != 0
					|| new FileInfo(i_path).Length > 256L * 1024L) return null;
				ModInstallLedger ledger = JsonConvert.DeserializeObject<ModInstallLedger>(File.ReadAllText(i_path),
					new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error, MaxDepth = 8 });
				if (ledger == null || ledger.SchemaVersion != 1 || ledger.Id != i_id || ledger.Records == null
					|| ledger.Records.Count > MaximumRecords) return null;
				foreach (ModInstallRecord record in ledger.Records)
					if (record == null || !SemanticVersion.TryParse(record.Version, out _) || !Sha256Text(record.ArchiveSha256)
						|| !Sha256Text(record.Fingerprint)) return null;
				return ledger;
			}
			catch (Exception exception) when (RecoverableException(exception) || exception is JsonException) { return null; }
		}

		private static bool TryPackAndStatePaths(string i_modsDirectory, string i_id, out string o_pack, out string o_state)
		{
			o_pack = o_state = null;
			if (!ModInstallRecovery.TryPaths(i_modsDirectory, i_id, out string mods, out string target,
				out string backup, out string removed)) return false;
			string parent = Directory.GetParent(mods)?.FullName;
			if (parent == null) return false;
			o_pack = target;
			string stateRoot = Path.GetFullPath(Path.Combine(parent, ".mod-install-state"));
			if (!AssetPatchDiscovery.IsInside(stateRoot, parent)
				|| (Directory.Exists(stateRoot) && !ModInstallRecovery.SafeDirectory(stateRoot))) return false;
			o_state = Path.GetFullPath(Path.Combine(stateRoot, i_id + ".json"));
			return AssetPatchDiscovery.IsInside(o_state, stateRoot);
		}

		private static string SafeName(string i_value)
		{
			StringBuilder result = new StringBuilder();
			foreach (char character in i_value) if (char.IsLetterOrDigit(character) || character == '.' || character == '-') result.Append(character);
			return result.Length == 0 ? "unknown" : result.ToString();
		}

		private static bool Sha256Text(string i_value)
		{
			if (i_value == null || i_value.Length != 64) return false;
			foreach (char character in i_value)
				if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f') || (character >= 'A' && character <= 'F'))) return false;
			return true;
		}

		private static bool RecoverableException(Exception i_exception)
		{
			return i_exception is IOException || i_exception is UnauthorizedAccessException || i_exception is InvalidDataException
				|| i_exception is ArgumentException || i_exception is NotSupportedException || i_exception is SecurityException;
		}

		private static void Warning(ValidationReport io_report, string i_code, string i_message, string i_source)
		{ io_report.Add(ValidationSeverity.Warning, i_code, i_message, i_source); }
		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{ io_report.Add(ValidationSeverity.Error, i_code, i_message, i_source); }
	}
}
