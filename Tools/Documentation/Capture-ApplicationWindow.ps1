param(
	[string]$Executable,
	[string[]]$Arguments = @(),
	[Parameter(Mandatory = $true)][string]$OutputPath,
	[int]$WaitSeconds = 12,
	[int]$ExistingProcessId = 0,
	[switch]$KeepOpen
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class DocumentationWindowCapture
{
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr handle, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr handle, IntPtr target, uint flags);
    [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr handle, int command);
}
'@

$process = if ($ExistingProcessId -gt 0) { Get-Process -Id $ExistingProcessId } else {
	if ([string]::IsNullOrWhiteSpace($Executable)) { throw 'Executable is required when ExistingProcessId is not supplied.' }
	Start-Process -FilePath $Executable -ArgumentList $Arguments -PassThru
}
$deadline = [DateTime]::UtcNow.AddSeconds($WaitSeconds)
$handle = [IntPtr]::Zero
while ([DateTime]::UtcNow -lt $deadline) {
	Start-Sleep -Milliseconds 500
	if (-not $process.HasExited) {
		$process.Refresh()
		$handle = $process.MainWindowHandle
		if ($handle -ne [IntPtr]::Zero) { break }
	}
}
if ($handle -eq [IntPtr]::Zero) { throw "The application did not expose a capturable window within $WaitSeconds seconds." }

[DocumentationWindowCapture]::ShowWindowAsync($handle, 3) | Out-Null
Start-Sleep -Seconds 2
$rect = New-Object DocumentationWindowCapture+Rect
if (-not [DocumentationWindowCapture]::GetWindowRect($handle, [ref]$rect)) { throw 'Could not read the application window bounds.' }
$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
if ($width -lt 100 -or $height -lt 100) { throw "Unexpected window size: ${width}x${height}." }

$absoluteOutput = [IO.Path]::GetFullPath($OutputPath)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($absoluteOutput)) | Out-Null
$bitmap = New-Object Drawing.Bitmap $width, $height
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$deviceContext = $graphics.GetHdc()
try {
	if (-not [DocumentationWindowCapture]::PrintWindow($handle, $deviceContext, 2)) { throw 'The application refused the window capture.' }
}
finally {
	$graphics.ReleaseHdc($deviceContext)
	$graphics.Dispose()
}
$bitmap.Save($absoluteOutput, [Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()

if (-not $KeepOpen -and -not $process.HasExited) { $process.CloseMainWindow() | Out-Null }
Write-Host "Captured $absoluteOutput (${width}x${height})."
