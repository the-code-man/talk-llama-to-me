using NAudio.Wave;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Talkllama.DesktopClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private WaveIn? _waveIn;
    private WaveFileWriter? _writer;
    private readonly string _tempAudioPath = "temp_audio.wav";
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _orchestratorUrl = "http://localhost:5000/api/prompt/audio";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void RecordButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        StartRecording();
    }

    private async void RecordButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        StopRecording();
        await SendAudioToOrchestratorAsync();
    }

    private void StartRecording()
    {
        RecordButton.Background = new SolidColorBrush(Colors.DarkRed);
        RecordButton.Content = "🔴 Recording...";
        StatusLabel.Text = "Listening...";
        OutputTextBlock.Text += $"{Environment.NewLine}Capturing audio from microphone...{Environment.NewLine}";

        _waveIn = new WaveIn
        {
            WaveFormat = new WaveFormat(16000, 16, 1)       // 16 kHz Mono for Whisper
        };

        _writer = new WaveFileWriter(_tempAudioPath, _waveIn.WaveFormat);
        _waveIn.DataAvailable += (s, a) => _writer.Write(a.Buffer, 0, a.BytesRecorded);
        _waveIn.StartRecording();
    }

    private void StopRecording()
    {
        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        _writer?.Flush();
        _writer?.Dispose();

        RecordButton.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
        RecordButton.Content = "Hold to Speak";
        StatusLabel.Text = "Processing with Orchestrator...";
    }

    private async Task SendAudioToOrchestratorAsync()
    {
        if (!File.Exists(_tempAudioPath)) return;

        try
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(_tempAudioPath);
            using var streamContent = new StreamContent(fileStream);

            form.Add(streamContent, "file", "voice_input.wav");

            var response = await _httpClient.PostAsync(_orchestratorUrl, form);

            if (response.IsSuccessStatusCode)
            {
                var resultJson = await response.Content.ReadAsStringAsync();
                OutputTextBlock.Text += $"{Environment.NewLine}✅ Orchestrator Response: {Environment.NewLine}{resultJson}";
                StatusLabel.Text = "Done";
            }
            else
            {
                OutputTextBlock.Text += $"{Environment.NewLine}❌ Server Error: {Environment.NewLine}{response.StatusCode}";
                StatusLabel.Text = "ERROR!";
            }
        }
        catch (Exception ex)
        {
            OutputTextBlock.Text += $"{Environment.NewLine}❌ Connection Error: {Environment.NewLine}{ex.Message}";
            StatusLabel.Text = "Connection Failed";
        }
    }
}