param(
    [string]$Exe,
    [string]$Out
)

# Extract the icon associated with the exe to confirm ApplicationIcon made it into the binary.
Add-Type -AssemblyName System.Drawing

$icon = [System.Drawing.Icon]::ExtractAssociatedIcon($Exe)
$bitmap = $icon.ToBitmap()
$bitmap.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap.Dispose()

Write-Output "saved $Out  $($icon.Width)x$($icon.Height)"
