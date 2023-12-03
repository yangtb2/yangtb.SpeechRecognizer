using PortAudioSharp;
using SherpaNcnn;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace yangtb.SpeechRecognizer;

public class SpeechRecognizer
{
    public event EventHandler<string> NewSegment;

    public event EventHandler<string> NewResult;

    public bool Enable { get; set; } = true;

    public SpeechRecognizer(IRecognizerInputStream inputStream)
    {
        _inputStream = inputStream;
        var config = new OnlineRecognizerConfig()
        {
            DecoderConfig = new TransducerDecoderConfig()
            {
                DecodingMethod = "greedy_search",
                NumActivePaths = 4
            },
            FeatConfig = new FeatureConfig()
            {
                FeatureDim = 80,
                SampleRate = _inputStream.SampleRate,
            },
            ModelConfig = new TransducerModelConfig()
            {
                EncoderParam = @"runtime\Models\encoder_jit_trace-pnnx.ncnn.param",
                EncoderBin = @"runtime\Models\encoder_jit_trace-pnnx.ncnn.bin",
                DecoderParam = @"runtime\Models\decoder_jit_trace-pnnx.ncnn.param",
                DecoderBin = @"runtime\Models\decoder_jit_trace-pnnx.ncnn.bin",
                JoinerParam = @"runtime\Models\joiner_jit_trace-pnnx.ncnn.param",
                JoinerBin = @"runtime\Models\joiner_jit_trace-pnnx.ncnn.bin", 
                Tokens = @"runtime\Models\tokens.txt", 
                NumThreads = 1,
                UseVulkanCompute = 0
            },
            EnableEndpoint = 1,
            Rule1MinTrailingSilence = 2.4f,
            Rule2MinTrailingSilence = 1.2f,
            Rule3MinUtteranceLength = 20.0f
        };
        _recognizer = new OnlineRecognizer(config);
        _recognizerStream = _recognizer.CreateStream();
        _inputStream.DataAvailable += InputStream_DataAvailable;
    }

    private void InputStream_DataAvailable(object? sender, float[] e)
    {
        _recognizerStream.AcceptWaveform(_inputStream.SampleRate, e);
    }

    public bool Start()
    {
        lock (this)
        {
            if(_recognizeTask != null)
                return false;
            Enable = true;
            _inputStream.Start();
            _recognizeTask = Task.Run(() =>
            {
                var lastText = "";
                while (Enable)
                {
                    while (Enable && _recognizer.IsReady(_recognizerStream))
                    {
                        _recognizer.Decode(_recognizerStream);
                    }

                    var text = _recognizer.GetResult(_recognizerStream).Text;
                    bool isEndpoint = _recognizer.IsEndpoint(_recognizerStream);
                    if (!string.IsNullOrWhiteSpace(text) && lastText != text)
                    {
                        lastText = text;
                        NewResult?.Invoke(this, text);
                    }

                    if (isEndpoint)
                    {
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            NewSegment?.Invoke(this, text);
                        }
                        _recognizer.Reset(_recognizerStream);
                    }

                    Thread.Sleep(200); // ms
                }
            });
            return true;
        }
    }

    public bool Stop(int timeout)
    {
        lock (this)
        {
            Enable = false;
            _inputStream.Stop();
            var result = _recognizeTask?.Wait(timeout);
            if (result == true)
            {
                _recognizeTask = null;
            }

            return result != false;
        }
    }

    private readonly OnlineRecognizer _recognizer;
    private readonly OnlineStream _recognizerStream;
    private readonly IRecognizerInputStream _inputStream;
    private Task? _recognizeTask;
}