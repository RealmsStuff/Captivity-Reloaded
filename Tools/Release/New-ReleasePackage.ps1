param(
	[string]$Version = '2.2.1',
	[string]$ProjectRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
	$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

$releaseRoot = Join-Path $ProjectRoot "Builds\Release-$Version"
$stagingRoot = Join-Path $releaseRoot '.staging'
$legalRoot = Join-Path $stagingRoot 'Legal'
$windowsBuild = Join-Path $ProjectRoot "Builds\Windows-$Version"
$webGlBuild = Join-Path $ProjectRoot "Builds\WebGL-$Version"
$androidBuild = Join-Path $ProjectRoot "Builds\Android\Captivity-Reloaded-$Version.apk"
$patchNotes = Join-Path $ProjectRoot "Docs\PatchNotes\$Version.md"
$nonShippingDirectoryPatterns = @(
	'*_BurstDebugInformation_DoNotShip',
	'BackUpThisFolder_ButDontShipItWithYourGame'
)

foreach ($required in @($windowsBuild, $webGlBuild, $androidBuild, $patchNotes)) {
	if (-not (Test-Path -LiteralPath $required)) {
		throw "Missing release input: $required"
	}
}

if (Test-Path -LiteralPath $releaseRoot) {
	Remove-Item -LiteralPath $releaseRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $legalRoot -Force | Out-Null

Copy-Item -LiteralPath (Join-Path $ProjectRoot 'LICENSE') -Destination (Join-Path $legalRoot 'LICENSE.txt')
Copy-Item -LiteralPath (Join-Path $ProjectRoot 'THIRD_PARTY_NOTICES.md') -Destination $legalRoot
Copy-Item -LiteralPath (Join-Path $ProjectRoot 'ThirdPartyLicenses') -Destination $legalRoot -Recurse
Copy-Item -LiteralPath (Join-Path $ProjectRoot 'Assets\Resources\InputGlyphs\Controller-License.txt') -Destination (Join-Path $legalRoot 'Kenney-Input-Prompts-CC0.txt')
Copy-Item -LiteralPath (Join-Path $ProjectRoot 'Assets\Resources\InputGlyphs\Mobile-License.txt') -Destination (Join-Path $legalRoot 'Kenney-Mobile-Controls-CC0.txt')

$packageLicenseRoot = Join-Path $legalRoot 'UnityPackages'
New-Item -ItemType Directory -Path $packageLicenseRoot -Force | Out-Null
$packageLock = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Packages\packages-lock.json') -Raw | ConvertFrom-Json
foreach ($property in $packageLock.dependencies.PSObject.Properties) {
	$packageName = $property.Name
	$versionValue = [string]$property.Value.version
	$cachePath = Join-Path $ProjectRoot "Library\PackageCache\$packageName@$versionValue"
	if (-not (Test-Path -LiteralPath $cachePath)) {
		throw "Package cache is missing $packageName@$versionValue; open the project in Unity before packaging."
	}
	$licenseFiles = @(Get-ChildItem -LiteralPath $cachePath -File | Where-Object {
		$_.Name -match '^(LICENSE|Third Party Notices)(\.(md|txt))?$'
	})
	if ($licenseFiles.Count -eq 0) {
		$packageManifest = Get-Content -LiteralPath (Join-Path $cachePath 'package.json') -Raw | ConvertFrom-Json
		if ([string]$packageManifest.type -eq 'feature' -or [string]$property.Value.source -eq 'builtin') { continue }
		throw "No top-level license or notice file found for $packageName@$versionValue."
	}
	$packageOutput = Join-Path $packageLicenseRoot "$packageName@$versionValue"
	New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null
	foreach ($licenseFile in $licenseFiles) {
		Copy-Item -LiteralPath $licenseFile.FullName -Destination $packageOutput
	}
}

$sourceText = @"
Captivity Reloaded $Version source

Repository: https://github.com/RealmsStuff/Captivity-Reloaded
Release tag: https://github.com/RealmsStuff/Captivity-Reloaded/tree/v$Version
License: GNU GPL version 3; see LICENSE.txt in this directory.
"@
Set-Content -LiteralPath (Join-Path $legalRoot 'SOURCE.txt') -Value $sourceText -Encoding UTF8

$windowsStage = Join-Path $stagingRoot 'Windows'
$webGlStage = Join-Path $stagingRoot 'WebGL'
New-Item -ItemType Directory -Path $windowsStage, $webGlStage -Force | Out-Null
foreach ($entry in @(Get-ChildItem -LiteralPath $windowsBuild -Force)) {
	$isNonShipping = $false
	foreach ($pattern in $nonShippingDirectoryPatterns) {
		if ($entry.PSIsContainer -and $entry.Name -like $pattern) {
			$isNonShipping = $true
			break
		}
	}
	if (-not $isNonShipping) {
		Copy-Item -LiteralPath $entry.FullName -Destination $windowsStage -Recurse -Force
	}
}
Copy-Item -Path (Join-Path $webGlBuild '*') -Destination $webGlStage -Recurse -Force
Copy-Item -LiteralPath $legalRoot -Destination $windowsStage -Recurse
Copy-Item -LiteralPath $legalRoot -Destination $webGlStage -Recurse

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
function New-ZipArchive {
	param([string]$SourceDirectory, [string]$DestinationPath)
	if (Test-Path -LiteralPath $DestinationPath) { Remove-Item -LiteralPath $DestinationPath -Force }
	$sourceRoot = (Resolve-Path -LiteralPath $SourceDirectory).Path.TrimEnd('\', '/')
	$outputStream = [System.IO.File]::Open($DestinationPath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
	$archive = $null
	try {
		$archive = [System.IO.Compression.ZipArchive]::new($outputStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)
		foreach ($file in @(Get-ChildItem -LiteralPath $sourceRoot -File -Recurse | Sort-Object FullName)) {
			$relativePath = $file.FullName.Substring($sourceRoot.Length).TrimStart('\', '/').Replace('\', '/')
			$entry = $archive.CreateEntry($relativePath, [System.IO.Compression.CompressionLevel]::Optimal)
			$entry.LastWriteTime = [System.DateTimeOffset]$file.LastWriteTime
			$inputStream = [System.IO.File]::OpenRead($file.FullName)
			$entryStream = $entry.Open()
			try {
				$inputStream.CopyTo($entryStream)
			}
			finally {
				$entryStream.Dispose()
				$inputStream.Dispose()
			}
		}
	}
	finally {
		if ($null -ne $archive) { $archive.Dispose() }
		else { $outputStream.Dispose() }
	}
}

function Assert-NoNonShippingBuildOutput {
	param([string]$ArchivePath)
	$archive = [System.IO.Compression.ZipFile]::OpenRead($ArchivePath)
	try {
		$nonPortable = @($archive.Entries | Where-Object { $_.FullName.Contains('\') })
		if ($nonPortable.Count -gt 0) {
			throw "Release archive contains a non-portable backslash path: $($nonPortable[0].FullName)"
		}
		$forbidden = @($archive.Entries | Where-Object {
			$entryPath = $_.FullName.Replace('\', '/')
			$segments = @($entryPath.Split('/', [System.StringSplitOptions]::RemoveEmptyEntries))
			$segments | Where-Object {
				$segment = $_
				$nonShippingDirectoryPatterns | Where-Object { $segment -like $_ }
			} | Select-Object -First 1
		})
		if ($forbidden.Count -gt 0) {
			throw "Release archive contains Unity non-shipping build output: $($forbidden[0].FullName)"
		}
	}
	finally {
		$archive.Dispose()
	}
}

$windowsZip = Join-Path $releaseRoot "Captivity-Reloaded-$Version-Windows.zip"
$webGlZip = Join-Path $releaseRoot "Captivity-Reloaded-$Version-WebGL.zip"
$licensesZip = Join-Path $releaseRoot "Captivity-Reloaded-$Version-Licenses.zip"
$androidOutput = Join-Path $releaseRoot "Captivity-Reloaded-$Version-Android.apk"
New-ZipArchive $windowsStage $windowsZip
New-ZipArchive $webGlStage $webGlZip
New-ZipArchive $legalRoot $licensesZip
Assert-NoNonShippingBuildOutput $windowsZip
Assert-NoNonShippingBuildOutput $webGlZip
Copy-Item -LiteralPath $androidBuild -Destination $androidOutput
Copy-Item -LiteralPath $patchNotes -Destination (Join-Path $releaseRoot 'PATCH-NOTES.md')

Remove-Item -LiteralPath $stagingRoot -Recurse -Force

$hashTargets = @(Get-ChildItem -LiteralPath $releaseRoot -File | Sort-Object Name)
$hashLines = foreach ($file in $hashTargets) {
	$hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
	"$hash  $($file.Name)"
}
Set-Content -LiteralPath (Join-Path $releaseRoot 'SHA256SUMS.txt') -Value $hashLines -Encoding ASCII

Get-ChildItem -LiteralPath $releaseRoot -File | Sort-Object Name | Select-Object Name, Length
