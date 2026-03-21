using System;
using System.Collections.Generic;

namespace TextEditorApp {
  public class TextCaretaker {
    private Stack<object> _history;  // History for rollback
    private Stack<object> _redoStack;  // To repeat   

    public TextCaretaker() { 
      _history = new Stack<object>();
      _redoStack = new Stack<object>();
    }

    /// Save the current state to history
    public void SaveState(IOriginator originator) { 
      _history.Push(originator.CreateMemento());
      _redoStack.Clear();
    }

    /// Save the state to history (without clearing Redo)
    public void PushState(IOriginator originator) { 
      _history.Push(originator.CreateMemento());
    }

    /// Rollback to the previous state (Undo)
    public bool Undo(IOriginator originator) { 
      if (_history.Count < 2) {
        return false;
      }

      // Save the current state to the Redo stack
      _redoStack.Push(originator.CreateMemento());
      // Remove the current state
       _history.Pop();

       // Restore the previous one
       object previousState = _history.Peek();
       originator.RestoreFromMemento(previousState);

       return true;
    }

    /// Redoing an undone action
    public bool Redo(IOriginator originator) {
      if (_redoStack.Count == 0) {
        return false;
      }

      // Save the current state to history
      _history.Push(originator.CreateMemento());

      // Restore from the Redo stack
      object redoState = _redoStack.Pop();
      originator.RestoreFromMemento(redoState);

      return true;
    }

    public int HistoryCount => _history.Count;

    public void ShowHistory() {
      if (_history.Count == 0) {
        Console.WriteLine("History is empty.");
        return;
      }

      Console.WriteLine($"HISTORY OF CHANGES (total: {_history.Count}):");
      int index = 1;
      foreach (var memento in _history) {
        if (memento is TextMemento textMemento) {
          Console.Write($"{++index}. ");
          textMemento.Print();
        }
      }
    }

    /// Get the latest snapshot (without extracting)
    public object PeekLastState() {
      return _history.Count > 0 ? _history.Peek() : null;
    }

    public void ClearHistory() {
      _history.Clear();
      _redoStack.Clear();
    }
  }
}
