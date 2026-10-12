# Publishing to the community catalog

The catalog accepts immutable `.capmod` or ZIP assets attached to GitHub Releases. A `.capmod` is the preferred output because the project packager validates and builds it directly; internally it is a deterministic ZIP. A moving branch ZIP is not accepted because its contents can change without changing the catalog record.

## Prepare a release

1. Validate the unpacked pack with `Tools/ModSchemaValidator`.
2. Build and test the final artifact using [Packaging a `.capmod`](packaging.md).
3. Keep `manifest.json` at the archive root; do not add an extra enclosing folder.
4. Publish the exact tested `.capmod` or ZIP as a versioned GitHub Release asset.
5. Record its exact byte size and lowercase SHA-256 digest.
6. Optionally attach up to eight PNG previews to the same repository's GitHub Releases.

Each catalog listing contains stable display metadata and one or more immutable releases:

```json
{
  "id": "example.author.example-mod",
  "displayName": "Example Mod",
  "authors": ["Example Author"],
  "summary": "Adds an example enemy.",
  "sourceRepository": "https://github.com/example/example-mod",
  "tags": ["enemy"],
  "contentWarnings": [],
  "previewImages": [
    "https://github.com/example/example-mod/releases/download/v1.0.0/preview.png"
  ],
  "versions": [
    {
      "version": "1.0.0",
      "modApiVersion": 1,
      "gameVersion": ">=2.1.2",
      "download": "https://github.com/example/example-mod/releases/download/v1.0.0/example.author.example-mod-1.0.0.capmod",
      "sha256": "64-lowercase-hexadecimal-digits",
      "sizeBytes": 123456,
      "dependencies": [{"id": "core", "version": ">=0.1.0"}],
      "conflicts": []
    }
  ]
}
```

Catalog dependencies and conflicts must exactly match the release manifest. Required dependencies are downloaded together when compatible catalog versions exist. Optional dependencies remain manifest-only and are never installed automatically.

## Submit and validate

Submit the catalog record through a pull request. Before review, run:

```powershell
dotnet run --project Tools/ModCatalogValidator -- . ModCatalog/catalog-v1.json
```

The contribution check validates the catalog schema, downloads every release and preview, enforces transfer limits, verifies archive sizes and SHA-256 digests, rejects unsafe ZIP paths and executable file types, compares catalog metadata with each manifest, and validates the extracted content against the public Mod API schemas.

Maintainers still review the description, tags, compatibility claim, screenshots, licensing, and content warnings. Passing automation means the package is structurally safe and reproducible; it is not a gameplay-quality endorsement.

Complete the [mod-author test matrix](../reference/author-test-matrix.md) before submission and include the platforms actually tested in the pull request description.
