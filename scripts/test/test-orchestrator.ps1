Add-Type -AssemblyName System.Net.Http

# Point to the Orchestrator endpoint, NOT the STT service directly
$filePath = "test_audio.wav"
$uri = "http://localhost:5000/api/prompt/audio"

if (-not (Test-Path $filePath)) {
    Write-Host "Error: Could not find '$filePath'. Make sure your test audio file is in the directory." -ForegroundColor Red
    return
}

Write-Host "Sending audio to Orchestrator (Port 5000)..." -ForegroundColor Cyan

$client = New-Object System.Net.Http.HttpClient
$form = New-Object System.Net.Http.MultipartFormDataContent
$fileStream = [System.IO.File]::OpenRead((Resolve-Path $filePath))
$streamContent = New-Object System.Net.Http.StreamContent($fileStream)

$form.Add($streamContent, "file", "test_audio.wav")

try {
    $response = $client.PostAsync($uri, $form).Result
    $responseContent = $response.Content.ReadAsStringAsync().Result
    
    if ($response.IsSuccessStatusCode) {
        Write-Host "End-to-End Pipeline Successful!" -ForegroundColor Green
        Write-Host "----------------------------------------"
        Write-Host "Response JSON: $responseContent" -ForegroundColor Yellow
        Write-Host "----------------------------------------"
    } else {
        Write-Host "Server returned error status: $($response.StatusCode)" -ForegroundColor Red
        Write-Host "Details: $responseContent" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error calling Orchestrator: $_" -ForegroundColor Red
}
finally {
    $fileStream.Close()
    $client.Dispose()
}