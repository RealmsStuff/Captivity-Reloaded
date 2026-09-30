using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Json.Schema;

const int MaxArchiveBytes = 256 * 1024 * 1024;
const int MaxPreviewBytes = 8 * 1024 * 1024;
const int MaxFiles = 4096;
const long MaxExtractedBytes = 512L * 1024L * 1024L;
HashSet<string> allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    { ".json", ".tsj", ".tx", ".png", ".ogg", ".wav", ".md", ".txt" };

string repository = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "../../../../../"));
string catalogPath = Path.GetFullPath(args.Length > 1 ? args[1] : Path.Combine(repository, "ModCatalog", "catalog-v1.json"));
string schemaPath = Path.Combine(repository, "Docs", "Modding", "schemas", "catalog-v1.schema.json");
List<string> failures = new();
string extractionRoot = Path.Combine(Path.GetTempPath(), "captivity-catalog-validation-" + Guid.NewGuid().ToString("N"));

try
{
    using JsonDocument catalogDocument = JsonDocument.Parse(File.ReadAllText(catalogPath));
    using JsonDocument schemaDocument = JsonDocument.Parse(File.ReadAllText(schemaPath));
    JsonSchema schema = JsonSchema.Build(schemaDocument.RootElement.Clone(), new BuildOptions { Dialect = Dialect.Draft07 });
    EvaluationResults schemaResult = schema.Evaluate(catalogDocument.RootElement,
        new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
    if (!schemaResult.IsValid) failures.Add("Catalog does not match catalog-v1.schema.json.");

    Directory.CreateDirectory(extractionRoot);
    using HttpClient client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
    client.Timeout = TimeSpan.FromSeconds(45);
    HashSet<string> ids = new(StringComparer.Ordinal);
    HashSet<string> releases = new(StringComparer.OrdinalIgnoreCase);
    int archiveCount = 0;

    foreach (JsonElement pack in catalogDocument.RootElement.GetProperty("packs").EnumerateArray())
    {
        string id = pack.GetProperty("id").GetString() ?? string.Empty;
        string repositoryUrl = pack.GetProperty("sourceRepository").GetString() ?? string.Empty;
        if (!ids.Add(id)) failures.Add($"Duplicate pack ID: {id}");

        if (pack.TryGetProperty("previewImages", out JsonElement previews))
            foreach (JsonElement preview in previews.EnumerateArray())
            {
                string url = preview.GetString() ?? string.Empty;
                try
                {
					if (!url.StartsWith(repositoryUrl + "/releases/download/", StringComparison.OrdinalIgnoreCase))
						throw new InvalidDataException("preview belongs to another repository");
                    byte[] image = await DownloadBounded(client, url, MaxPreviewBytes);
                    if (image.Length < 24 || image[0] != 0x89 || image[1] != 0x50 || image[2] != 0x4e || image[3] != 0x47)
                        failures.Add($"{id}: preview is not a PNG: {url}");
                    else
                    {
                        int width = ReadBigEndian(image, 16), height = ReadBigEndian(image, 20);
                        if (width < 1 || height < 1 || width > 8192 || height > 8192 || (long)width * height > 32L * 1024L * 1024L)
                            failures.Add($"{id}: preview dimensions exceed runtime limits: {url}");
                    }
                }
                catch (Exception exception) { failures.Add($"{id}: preview download failed: {exception.Message}"); }
            }

        HashSet<string> versions = new(StringComparer.Ordinal);
        foreach (JsonElement version in pack.GetProperty("versions").EnumerateArray())
        {
            string versionText = version.GetProperty("version").GetString() ?? string.Empty;
            string download = version.GetProperty("download").GetString() ?? string.Empty;
            string expectedHash = version.GetProperty("sha256").GetString() ?? string.Empty;
            long expectedSize = version.GetProperty("sizeBytes").GetInt64();
            if (!versions.Add(versionText)) failures.Add($"{id}: duplicate version {versionText}");
            if (!download.StartsWith(repositoryUrl + "/releases/download/", StringComparison.OrdinalIgnoreCase))
                failures.Add($"{id} {versionText}: release asset belongs to another repository.");
            if (!releases.Add(id + "@" + versionText)) failures.Add($"Duplicate catalog release: {id}@{versionText}");
            try
            {
                byte[] archive = await DownloadBounded(client, download, Math.Min(MaxArchiveBytes, checked((int)expectedSize)));
                if (archive.LongLength != expectedSize) throw new InvalidDataException($"size is {archive.LongLength}, expected {expectedSize}");
                string actualHash = Convert.ToHexString(SHA256.HashData(archive)).ToLowerInvariant();
                if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"SHA-256 is {actualHash}, expected {expectedHash}");
                string target = Path.Combine(extractionRoot, id + "-" + versionText);
                ValidateAndExtract(archive, target, id, version, failures, allowedExtensions);
                archiveCount++;
            }
            catch (Exception exception) { failures.Add($"{id} {versionText}: {exception.Message}"); }
        }
    }

    if (failures.Count == 0)
    {
        ProcessStartInfo start = new("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add("run"); start.ArgumentList.Add("--project");
        start.ArgumentList.Add(Path.Combine(repository, "Tools", "ModSchemaValidator"));
        start.ArgumentList.Add("--"); start.ArgumentList.Add(repository); start.ArgumentList.Add(extractionRoot);
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the content schema validator.");
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0) failures.Add("Extracted content schema validation failed:\n" + error + output);
    }

    if (failures.Count > 0)
    {
        Console.Error.WriteLine($"Catalog validation failed with {failures.Count} problem(s):");
        foreach (string failure in failures) Console.Error.WriteLine("- " + failure);
        return 1;
    }
    Console.WriteLine($"Validated {ids.Count} catalog packs and {archiveCount} immutable release archives.");
    return 0;
}
finally
{
    if (Directory.Exists(extractionRoot)) Directory.Delete(extractionRoot, true);
}

static async Task<byte[]> DownloadBounded(HttpClient client, string url, int maximum)
{
    using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    if (response.Content.Headers.ContentLength is long length && (length < 1 || length > maximum))
        throw new InvalidDataException($"download size {length} exceeds the {maximum}-byte limit");
    await using Stream input = await response.Content.ReadAsStreamAsync();
    using MemoryStream output = new();
    byte[] buffer = new byte[65536];
    int read;
    while ((read = await input.ReadAsync(buffer)) > 0)
    {
        if (output.Length > maximum - read) throw new InvalidDataException($"download exceeds the {maximum}-byte limit");
        output.Write(buffer, 0, read);
    }
    if (output.Length == 0) throw new InvalidDataException("download is empty");
    return output.ToArray();
}

static void ValidateAndExtract(byte[] archive, string target, string expectedId, JsonElement catalogVersion,
    List<string> failures, HashSet<string> allowedExtensions)
{
    using MemoryStream input = new(archive, false);
    using ZipArchive zip = new(input, ZipArchiveMode.Read);
    if (zip.Entries.Count > MaxFiles) throw new InvalidDataException("archive contains too many entries");
    long total = 0;
    int manifests = 0;
	HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
    Directory.CreateDirectory(target);
    foreach (ZipArchiveEntry entry in zip.Entries)
    {
        string name = entry.FullName;
        bool directory = name.EndsWith('/');
        string[] parts = name.TrimEnd('/').Split('/');
        if (string.IsNullOrEmpty(name) || name.StartsWith('/') || name.Contains('\\') || name.Contains(':')
			|| name.IndexOfAny(['<', '>', '"', '|', '?', '*']) >= 0
			|| parts.Any(part => part.Length == 0 || part is "." or ".." || part.EndsWith('.') || part.EndsWith(' ')
				|| IsWindowsDeviceName(part)) || !paths.Add(name.TrimEnd('/')))
            throw new InvalidDataException("archive contains an unsafe path: " + name);
		int unixType = (entry.ExternalAttributes >> 16) & 0xf000;
		if ((unixType != 0 && unixType != 0x8000 && unixType != 0x4000)
			|| (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0)
			throw new InvalidDataException("archive links and special files are forbidden: " + name);
        if (!directory && !allowedExtensions.Contains(Path.GetExtension(name)))
            throw new InvalidDataException("archive contains a forbidden extension: " + name);
		long entryLimit = EntryLimit(name);
		if (entry.Length < 0 || entry.Length > entryLimit || total > MaxExtractedBytes - entry.Length)
            throw new InvalidDataException("archive exceeds the extracted-size limit");
        total += entry.Length;
        string destination = Path.GetFullPath(Path.Combine(target, name.Replace('/', Path.DirectorySeparatorChar)));
        if (!destination.StartsWith(Path.GetFullPath(target) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("archive path escapes extraction root: " + name);
        if (directory) { Directory.CreateDirectory(destination); continue; }
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        entry.ExtractToFile(destination, false);
        if (name.Equals("manifest.json", StringComparison.OrdinalIgnoreCase)) manifests++;
    }
    if (manifests != 1) throw new InvalidDataException("archive must contain exactly one root manifest.json");
    using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(target, "manifest.json")));
    JsonElement root = manifest.RootElement;
    if (root.GetProperty("id").GetString() != expectedId
        || root.GetProperty("version").GetString() != catalogVersion.GetProperty("version").GetString()
        || root.GetProperty("modApiVersion").GetInt32() != catalogVersion.GetProperty("modApiVersion").GetInt32())
        throw new InvalidDataException("manifest identity differs from catalog release");
    if (!CanonicalRequirements(root, "dependencies").SetEquals(CanonicalRequirements(catalogVersion, "dependencies")))
        failures.Add($"{expectedId}: manifest dependencies differ from the catalog");
    if (!CanonicalStrings(root, "conflicts").SetEquals(CanonicalStrings(catalogVersion, "conflicts")))
        failures.Add($"{expectedId}: manifest conflicts differ from the catalog");
}

static HashSet<string> CanonicalRequirements(JsonElement root, string name)
{
    HashSet<string> result = new(StringComparer.Ordinal);
    if (root.TryGetProperty(name, out JsonElement values))
        foreach (JsonElement value in values.EnumerateArray())
            result.Add((value.GetProperty("id").GetString() ?? "") + "\n" + (value.GetProperty("version").GetString() ?? ""));
    return result;
}

static HashSet<string> CanonicalStrings(JsonElement root, string name)
{
    HashSet<string> result = new(StringComparer.Ordinal);
    if (root.TryGetProperty(name, out JsonElement values))
        foreach (JsonElement value in values.EnumerateArray()) result.Add(value.GetString() ?? "");
    return result;
}

static int ReadBigEndian(byte[] bytes, int offset) =>
    (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

static long EntryLimit(string path)
{
	if (path.Equals("manifest.json", StringComparison.OrdinalIgnoreCase)) return 256L * 1024L;
	string extension = Path.GetExtension(path);
	if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase)) return 32L * 1024L * 1024L;
	if (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)
		|| extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase)) return 64L * 1024L * 1024L;
	return 1024L * 1024L;
}

static bool IsWindowsDeviceName(string part)
{
	string stem = part.Split('.')[0].ToUpperInvariant();
	if (stem is "CON" or "PRN" or "AUX" or "NUL") return true;
	return stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal)
		|| stem.StartsWith("LPT", StringComparison.Ordinal)) && stem[3] is >= '1' and <= '9';
}
