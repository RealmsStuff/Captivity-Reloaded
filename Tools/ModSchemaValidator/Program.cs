using System.Text.Json;
using Json.Schema;

string repository = args.Length == 0
    ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"))
    : Path.GetFullPath(args[0]);
string schemaDirectory = Path.Combine(repository, "Docs", "Modding", "schemas");
string examplesDirectory = args.Length > 1
    ? Path.GetFullPath(args[1])
    : Path.Combine(repository, "ExampleMods");
if (!Directory.Exists(examplesDirectory))
    throw new DirectoryNotFoundException("Validation directory was not found: " + examplesDirectory);

Dictionary<string, JsonSchema> schemas = new(StringComparer.Ordinal);
string[] expectedSchemas =
{
    "manifest", "asset-patch", "challenge", "clothing", "difficulty", "enemy", "enemy-animation", "enemy-rig-reference",
    "player-animation", "player-attachment", "player-rig", "rule-profile", "stage", "stage-script", "tiled-map", "tiled-tileset", "usable", "weapon"
};
foreach (string path in Directory.GetFiles(schemaDirectory)
    .Where(path => path.EndsWith(".schema.json", StringComparison.OrdinalIgnoreCase)))
{
    string schemaName = Path.GetFileName(path).Replace(".schema.json", string.Empty);
    // The community catalog has its own Draft-07 contract and validator. It is
    // intentionally not one of the per-pack Mod API content schemas below.
    if (!expectedSchemas.Contains(schemaName, StringComparer.Ordinal)) continue;
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    JsonElement root = document.RootElement;
    if (!root.TryGetProperty("$schema", out JsonElement dialect)
        || dialect.GetString() != "https://json-schema.org/draft/2020-12/schema")
        throw new InvalidDataException(Path.GetFileName(path) + " must declare JSON Schema Draft 2020-12.");
    if (!root.TryGetProperty("x-stability", out _))
        throw new InvalidDataException(Path.GetFileName(path) + " has no root x-stability annotation.");
    if (root.TryGetProperty("properties", out JsonElement properties))
        foreach (JsonProperty property in properties.EnumerateObject())
            if (!property.Value.TryGetProperty("x-stability", out _))
                throw new InvalidDataException(Path.GetFileName(path) + " property '" + property.Name + "' has no x-stability annotation.");
    schemas[schemaName] = JsonSchema.Build(document.RootElement.Clone(),
        new BuildOptions { Dialect = Dialect.Draft202012 });
}
foreach (string expected in expectedSchemas)
    if (!schemas.ContainsKey(expected)) throw new FileNotFoundException("Missing public Mod API schema: " + expected + ".schema.json");
if (schemas.Count != expectedSchemas.Length)
    throw new InvalidDataException("Unexpected public schema inventory; update the validator's explicit v1 schema list.");

List<string> failures = new();
int validated = 0;
foreach (string path in Directory.GetFiles(examplesDirectory, "*", SearchOption.AllDirectories)
    .Where(path => path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".tsj", StringComparison.OrdinalIgnoreCase)))
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    JsonElement root = document.RootElement;
    if (root.ValueKind != JsonValueKind.Object) continue;
    string? schemaName = null;
    if (path.EndsWith(".tsj", StringComparison.OrdinalIgnoreCase)) schemaName = "tiled-tileset";
    else if (Path.GetFileName(path).Equals("manifest.json", StringComparison.OrdinalIgnoreCase)) schemaName = "manifest";
    else if (root.TryGetProperty("type", out JsonElement type))
    {
        schemaName = type.GetString() switch
        {
            "assetPatch" => "asset-patch",
            "challenge" => "challenge",
            "clothing" => "clothing",
            "difficulty" => "difficulty",
            "enemy" => "enemy",
            "enemyAnimation" => "enemy-animation",
            "enemyRigReference" => "enemy-rig-reference",
            "playerAnimation" => "player-animation",
            "playerAttachment" => "player-attachment",
            "playerRig" => "player-rig",
            "ruleProfile" => "rule-profile",
            "stage" => "stage",
            "stageScript" => "stage-script",
            "usable" => "usable",
            "weapon" => "weapon",
            _ => null
        };
    }
    if (schemaName == null && root.TryGetProperty("tiledversion", out _) && root.TryGetProperty("layers", out _))
    {
        schemaName = "tiled-map";
    }
    if (schemaName == null) continue; // Referenced Tiled support documents are not Mod API content documents.
    EvaluationResults result = schemas[schemaName].Evaluate(root,
        new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
    validated++;
    if (!result.IsValid)
        failures.Add(Path.GetRelativePath(repository, path) + Environment.NewLine + FormatErrors(result));
}

if (failures.Count > 0)
{
    Console.Error.WriteLine($"Schema validation failed for {failures.Count} of {validated} documents:");
    foreach (string failure in failures) Console.Error.WriteLine(failure);
    return 1;
}
Console.WriteLine($"Validated {validated} documents under {Path.GetRelativePath(repository, examplesDirectory)} against Mod API v1 schemas.");
return 0;

static string FormatErrors(EvaluationResults result)
{
    List<string> messages = new();
    CollectErrors(result, messages);
    return string.Join(Environment.NewLine, messages.Distinct().Take(50));
}

static void CollectErrors(EvaluationResults result, List<string> messages)
{
    if (result.Errors != null)
        foreach (KeyValuePair<string, string> error in result.Errors)
            if (error.Key != "properties" && error.Key != "items")
                messages.Add($"{result.InstanceLocation} [{error.Key}] {error.Value}");
    if (result.Details != null)
        foreach (EvaluationResults detail in result.Details)
            if (!detail.IsValid) CollectErrors(detail, messages);
}
