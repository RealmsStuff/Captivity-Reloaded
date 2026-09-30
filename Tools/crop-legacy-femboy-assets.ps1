$ErrorActionPreference = "Stop"
$extractor = Join-Path $PSScriptRoot "extract-femboy-edition.py"
python $extractor
if ($LASTEXITCODE -ne 0) { throw "Femboy Edition extraction failed with exit code $LASTEXITCODE." }
