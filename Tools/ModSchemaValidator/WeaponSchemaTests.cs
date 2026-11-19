using System.Text.Json.Nodes;
using System.Text.Json;
using Json.Schema;

// Run with: dotnet run --project Tools/ModSchemaValidator -- <repository> --self-test
internal static class WeaponSchemaTests
{
    public static int Run(JsonSchema schema)
    {
        var weapon = JsonNode.Parse("""
        {"schemaVersion":1,"type":"weapon","id":"test.pose:item/weapon/test","displayName":"Test",
         "extends":"core:item/weapon/pistol","stats":{"holdType":"TwoHanded1"},
         "visual":{"type":"coreWeaponSprites","sprites":{"base":"assets/base.png"}},
         "holdPose":{"rightUpper":-45,"rightLower":65,"rightHand":90,"useLeftHand":true,"leftUpper":60,"leftLower":75,"leftHand":80}}
        """)!;
        int count = 0;
        void Check(JsonNode value, bool expected, string label)
        {
            using var document = JsonDocument.Parse(value.ToJsonString());
            if (schema.Evaluate(document.RootElement).IsValid != expected) throw new InvalidDataException("Weapon schema self-test: " + label);
            count++;
        }
        Check(weapon, true, "custom grip");
        var legacy = weapon.DeepClone(); legacy.AsObject().Remove("holdPose"); Check(legacy, true, "legacy preset");
        foreach (string field in new[] { "rightUpper", "rightLower", "rightHand", "leftUpper", "leftLower", "leftHand" })
        {
            foreach (int value in new[] { -361, 361 }) { var bad = weapon.DeepClone(); bad["holdPose"]![field] = value; Check(bad, false, field + " bounds"); }
            var missing = weapon.DeepClone(); missing["holdPose"]!.AsObject().Remove(field); Check(missing, false, field + " required");
        }
        var stocked = weapon.DeepClone(); stocked["stats"]!["stock"] = false; Check(stocked, true, "stock flag");
        var badStock = weapon.DeepClone(); badStock["stats"]!["stock"] = "sometimes"; Check(badStock, false, "boolean stock");
        var fallback = weapon.DeepClone(); fallback["stats"]!.AsObject().Remove("holdType"); Check(fallback, false, "fallback required");
        var typo = weapon.DeepClone(); typo["holdPose"]!["typo"] = 0; Check(typo, false, "unknown field");
        var nullPose = weapon.DeepClone(); nullPose["holdPose"] = null; Check(nullPose, false, "null pose");
        var badFlag = weapon.DeepClone(); badFlag["holdPose"]!["useLeftHand"] = 1; Check(badFlag, false, "boolean flag");
        Console.WriteLine($"Passed {count} weapon schema self-tests.");
        return 0;
    }
}
