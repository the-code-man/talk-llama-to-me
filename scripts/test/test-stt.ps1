# Ensure the HTTP assembly is loaded for PowerShell 5.1
Add-Type -AssemblyName System.Net.Http

$filePath = "test_audio.wav"
$uri = "http://localhost:5002/api/audio/transcribe"

Write-Host "Sending '$filePath' to Whisper STT service..." -ForegroundColor Cyan

$client = New-Object System.Net.Http.HttpClient
$form = New-Object System.Net.Http.MultipartFormDataContent
$fileStream = [System.IO.File]::OpenRead((Resolve-Path $filePath))
$streamContent = New-Object System.Net.Http.StreamContent($fileStream)

$form.Add($streamContent, "file", "test_audio.wav")

try {
    $response = $client.PostAsync($uri, $form).Result
    $responseContent = $response.Content.ReadAsStringAsync().Result
    
    if ($response.IsSuccessStatusCode) {
        Write-Host "Transcription Successful!" -ForegroundColor Green
        Write-Host "----------------------------------------"
        Write-Host "Response: $responseContent" -ForegroundColor Yellow
        Write-Host "----------------------------------------"
    } else {
        Write-Host "Server returned error status: $($response.StatusCode)" -ForegroundColor Red
        Write-Host "Details: $responseContent" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error calling STT service: $_" -ForegroundColor Red
}
finally {
    $fileStream.Close()
    $client.Dispose()
}