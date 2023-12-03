using System;
using System.Windows.Input;

namespace yangtb.SpeechRecognizer;

public class ActionCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    
    public ActionCommand(Action execution)
    {
        _execution = execution ?? throw new ArgumentNullException(nameof(execution));
    }
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _execution.Invoke();
    }

    private readonly Action _execution;
}