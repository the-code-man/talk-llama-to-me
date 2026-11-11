$filePath = Join-Path (Get-Location) "test_audio.wav"
Write-Host "Generating audible test file at: $filePath" -ForegroundColor Cyan

$sampleRate = 16000
$bitsPerSample = 16
$channels = 1
$durationSeconds = 2
$frequency = 440.0 # A4 note tone

$dataSize = $sampleRate * ($bitsPerSample / 8) * $channels * $durationSeconds
$fileSize = 36 + $dataSize

$stream = [System.IO.File]::Open($filePath, [System.IO.FileMode]::Create)
$writer = New-Object System.IO.BinaryWriter($stream)

# RIFF header
$writer.Write([System.Text.Encoding]::ASCII.GetBytes("RIFF"))
$writer.Write([System.BitConverter]::GetBytes([Int32]$fileSize), 0, 4)
$writer.Write([System.Text.Encoding]::ASCII.GetBytes("WAVE"))

# fmt chunk
$writer.Write([System.Text.Encoding]::ASCII.GetBytes("fmt "))
$writer.Write([System.BitConverter]::GetBytes([Int32]16), 0, 4)
$writer.Write([System.BitConverter]::GetBytes([Int16]1), 0, 2)
$writer.Write([System.BitConverter]::GetBytes([Int16]$channels), 0, 2)
$writer.Write([System.BitConverter]::GetBytes([Int32]$sampleRate), 0, 4)
$writer.Write([System.BitConverter]::GetBytes([Int32]($sampleRate * $channels * ($bitsPerSample / 8))), 0, 4)
$writer.Write([System.BitConverter]::GetBytes([Int16]($channels * ($bitsPerSample / 8))), 0, 2)
$writer.Write([System.BitConverter]::GetBytes([Int16]$bitsPerSample), 0, 2)

# data chunk
$writer.Write([System.Text.Encoding]::ASCII.GetBytes("data"))
$writer.Write([System.BitConverter]::GetBytes([Int32]$dataSize), 0, 4)

# Generate sine wave PCM samples
for ($i = 0; $i -lt ($sampleRate * $durationSeconds); $i++) {
    $t = $i / $sampleRate
    $sample = [Math]::Sin(2 * [Math]::PI * $frequency * $t)
    $pcmSample = [Int16]($sample * 32767)
    $writer.Write([System.BitConverter]::GetBytes($pcmSample), 0, 2)
}

$writer.Close()
$stream.Close()
Write-Host "Generated audible test_audio.wav successfully!" -ForegroundColor Green