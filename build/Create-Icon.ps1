param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\Loggin.Desktop\Assets\loggin.ico')
)

Add-Type -AssemblyName System.Drawing

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$images = [System.Collections.Generic.List[byte[]]]::new()

foreach ($size in $sizes) {
    $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $padding = [Math]::Max(1, [int]($size * 0.03))
    $radius = [int]($size * 0.22)
    $rect = [System.Drawing.RectangleF]::new($padding, $padding, $size - 2 * $padding, $size - 2 * $padding)
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = $radius * 2
    $path.AddArc($rect.X, $rect.Y, $diameter, $diameter, 180, 90)
    $path.AddArc($rect.Right - $diameter, $rect.Y, $diameter, $diameter, 270, 90)
    $path.AddArc($rect.Right - $diameter, $rect.Bottom - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    $gradient = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        $rect, [System.Drawing.Color]::FromArgb(120, 106, 248),
        [System.Drawing.Color]::FromArgb(74, 58, 208), 45)
    $graphics.FillPath($gradient, $path)

    $lineWidth = [Math]::Max(2, [single]($size * 0.075))
    $pen = [System.Drawing.Pen]::new([System.Drawing.Color]::White, $lineWidth)
    $pen.StartCap = $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $x = [single]($size * 0.24)
    foreach ($line in @(@(.30, .52), @(.50, .68), @(.70, .58))) {
        $y = [single]($size * $line[0])
        $graphics.DrawLine($pen, $x, $y, [single]($size * $line[1]), $y)
    }
    $graphics.FillEllipse([System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(32, 199, 183)),
        [single]($size * .68), [single]($size * .22), [single]($size * .16), [single]($size * .16))
    $graphics.FillEllipse([System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(245, 158, 11)),
        [single]($size * .72), [single]($size * .62), [single]($size * .16), [single]($size * .16))

    $stream = [System.IO.MemoryStream]::new()
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $images.Add($stream.ToArray())
    $stream.Dispose()
    $pen.Dispose()
    $gradient.Dispose()
    $path.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}

$outputDirectory = Split-Path -Parent $OutputPath
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$file = [System.IO.File]::Create($OutputPath)
$writer = [System.IO.BinaryWriter]::new($file)
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$sizes.Count)
$offset = 6 + (16 * $sizes.Count)
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $dimension = if ($sizes[$i] -eq 256) { 0 } else { $sizes[$i] }
    $writer.Write([byte]$dimension)
    $writer.Write([byte]$dimension)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]32)
    $writer.Write([uint32]$images[$i].Length)
    $writer.Write([uint32]$offset)
    $offset += $images[$i].Length
}
foreach ($image in $images) { $writer.Write($image) }
$writer.Dispose()
$file.Dispose()

Write-Output "Icon created: $OutputPath"
