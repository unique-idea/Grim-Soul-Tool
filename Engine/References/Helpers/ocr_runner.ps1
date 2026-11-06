param([string]$ImagePath)
$output = @{ Name = ""; CurrentHp = 0; MaxHp = 0; Level = 0; RawText = "" }
if (-not (Test-Path $ImagePath)) { $output | ConvertTo-Json -Compress; exit 0 }

Add-Type -AssemblyName System.Runtime.WindowsRuntime
[Windows.Graphics.Imaging.BitmapDecoder, Windows.Graphics, ContentType = WindowsRuntime] | Out-Null
[Windows.Graphics.Imaging.SoftwareBitmap, Windows.Graphics, ContentType = WindowsRuntime] | Out-Null
[Windows.Storage.Streams.IRandomAccessStream, Windows.Storage.Streams, ContentType = WindowsRuntime] | Out-Null
[Windows.Media.Ocr.OcrEngine, Windows.Media.Ocr, ContentType = WindowsRuntime] | Out-Null
[Windows.Media.Ocr.OcrResult, Windows.Media.Ocr, ContentType = WindowsRuntime] | Out-Null

function Await-WinRT($op, $type) {
    $m = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { 
        $_.Name -eq 'AsTask' -and $_.IsGenericMethod -and $_.GetParameters().Count -eq 1 
    } | Select-Object -First 1
    $gm = $m.MakeGenericMethod($type)
    return $gm.Invoke($null, @($op)).GetAwaiter().GetResult()
}

try {
    $bytes = [System.IO.File]::ReadAllBytes($ImagePath)
    $ms = New-Object System.IO.MemoryStream(,$bytes)
    $ras = [System.IO.WindowsRuntimeStreamExtensions]::AsRandomAccessStream($ms)

    $opDecoder = [Windows.Graphics.Imaging.BitmapDecoder]::CreateAsync($ras)
    $decoder = Await-WinRT $opDecoder ([Windows.Graphics.Imaging.BitmapDecoder])

    $opSb = $decoder.GetSoftwareBitmapAsync()
    $sb = Await-WinRT $opSb ([Windows.Graphics.Imaging.SoftwareBitmap])

    $engine = [Windows.Media.Ocr.OcrEngine]::TryCreateFromUserProfileLanguages()
    $opResult = $engine.RecognizeAsync($sb)
    $result = Await-WinRT $opResult ([Windows.Media.Ocr.OcrResult])

    $rawLines = @()
    foreach ($l in $result.Lines) { $rawLines += $l.Text }
    $output.RawText = ($rawLines -join "`n")

    foreach ($line in $rawLines) {
        # HP: "90/102", "90 / 102", "91) 102", "90 102"
        if ($line -match '(\d{1,4})\s*[/\\|\)\]\}IltT:\-]\s*(\d{1,4})') {
            $output.CurrentHp = [int]$matches[1]
            $output.MaxHp = [int]$matches[2]
        } elseif ($line -match '(\d{1,4})\s+(\d{2,4})' -and $line -notmatch 'Lvl|Level') {
            $output.CurrentHp = [int]$matches[1]
            $output.MaxHp = [int]$matches[2]
        }

        # Level: "41 [.vt.", "41 [Nt-", "41 Lvl", "41 Lv"
        if ($line -match '(\d{1,3})\s*(?:\[[A-Za-z0-9\.\-]+|Lvl|Lv|Level|lvl|\.vt\.)') {
            $output.Level = [int]$matches[1]
        }

        # Name: Alphabetic character string
        if ($line -match '^[A-Za-z\s]{3,}$' -and $output.Name -eq "") {
            $output.Name = $line.Trim()
        }
    }
} catch {
}

$output | ConvertTo-Json -Compress
