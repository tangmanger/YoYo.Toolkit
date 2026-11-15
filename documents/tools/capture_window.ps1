# 真实桌面截图：验证 DWM 圆角与窗口阴影（RenderTargetBitmap 抓不到系统合成层）。
# 用法：powershell -File capture_window.ps1 -Exe <exe> -Out <png> -ExtraArgs "--front 14000" -Delay 6000
param(
    [string]$Exe,
    [string]$Out,
    [string]$ExtraArgs = "",
    [int]$Delay = 5000
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$process = Start-Process -FilePath $Exe -ArgumentList $ExtraArgs -PassThru

Start-Sleep -Milliseconds $Delay

$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$bitmap.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()

$process.Refresh()
Write-Output "saved $Out  $($bounds.Width)x$($bounds.Height)  title=$($process.MainWindowTitle)"

if (-not $process.HasExited) { $process.Kill() }
