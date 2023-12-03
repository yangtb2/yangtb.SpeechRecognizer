using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace yangtb.SpeechRecognizer;

public class AudioStream : IRecognizerInputStream
{
    public AudioStream()
    {
        var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        _audio = new WasapiLoopbackCapture(device);
        _audio.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(38400, 1);
        _audio.DataAvailable += Loop_DataAvailable;
    }

    private void Loop_DataAvailable(object? sender, WaveInEventArgs e)
    {
        var length = e.BytesRecorded / sizeof(float);
        var data = new float[length];
        Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesRecorded);
        DataAvailable?.Invoke(this, data);
    }

    public void Dispose()
    {
        _audio.Dispose();
    }

    public event EventHandler<float[]>? DataAvailable;
    public float SampleRate => _audio.WaveFormat.SampleRate;
    public void Start()
    {
        _audio.StartRecording();
    }

    public void Stop()
    {
        _audio.StopRecording();
    }

    private readonly WasapiLoopbackCapture _audio;
}