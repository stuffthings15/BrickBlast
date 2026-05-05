Add-Type -AssemblyName System.Drawing, System.Drawing.Drawing2D

$root   = Split-Path $PSScriptRoot -Parent
$ssDir  = Join-Path $root "Docs\Screenshots"
if (-not (Test-Path $ssDir)) { New-Item -ItemType Directory -Path $ssDir | Out-Null }

# ───────────── palette ─────────────────────────────────────────────────────
$C_BG1   = [System.Drawing.Color]::FromArgb(255,  8, 10, 22)
$C_BG2   = [System.Drawing.Color]::FromArgb(255,  4, 15, 40)
$C_PANEL = [System.Drawing.Color]::FromArgb(200, 12, 18, 45)
$C_BORD  = [System.Drawing.Color]::FromArgb(120, 80,120,200)
$C_WHT   = [System.Drawing.Color]::FromArgb(240,240,255)
$C_GOLD  = [System.Drawing.Color]::FromArgb(255,230, 80)
$C_CYAN  = [System.Drawing.Color]::FromArgb(100,210,255)
$C_DIM   = [System.Drawing.Color]::FromArgb(130,140,170)
$C_GRN   = [System.Drawing.Color]::FromArgb( 80,230,120)
$C_RED   = [System.Drawing.Color]::FromArgb(255, 80, 80)
$C_ORG   = [System.Drawing.Color]::FromArgb(255,140, 40)
$C_PRP   = [System.Drawing.Color]::FromArgb(190, 80,255)

# ───────────── fonts ────────────────────────────────────────────────────────
$F10  = [System.Drawing.Font]::new("Segoe UI",10)
$F11  = [System.Drawing.Font]::new("Segoe UI",11)
$F12  = [System.Drawing.Font]::new("Segoe UI",12)
$F14  = [System.Drawing.Font]::new("Segoe UI",14)
$F14B = [System.Drawing.Font]::new("Segoe UI",14,[System.Drawing.FontStyle]::Bold)
$F16B = [System.Drawing.Font]::new("Segoe UI",16,[System.Drawing.FontStyle]::Bold)
$F18  = [System.Drawing.Font]::new("Segoe UI",18)
$F18B = [System.Drawing.Font]::new("Segoe UI",18,[System.Drawing.FontStyle]::Bold)
$F20B = [System.Drawing.Font]::new("Segoe UI",20,[System.Drawing.FontStyle]::Bold)
$F28B = [System.Drawing.Font]::new("Segoe UI",28,[System.Drawing.FontStyle]::Bold)
$F36B = [System.Drawing.Font]::new("Segoe UI",36,[System.Drawing.FontStyle]::Bold)
$F48B = [System.Drawing.Font]::new("Segoe UI",48,[System.Drawing.FontStyle]::Bold)

# ───────────── helpers ──────────────────────────────────────────────────────
function BgGrad([System.Drawing.Graphics]$g, $w, $h) {
    $lgb = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.Point]::new(0,0),[System.Drawing.Point]::new(0,$h),$C_BG1,$C_BG2)
    $g.FillRectangle($lgb,0,0,$w,$h); $lgb.Dispose()
}

function Stars([System.Drawing.Graphics]$g, $w, $h, $seed) {
    $rng = [Random]::new($seed)
    for ($i=0; $i -lt 200; $i++) {
        $x   = [float]($rng.NextDouble()*$w)
        $y   = [float]($rng.NextDouble()*$h)
        $sz  = [float]($rng.NextDouble()*2.6+0.4)
        $alp = $rng.Next(50,210)
        $br  = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb($alp,220,230,255))
        $g.FillEllipse($br,$x,$y,$sz,$sz); $br.Dispose()
    }
}

function RoundRect($x,$y,$pw,$ph,$r) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddArc([float]$x,[float]$y,[float]$r,[float]$r,180,90)
    $path.AddArc([float]($x+$pw-$r),[float]$y,[float]$r,[float]$r,270,90)
    $path.AddArc([float]($x+$pw-$r),[float]($y+$ph-$r),[float]$r,[float]$r,0,90)
    $path.AddArc([float]$x,[float]($y+$ph-$r),[float]$r,[float]$r,90,90)
    $path.CloseFigure(); return $path
}

function DrawPanel([System.Drawing.Graphics]$g, $x,$y,$pw,$ph) {
    $path = RoundRect $x $y $pw $ph 14
    $fill = [System.Drawing.SolidBrush]::new($C_PANEL); $g.FillPath($fill,$path); $fill.Dispose()
    $pen  = [System.Drawing.Pen]::new($C_BORD,1.5); $g.DrawPath($pen,$path); $pen.Dispose()
    $path.Dispose()
}

function CText([System.Drawing.Graphics]$g, $txt, $fnt, [System.Drawing.Color]$c, [float]$y) {
    $sz = $g.MeasureString($txt,$fnt)
    $br = [System.Drawing.SolidBrush]::new($c)
    $g.DrawString($txt,$fnt,$br,[float]((1200-$sz.Width)/2),[float]($y-$sz.Height/2)); $br.Dispose()
}

function LText([System.Drawing.Graphics]$g, $txt, $fnt, [System.Drawing.Color]$c, [float]$x,[float]$y) {
    $br = [System.Drawing.SolidBrush]::new($c); $g.DrawString($txt,$fnt,$br,$x,$y); $br.Dispose()
}

function BigTitle([System.Drawing.Graphics]$g, $line1, $line2) {
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    foreach ($pair in @(@($line1,140,48,$C_CYAN), @($line2,200,58,$C_GOLD))) {
        $text = $pair[0]; $ty = [float]$pair[1]; $sz = [float]$pair[2]
        $ff   = [System.Drawing.FontFamily]::new("Segoe UI")
        $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
        $path.AddString($text,$ff,[int][System.Drawing.FontStyle]::Bold,$sz,[System.Drawing.Point]::new(0,0),[System.Drawing.StringFormat]::GenericDefault)
        $b2   = $path.GetBounds()
        $mat  = [System.Drawing.Drawing2D.Matrix]::new()
        $mat.Translate([float]((1200-$b2.Width)/2-$b2.X),[float]($ty-$b2.Y))
        $path.Transform($mat)
        for ($gl=8; $gl -ge 2; $gl-=2) {
            $p = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(25,80,150,255),[float]$gl)
            $g.DrawPath($p,$path); $p.Dispose()
        }
        $lgb = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
            [System.Drawing.Point]::new(0,[int]$ty),[System.Drawing.Point]::new(0,[int]($ty+$sz)),
            $pair[2],$C_GOLD)
        $g.FillPath($lgb,$path); $lgb.Dispose(); $path.Dispose(); $ff.Dispose()
    }
}

function Button([System.Drawing.Graphics]$g, $label, $fnt, [System.Drawing.Color]$accent, [float]$cx,[float]$cy,[float]$bw,[float]$bh) {
    $x = $cx - $bw/2; $y = $cy - $bh/2
    $path = RoundRect $x $y $bw $bh 10
    $lgb  = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.Point]::new([int]$x,[int]$y),[System.Drawing.Point]::new([int]$x,[int]($y+$bh)),
        [System.Drawing.Color]::FromArgb(200,$accent.R,$accent.G,$accent.B),
        [System.Drawing.Color]::FromArgb(100,[int]($accent.R*0.4),[int]($accent.G*0.4),[int]($accent.B*0.4)))
    $g.FillPath($lgb,$path); $lgb.Dispose()
    $pen = [System.Drawing.Pen]::new($accent,1.5); $g.DrawPath($pen,$path); $pen.Dispose()
    $path.Dispose()
    CText $g $label $fnt $C_WHT $cy
}

function BrickRow([System.Drawing.Graphics]$g, [float]$startY, [int]$row, [int]$level, $colors) {
    $bw = 84; $bh = 26; $gx = 8; $left = 48
    for ($c=0; $c -lt 12; $c++) {
        $x = $left + $c*($bw+$gx); $y = $startY + $row*($bh+4)
        $idx = ($c + $row) % $colors.Count
        $col = $colors[$idx]
        $path = RoundRect $x $y $bw $bh 5
        $br   = [System.Drawing.SolidBrush]::new($col); $g.FillPath($br,$path); $br.Dispose()
        $pen  = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(80,255,255,255),1); $g.DrawPath($pen,$path); $pen.Dispose()
        $path.Dispose()
        # highlight
        $hlBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(50,255,255,255))
        $g.FillRectangle($hlBr,$x+4,$y+2,$bw-8,$bh/3); $hlBr.Dispose()
    }
}

function SaveBmp($bmp, $name) {
    $p = Join-Path $ssDir $name; $bmp.Save($p,[System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose(); Write-Host "  >> $name"
}

# ═══════════════════════════════════════════════════════════════════════════
# SS-01  Main Menu
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-01_main_menu.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 42

BigTitle $g "TEAM FAST TALK" "BRICK BLAST"

# Press SPACE button
Button $g "▶  Press SPACE to Start" $F18B $C_CYAN 600 318 340 44

# Mini leaderboard
DrawPanel $g 400 358 400 76
CText $g "BEST SCORES" $F14B $C_GOLD 375
LText $g "1.  CurtisG         128,450" $F12 $C_WHT 415 430
LText $g "2.  Player1          94,200" $F12 $C_DIM 415 395
LText $g "3.  GuestX           61,800" $F12 $C_DIM 415 375

CText $g ([char]0x25C6 + "  Press S for STORE  (4,200 coins)  " + [char]0x25C6) $F14B $C_GOLD 454
CText $g ([char]0x2699 + "  Press H for OPTIONS  |  C for CREDITS  " + [char]0x2699) $F14B $C_CYAN 486
CText $g "ARROW KEYS to move  |  F speed boost  |  P pause" $F11 $C_DIM 520
CText $g "Destroy bricks  •  Catch power-ups  •  Build combos!" $F11 $C_DIM 548
CText $g "BrickBlast: Velocity Market  |  v1.0.0" $F10 $C_DIM 590
CText $g "[F12] Export marketing assets  |  [C] Credits  |  [S] Store  |  [H] Settings" $F10 ([System.Drawing.Color]::FromArgb(55,70,90)) 610

$g.Dispose()
SaveBmp $bmp "SS-01_main_menu.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-02  Gameplay — Level 3
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-02_gameplay_level3.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 7

# HUD bar
$hudBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,5,8,28))
$g.FillRectangle($hudBr,0,0,1200,56); $hudBr.Dispose()
$hudLine = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(60,80,120,200),1)
$g.DrawLine($hudLine,0,56,1200,56); $hudLine.Dispose()

LText $g "SCORE" $F10 $C_DIM 20 8
LText $g "247,800" $F16B $C_WHT 14 22
LText $g "LEVEL 3" $F16B $C_GOLD 540 20
LText $g "COINS" $F10 $C_DIM 1088 8
LText $g "1,650" $F16B $C_GOLD 1082 22
# Lives
for ($li=0;$li -lt 3;$li++) {
    $lx = 900 + $li*26
    $br = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255,80,80))
    $g.FillEllipse($br,$lx,22,16,16); $br.Dispose()
}
LText $g "SYNC ✓" $F10 $C_GRN 1148 46

# Bricks — 7 rows, 5 brick types
$brickColors = @(
    [System.Drawing.Color]::FromArgb(255,60,80),   # red — standard
    [System.Drawing.Color]::FromArgb(60,180,255),  # blue — reinforced
    [System.Drawing.Color]::FromArgb(80,230,120),  # green — standard
    [System.Drawing.Color]::FromArgb(255,190,40),  # amber — explosive
    [System.Drawing.Color]::FromArgb(180,80,255)   # purple — ghost
)
for ($row=0;$row -lt 7;$row++) { BrickRow $g 72 $row 3 $brickColors }

# Paddle (cyan glow)
$paddleY = 867-50-14; $paddleX = 440; $paddleW = 320
$pPath = RoundRect $paddleX $paddleY $paddleW 14 7
$pLgb  = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
    [System.Drawing.Point]::new(0,$paddleY),[System.Drawing.Point]::new(0,$paddleY+14),
    [System.Drawing.Color]::FromArgb(200,100,210,255),
    [System.Drawing.Color]::FromArgb(150,40,100,200))
$g.FillPath($pLgb,$pPath); $pLgb.Dispose()
$pPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(200,100,210,255),1.5)
$g.DrawPath($pPen,$pPath); $pPen.Dispose(); $pPath.Dispose()

# Ball in motion
$ballBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255,255,255))
$g.FillEllipse($ballBr,587,380,16,16); $ballBr.Dispose()
# Motion trail
for ($t=1;$t -le 5;$t++) {
    $ta = 50-$t*8; $tx = 587-$t*7; $ty = 380+$t*9; $tsz = 16-$t*1.5
    $trBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb($ta,200,220,255))
    $g.FillEllipse($trBr,$tx+$tsz/2,$ty+$tsz/2,$tsz,$tsz); $trBr.Dispose()
}

# Power-up falling
$puBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(200,100,210,255))
$puPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,180,100,255),2)
$g.FillEllipse($puBr,660,520,24,24); $g.DrawEllipse($puPen,658,518,28,28)
$puBr.Dispose(); $puPen.Dispose()
LText $g "W" $F12 $C_WHT 666 523

$g.Dispose()
SaveBmp $bmp "SS-02_gameplay_level3.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-03  Store — Balls tab
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-03_store_balls.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 13

DrawPanel $g 30 20 1140 820

CText $g "VELOCITY MARKET" $F36B $C_GOLD 80
CText $g "Balance: 4,200 coins" $F16B $C_CYAN 120

# Tabs
$tabs = @("BALLS","BRICKS","BONUSES")
$tabX = @(200,490,780); $tabW = 220; $tabH = 40
for ($t=0;$t -lt 3;$t++) {
    $tx = $tabX[$t]; $ty = 145
    $col = if ($t -eq 0) { $C_CYAN } else { $C_DIM }
    $path = RoundRect $tx $ty $tabW $tabH 8
    if ($t -eq 0) {
        $tbr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,0,80,140)); $g.FillPath($tbr,$path); $tbr.Dispose()
        $tpen = [System.Drawing.Pen]::new($C_CYAN,2); $g.DrawPath($tpen,$path); $tpen.Dispose()
    }
    $path.Dispose()
    CText $g $tabs[$t] $F14B $col ($ty+20)
}

# Ball items grid
$balls = @(
    @("Classic Ball","Equipped","Default","",  $C_WHT),
    @("Neon Streak", "Owned",   "2,000 c","",  $C_CYAN),
    @("Inferno",     "Owned",   "3,500 c","",  $C_RED),
    @("Galaxy Orb",  "Buy",     "4,000 c","",  $C_PRP),
    @("Shadow Void", "Buy",     "5,000 c","",  [System.Drawing.Color]::FromArgb(80,80,120)),
    @("Pixel Block",  "Buy",    "6,000 c","",  $C_GOLD),
    @("Soul Crystal","Buy",     "7,500 c","",  [System.Drawing.Color]::FromArgb(120,255,200)),
    @("Virus Core",  "Buy",     "9,000 c","",  [System.Drawing.Color]::FromArgb(100,240,60))
)
$cols2 = 4; $iw = 240; $ih = 130; $ipadX = 40; $ipadY = 200
for ($i=0;$i -lt $balls.Count;$i++) {
    $col = $i % $cols2; $row = [int]($i / $cols2)
    $ix = $ipadX + $col*($iw+20); $iy = $ipadY + $row*($ih+14)
    $bc = $balls[$i][4]
    $sel = ($i -lt 2)
    $bp = RoundRect $ix $iy $iw $ih 10
    $bbr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(160,16,22,55)); $g.FillPath($bbr,$bp); $bbr.Dispose()
    $bpen = [System.Drawing.Pen]::new((if($sel){$C_CYAN}else{$C_BORD}),(if($sel){2.5}else{1})); $g.DrawPath($bpen,$bp); $bpen.Dispose()
    $bp.Dispose()
    # ball icon
    $ballCol = [System.Drawing.Color]::FromArgb(200,$bc.R,$bc.G,$bc.B)
    $bbr2 = [System.Drawing.SolidBrush]::new($ballCol)
    $g.FillEllipse($bbr2,$ix+$iw/2-20,$iy+12,40,40); $bbr2.Dispose()
    # Equipped badge
    if ($balls[$i][1] -eq "Equipped") {
        $ebr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(200,0,120,60)); $g.FillRectangle($ebr,$ix,$iy,$iw,20); $ebr.Dispose()
        CText $g "EQUIPPED" $F10 $C_GRN ($iy+10)
    }
    LText $g $balls[$i][0] $F12 $C_WHT ($ix+8) ($iy+58)
    $stateC = switch ($balls[$i][1]) { "Equipped" { $C_GRN } "Owned" { $C_CYAN } default { $C_GOLD } }
    LText $g $balls[$i][2] $F10 $stateC ($ix+8) ($iy+78)
    if ($balls[$i][2] -ne "Default") { LText $g $balls[$i][2] $F11 $C_GOLD ($ix+8) ($iy+96) }
}

CText $g "TAB/← → to switch categories  |  ↑↓ to browse  |  ENTER to buy/equip  |  ESC back" $F11 $C_DIM 840

$g.Dispose()
SaveBmp $bmp "SS-03_store_balls.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-04  Store — Bonuses tab
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-04_store_bonuses.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 99

DrawPanel $g 30 20 1140 820
CText $g "VELOCITY MARKET" $F36B $C_GOLD 80
CText $g "Balance: 4,200 coins" $F16B $C_CYAN 120

# Tabs — Bonuses selected
$tabs2 = @("BALLS","BRICKS","BONUSES")
$tabX2 = @(200,490,780)
for ($t=0;$t -lt 3;$t++) {
    $tx = $tabX2[$t]; $ty = 145; $col = if ($t -eq 2) { $C_PRP } else { $C_DIM }
    $path = RoundRect $tx $ty 220 40 8
    if ($t -eq 2) {
        $tbr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,80,0,140)); $g.FillPath($tbr,$path); $tbr.Dispose()
        $tpen = [System.Drawing.Pen]::new($C_PRP,2); $g.DrawPath($tpen,$path); $tpen.Dispose()
    }
    $path.Dispose()
    CText $g $tabs2[$t] $F14B $col ($ty+20)
}

# Bonus pack grid — 4 cols x 4 rows = 16
$bonuses = @(
    @("Classic Pack",  "Equipped","Default", $C_CYAN),
    @("Lava Pack",     "Owned",   "2,500 c", $C_RED),
    @("Thunder Pack",  "Owned",   "3,000 c", $C_GOLD),
    @("Cosmic Pack",   "Buy",     "3,500 c", $C_PRP),
    @("Nature Pack",   "Buy",     "3,500 c", $C_GRN),
    @("Ice Pack",      "Buy",     "4,000 c", [System.Drawing.Color]::FromArgb(160,230,255)),
    @("Shadow Pack",   "Buy",     "4,500 c", [System.Drawing.Color]::FromArgb(90,80,140)),
    @("Virus Pack",    "Buy",     "5,000 c", [System.Drawing.Color]::FromArgb(140,255,40)),
    @("Crystal Pack",  "Buy",     "5,500 c", [System.Drawing.Color]::FromArgb(180,240,200)),
    @("Inferno Pack",  "Buy",     "6,000 c", [System.Drawing.Color]::FromArgb(255,120,30)),
    @("Ocean Pack",    "Buy",     "6,000 c", [System.Drawing.Color]::FromArgb(30,130,220)),
    @("Galaxy Pack",   "Buy",     "7,000 c", [System.Drawing.Color]::FromArgb(140,80,255)),
    @("Horror Pack",   "Buy",     "7,500 c", [System.Drawing.Color]::FromArgb(180,30,30)),
    @("Pixel Pack",    "Buy",     "8,000 c", [System.Drawing.Color]::FromArgb(255,220,30)),
    @("Hologram Pack", "Buy",     "9,000 c", [System.Drawing.Color]::FromArgb(80,255,220)),
    @("Void Pack",     "Buy",     "12,000 c",[System.Drawing.Color]::FromArgb(20,20,40))
)
$icons = @([char]0x2605,[char]0x2665,[char]0x26A1,[char]0x2734,[char]0x2618,[char]0x2745,[char]0x25A0,[char]0x2622,
           [char]0x25CA,[char]0x2600,[char]0x2693,[char]0x2B50,[char]0x2620,[char]0x25FC,[char]0x25C6,[char]0x25CF)
$cols4=4; $iw4=250; $ih4=110
for ($i=0;$i -lt $bonuses.Count;$i++) {
    $col=$i%$cols4; $row=[int]($i/$cols4)
    $ix = 50+$col*($iw4+18); $iy = 200+$row*($ih4+12)
    $bc = $bonuses[$i][3]
    $bp = RoundRect $ix $iy $iw4 $ih4 10
    $bbr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(160,16,22,55)); $g.FillPath($bbr,$bp); $bbr.Dispose()
    $sel2 = ($bonuses[$i][1] -eq "Equipped")
    $bpen = [System.Drawing.Pen]::new((if($sel2){$C_PRP}else{$C_BORD}),(if($sel2){2.5}else{1})); $g.DrawPath($bpen,$bp); $bpen.Dispose()
    $bp.Dispose()
    if ($sel2) {
        $ebr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,60,0,100)); $g.FillRectangle($ebr,$ix,$iy,$iw4,20); $ebr.Dispose()
        CText $g "EQUIPPED" $F10 $C_PRP ($iy+10)
    }
    # icon circle
    $icBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,$bc.R,$bc.G,$bc.B))
    $g.FillEllipse($icBr,$ix+12,$iy+22,56,56); $icBr.Dispose()
    $icPen = [System.Drawing.Pen]::new($bc,1.5); $g.DrawEllipse($icPen,$ix+12,$iy+22,56,56); $icPen.Dispose()
    $icFnt = [System.Drawing.Font]::new("Segoe UI Symbol",18)
    $icBr2 = [System.Drawing.SolidBrush]::new($C_WHT); $g.DrawString($icons[$i],$icFnt,$icBr2,$ix+22,$iy+30); $icBr2.Dispose(); $icFnt.Dispose()
    LText $g $bonuses[$i][0] $F11 $C_WHT ($ix+76) ($iy+24)
    $sc = if ($bonuses[$i][1] -eq "Equipped"){$C_PRP}elseif($bonuses[$i][1] -eq "Owned"){$C_CYAN}else{$C_GOLD}
    LText $g $bonuses[$i][2] $F10 $sc ($ix+76) ($iy+48)
}

CText $g "TAB/← → to switch categories  |  ↑↓ to browse  |  ENTER to equip  |  ESC back" $F11 $C_DIM 840

$g.Dispose()
SaveBmp $bmp "SS-04_store_bonuses.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-05  Game Over
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-05_game_over.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 55

# Dim overlay
$dimBr = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(160,0,0,10))
$g.FillRectangle($dimBr,0,0,1200,867); $dimBr.Dispose()

DrawPanel $g 300 160 600 540

# GAME OVER title
$ff = [System.Drawing.FontFamily]::new("Segoe UI")
$goPath = [System.Drawing.Drawing2D.GraphicsPath]::new()
$goPath.AddString("GAME OVER",$ff,[int][System.Drawing.FontStyle]::Bold,72,[System.Drawing.Point]::new(0,0),[System.Drawing.StringFormat]::GenericDefault)
$gob = $goPath.GetBounds()
$goMat = [System.Drawing.Drawing2D.Matrix]::new(); $goMat.Translate([float]((1200-$gob.Width)/2-$gob.X),[float](195-$gob.Y)); $goPath.Transform($goMat)
for ($gl=12;$gl -ge 2;$gl-=2) { $gp=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(30,220,30,30),[float]$gl); $g.DrawPath($gp,$goPath); $gp.Dispose() }
$goLgb=[System.Drawing.Drawing2D.LinearGradientBrush]::new([System.Drawing.Point]::new(0,195),[System.Drawing.Point]::new(0,267),[System.Drawing.Color]::FromArgb(255,100,80),[System.Drawing.Color]::FromArgb(200,50,50))
$g.FillPath($goLgb,$goPath); $goLgb.Dispose(); $goPath.Dispose(); $ff.Dispose()

CText $g "Player: CurtisG" $F16B $C_CYAN 300
CText $g "Level 3  —  Score: 247,800" $F18B $C_WHT 340
CText $g "+1,240 coins earned this run" $F14B $C_GOLD 380

# Separator
$sepPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(60,100,140,200),1)
$g.DrawLine($sepPen,360,406,840,406); $sepPen.Dispose()

# Buttons
Button $g "[ R ]  RETRY" $F18B $C_CYAN 600 450 280 52
Button $g "[ S ]  STORE" $F18B $C_PRP 600 516 280 52
Button $g "[ M ]  MAIN MENU" $F16B $C_DIM 600 580 280 44

CText $g "R = Retry  |  S = Store  |  Esc / M = Menu" $F11 $C_DIM 640

$g.Dispose()
SaveBmp $bmp "SS-05_game_over.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-06  Level Complete
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-06_level_complete.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 17

# Particles / celebration
$rng2 = [Random]::new(88)
for ($p=0;$p -lt 80;$p++) {
    $px=[float]($rng2.NextDouble()*1200); $py=[float]($rng2.NextDouble()*867)
    $ps=[float]($rng2.NextDouble()*10+4)
    $pr=$rng2.Next(0,360); $pc=[System.Drawing.Color]::FromArgb(180,$rng2.Next(100,255),$rng2.Next(100,255),$rng2.Next(50,255))
    $pbr=[System.Drawing.SolidBrush]::new($pc); $g.FillEllipse($pbr,$px,$py,$ps,$ps); $pbr.Dispose()
}

DrawPanel $g 250 140 700 560

# LEVEL COMPLETE
$ff2=[System.Drawing.FontFamily]::new("Segoe UI")
$lcPath=[System.Drawing.Drawing2D.GraphicsPath]::new()
$lcPath.AddString("LEVEL COMPLETE",$ff2,[int][System.Drawing.FontStyle]::Bold,52,[System.Drawing.Point]::new(0,0),[System.Drawing.StringFormat]::GenericDefault)
$lcb=$lcPath.GetBounds()
$lcMat=[System.Drawing.Drawing2D.Matrix]::new(); $lcMat.Translate([float]((1200-$lcb.Width)/2-$lcb.X),[float](168-$lcb.Y)); $lcPath.Transform($lcMat)
for ($gl=10;$gl -ge 2;$gl-=2){$lcp=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(30,80,200,80),[float]$gl);$g.DrawPath($lcp,$lcPath);$lcp.Dispose()}
$lcLgb=[System.Drawing.Drawing2D.LinearGradientBrush]::new([System.Drawing.Point]::new(0,168),[System.Drawing.Point]::new(0,220),[System.Drawing.Color]::FromArgb(120,255,100),[System.Drawing.Color]::FromArgb(255,230,80))
$g.FillPath($lcLgb,$lcPath); $lcLgb.Dispose(); $lcPath.Dispose(); $ff2.Dispose()

CText $g "Level 3 Complete!" $F18B $C_WHT 258

# Score tally
$tallies = @(
    @("Base Score",  "200,000"),
    @("Combo Bonus", " +28,400"),
    @("Speed Bonus", " +19,400"),
    @("Total Score", "247,800")
)
$ty2=300
foreach ($row in $tallies) {
    $sep = ($row[0] -eq "Total Score")
    if ($sep) {
        $g.DrawLine([System.Drawing.Pen]::new($C_DIM,1),340,$ty2,860,$ty2); $ty2+=12
    }
    $lbr=[System.Drawing.SolidBrush]::new($C_DIM); $g.DrawString($row[0],$F14,$lbr,340,$ty2); $lbr.Dispose()
    $sz2=$g.MeasureString($row[1],$F14B)
    $vc = if($sep){$C_GOLD}else{$C_WHT}
    $vbr=[System.Drawing.SolidBrush]::new($vc); $g.DrawString($row[1],$F14B,$vbr,860-$sz2.Width,$ty2); $vbr.Dispose()
    $ty2+=36
}

CText $g "+1,240 coins" $F16B $C_GOLD 500
Button $g "NEXT LEVEL  ▶" $F18B $C_GRN 600 590 280 52
CText $g "SPACE / Enter = Next Level  |  M = Menu" $F11 $C_DIM 640

$g.Dispose()
SaveBmp $bmp "SS-06_level_complete.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-07  Credits
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-07_credits.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 31

DrawPanel $g 140 60 920 750

CText $g "CREDITS" $F48B $C_GOLD 110

$sections = @(
    @("TEAM FAST TALK",""),
    @("",""),
    @("Lead Developer","CurtisG"),
    @("Game Design","Team Fast Talk"),
    @("UI / Art","Team Fast Talk"),
    @("Audio Direction","Team Fast Talk"),
    @("QA / Testing","Team Fast Talk"),
    @("",""),
    @("TECHNOLOGY",""),
    @("",""),
    @("Runtime",".NET 10 / VB.NET WinForms"),
    @("Rendering","GDI+ (System.Drawing)"),
    @("Persistence","System.Text.Json"),
    @("Audio","Win32 PlaySound / MCI"),
    @("Controller","XInput"),
    @("Networking","System.Net.Http"),
    @("",""),
    @("COURSE","CS-120  |  Spring 2025"),
    @("Starter Source","CS-120 Semester Project Template"),
    @("",""),
    @("Press ESC or ENTER to return to menu","")
)
$cy2=168
foreach ($row in $sections) {
    if ($row[0] -eq "" -and $row[1] -eq "") { $cy2+=14; continue }
    $isHead = ($row[1] -eq "")
    if ($isHead) {
        CText $g $row[0] $F16B $C_CYAN $cy2; $cy2+=30
    } else {
        $lbr=[System.Drawing.SolidBrush]::new($C_DIM); $g.DrawString($row[0],$F12,$lbr,220,$cy2-8); $lbr.Dispose()
        $vbr=[System.Drawing.SolidBrush]::new($C_WHT); $g.DrawString($row[1],$F12,$vbr,600,$cy2-8); $vbr.Dispose()
        $cy2+=24
    }
}

$g.Dispose()
SaveBmp $bmp "SS-07_credits.png"

# ═══════════════════════════════════════════════════════════════════════════
# SS-08  Settings / Sync
# ═══════════════════════════════════════════════════════════════════════════
Write-Host "Generating SS-08_settings_sync.png..."
$bmp = [System.Drawing.Bitmap]::new(1200,867,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

BgGrad $g 1200 867
Stars  $g 1200 867 63

DrawPanel $g 200 60 800 740

CText $g ([char]0x2699 + " OPTIONS") $F36B $C_CYAN 108

$opts = @(
    @("Music Style",    "Retro Chiptune"),
    @("SFX Style",      "Arcade Classic"),
    @("Window Scale",   "1200×867 (Native)"),
    @("Color Blind",    "Off"),
    @("Difficulty",     "Normal"),
    @("Particle FX",    "On"),
    @("Ball Trail",     "On"),
    @("Sync Profile",   "")
)
$oy=168
foreach ($opt in $opts) {
    $isSyncRow = ($opt[0] -eq "Sync Profile")
    $lbr=[System.Drawing.SolidBrush]::new($C_DIM); $g.DrawString($opt[0],$F14,$lbr,250,$oy); $lbr.Dispose()
    if ($isSyncRow) {
        # Sync status box
        $spath = RoundRect 640 $oy 300 34 8
        $sbr=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(160,0,80,30)); $g.FillPath($sbr,$spath); $sbr.Dispose()
        $spen=[System.Drawing.Pen]::new($C_GRN,1.5); $g.DrawPath($spen,$spath); $spen.Dispose(); $spath.Dispose()
        $vbr=[System.Drawing.SolidBrush]::new($C_GRN); $g.DrawString([char]0x2714 + " Synced  —  Last: 2 min ago",$F12,$vbr,648,$oy+7); $vbr.Dispose()
        $oy+=50
        # Manual sync button
        Button $g "SYNC NOW" $F14B $C_GRN 600 ($oy+20) 180 36
        $oy+=62
    } else {
        $vbr=[System.Drawing.SolidBrush]::new($C_WHT); $g.DrawString($opt[1],$F14,$vbr,640,$oy); $vbr.Dispose()
        $oy+=44
    }
}

CText $g "↑↓ navigate  |  ← → change value  |  ESC back to menu" $F11 $C_DIM 792

$g.Dispose()
SaveBmp $bmp "SS-08_settings_sync.png"

Write-Host ""; Write-Host "All 8 screenshots generated in $ssDir"
