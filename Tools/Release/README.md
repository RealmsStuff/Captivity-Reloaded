# Release tools

Run the read-only release audit from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Release\Invoke-ReleaseAudit.ps1
```

The audit never deletes, stages, commits, downloads, tests, or builds anything. It blocks a release when the source snapshot is dirty, generated files are tracked, a Unity platform module or test suite is missing, the stress arena is only a local installed mod, the temporary catalog is still configured, or licensing policy files are absent.

From a fresh release-candidate checkout, run both suites and reject zero-test runs:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Release\Invoke-UnityTestSuites.ps1 -Label rc1
```

Unity licensing must already work for the Windows account running the command. Result XML goes under `TestResults/Release`; logs go under `Logs/Release`.

After every audit item passes, run Unity tests from a fresh clone/worktree and use these explicit build entry points:

- `BuildReleasePlayers.BuildWindows`
- `BuildReleasePlayers.BuildAndroid`
- `BuildReleasePlayers.BuildWebGL`
- `BuildReleasePlayers.BuildAll` only for a deliberate complete release build

Package all three completed player builds with the repository license, third-party notices, pinned Unity-package license texts, source link, patch notes, and SHA-256 manifest:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Release\New-ReleasePackage.ps1 -Version 2.2.1
```

The Windows and WebGL archives carry the legal bundle internally. The same bundle is emitted as a separate `Licenses.zip` companion for the Android APK and release page.

After the GitHub release and every advertised download are publicly available, update `release-channel-v1.json` on `main` with the published semantic version, short player-facing summary, release-page URL, and prerelease flag. Do this last: installed games read that small file once per day, and publishing it early would prompt players before the downloads exist. The runtime accepts only the official HTTPS GitHub release-page path and ignores invalid or unreachable channel data without blocking startup.
