
using System;
using System.Windows.Input;

namespace ElectronicBooks.Heap.Behaviors
{
  public class DelegateCommand : ICommand
  {
    private readonly Action _action;

    public DelegateCommand(Action action) => this._action = action;

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter) => this._action();

    public event EventHandler CanExecuteChanged;
  }
}
