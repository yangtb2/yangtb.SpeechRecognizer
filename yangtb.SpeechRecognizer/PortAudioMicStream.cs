using System;
using System.Runtime.InteropServices;
using PortAudioSharp;

namespace yangtb.SpeechRecognizer;

public class PortAudioMicStream : IRecognizerInputStream
{
    public event EventHandler<float[]>? DataAvailable;
    public float SampleRate { get; }

    public PortAudioMicStream() : this(0){}

    public PortAudioMicStream(float sampleRate)
    {
        PortAudio.Initialize();
        var micIndex = PortAudio.DefaultInputDevice;
        if (micIndex < 0)
            throw new InvalidOperationException("Cannot find default input device");
        var mic = PortAudio.GetDeviceInfo(micIndex);
        SampleRate = sampleRate > 0 ? sampleRate : (float)mic.defaultSampleRate;
        var param = new StreamParameters()
        {
            device = micIndex,
            channelCount = 1,
            sampleFormat = SampleFormat.Float32,
            suggestedLatency = mic.defaultLowInputLatency,
            hostApiSpecificStreamInfo = IntPtr.Zero
        };
        _stream = new Stream(param, null, SampleRate, 0, StreamFlags.ClipOff, OnDataAvailable, IntPtr.Zero);
    }

    public void Start()
    {
        _stream.Start();
    }

    public void Stop()
    {
        _stream.Stop();
    }

    private StreamCallbackResult OnDataAvailable(IntPtr input, IntPtr output, uint count, ref StreamCallbackTimeInfo info,
        StreamCallbackFlags flags, IntPtr ptr)
    {
        float[] samples = new float[count];
        Marshal.Copy(input, samples, 0, (int)count);

        DataAvailable?.Invoke(this, samples);

        return StreamCallbackResult.Continue;
    }

    public void Dispose()
    {
        _stream.Close();
        _stream.Dispose();
        PortAudio.Terminate();
    }

    ~PortAudioMicStream()
    {
        Dispose();
    }

    private readonly Stream _stream;
}