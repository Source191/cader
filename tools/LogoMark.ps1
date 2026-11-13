<#
.SYNOPSIS
The mark (two worktree branches curving from one node into a C, with a star in its opening, on a rounded tile), drawn once for every script that needs it.
.DESCRIPTION
Dot-source it: New-Icon.ps1 draws the icon with it, New-Header.ps1 draws the mark on the header images, and New-IconPixels.ps1
draws it without its tile (-NoTile) for the console's black background, where the dark tile disappeared. The mark is laid out on a
512 px square and scaled to the size asked for, so every image carries the same drawing.
#>

Add-Type -AssemblyName System.Drawing

function ColourOf([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
function PointOf([double]$x, [double]$y) { [System.Drawing.PointF]::new($x, $y) }

function New-RoundedRectangle([float]$size, [float]$radius) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = $radius * 2
    $path.AddArc(0, 0, $diameter, $diameter, 180, 90)
    $path.AddArc($size - $diameter, 0, $diameter, $diameter, 270, 90)
    $path.AddArc($size - $diameter, $size - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc(0, $size - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    $path
}

function New-Pen([string]$hex, [float]$width) {
    $pen = [System.Drawing.Pen]::new((ColourOf $hex), $width)
    $pen.StartCap = 'Round'
    $pen.EndCap = 'Round'
    $pen
}

function Add-Node([System.Drawing.Graphics]$graphics, [float]$x, [float]$y, [float]$radius, [string]$ring, [string]$centre = '#0F172A') {
    $graphics.FillEllipse([System.Drawing.SolidBrush]::new((ColourOf $centre)), $x - $radius, $y - $radius, 2 * $radius, 2 * $radius)
    $inner = $radius * 0.78
    $graphics.DrawEllipse((New-Pen $ring ($radius * 0.55)), $x - $inner, $y - $inner, 2 * $inner, 2 * $inner)
}

function Add-Star([System.Drawing.Graphics]$graphics, [float]$x, [float]$y, [float]$radius, [string]$hex) {
    # A four-pointed star: long points up, down, left and right, pinched between them.
    $points = for ($index = 0; $index -lt 8; $index++) {
        $angle = [Math]::PI / 4 * $index - [Math]::PI / 2
        $length = ($index % 2 -eq 0) ? $radius : $radius * 0.32
        PointOf ($x + $length * [Math]::Cos($angle)) ($y + $length * [Math]::Sin($angle))
    }
    $graphics.FillPolygon([System.Drawing.SolidBrush]::new((ColourOf $hex)), [System.Drawing.PointF[]]$points)
}

function Add-LogoMark([System.Drawing.Graphics]$graphics, [float]$x, [float]$y, [float]$size, [switch]$NoTile, [string]$NodeCentre = '#0F172A', [string]$Outline) {
    $layout = 512
    $state = $graphics.Save()
    $graphics.TranslateTransform($x, $y)
    $graphics.ScaleTransform($size / $layout, $size / $layout)
    if (-not $NoTile) {
        $tile = New-RoundedRectangle $layout 96
        $graphics.SetClip($tile)
        $background = [System.Drawing.Drawing2D.LinearGradientBrush]::new((PointOf 0 0), (PointOf 0 $layout), (ColourOf '#1E1B4B'), (ColourOf '#0F172A'))
        $graphics.FillRectangle($background, 0, 0, $layout, $layout)
        if ($Outline) {
            # An edge that sets the dark tile off a black background, drawn on a path inset by half its width so the whole
            # stroke lies inside the tile and the four sides come out alike (a stroke on the tile's own edge was clipped unevenly).
            $width = 28
            $inset = New-RoundedRectangle ($layout - $width) (96 - $width / 2)
            $shift = [System.Drawing.Drawing2D.Matrix]::new()
            $shift.Translate($width / 2, $width / 2)
            $inset.Transform($shift)
            $graphics.DrawPath([System.Drawing.Pen]::new((ColourOf $Outline), $width), $inset)
        }
    }

    # A C: from one node on the left, a teal branch curves up and round to the top right and a purple one down to the bottom
    # right, both on one circle; the star sits in the opening, for Cadair Idris, where the giant sat to watch the stars.
    $graphics.DrawArc((New-Pen '#2DD4BF' 40), 104, 116, 280, 280, 180, 125)
    $graphics.DrawArc((New-Pen '#A78BFA' 40), 104, 116, 280, 280, 55, 125)
    Add-Node $graphics 104 256 46 '#FFFFFF' $NodeCentre
    Add-Node $graphics 324 141 46 '#2DD4BF' $NodeCentre
    Add-Node $graphics 324 371 46 '#A78BFA' $NodeCentre
    Add-Star $graphics 372 256 54 '#FFFFFF'
    $graphics.Restore($state)
}
