using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace yangtb.SpeechRecognizer;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        var stream = new AudioStream();
        var recognize = new SpeechRecognizer(stream);
        recognize.NewResult += Recognize_NewResult;
        recognize.NewSegment += RecognizeNewSegment;
        recognize.Start();
    }

    private async void RecognizeNewSegment(object? sender, string e)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Text = "";
            AllText += e + Environment.NewLine;
        });
    }

    private async void Recognize_NewResult(object? sender, string e)
    {
        await Application.Current.Dispatcher.InvokeAsync(() => Text = e);
    }

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            SetField(ref _text, value);
        }
    }

    private string _allText;
    public string AllText
    {
        get => _allText;
        set
        {
            SetField(ref _allText, value);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}