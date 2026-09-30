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
