using System;

namespace yangtb.SpeechRecognizer;

public interface IRecognizerInputStream : IDisposable
{
    public event EventHandler<float[]> DataAvailable ;

    public float SampleRate { get; }

    public void Start();

    public void Stop();
}