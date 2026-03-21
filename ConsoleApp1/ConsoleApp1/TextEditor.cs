using System;
using System.Text;
using System.IO;

namespace TextEditorApp {
  public class TextEditor {
    private TextFileOriginator _currentFile;
    private TextCaretaker _caretaker;

    public TextFileOriginator CurrentFile => _currentFile;

    public TextEditor() {
      _caretaker = new TextCaretaker();
    }

    public void OpenFile(string filePath) {
      if (!File.Exists(filePath)) {
        Console.WriteLine($"The file does not exist. Create a new one? (y/n)");
        if (Console.ReadLine().ToLower() == "y") {
          _currentFile = new TextFileOriginator(filePath);
          _currentFile.Save();
          Console.WriteLine($"New file created:{filePath}");
        } else {
          return;
        }
      } else {
        _currentFile = new TextFileOriginator(filePath);
      }

      // Save the initial state to history
      _caretaker.SaveState(_currentFile);
      Console.WriteLine($"Opened file:{_currentFile.FileName}");
      Console.WriteLine($"Content:\n{_currentFile.Content}");
    }

    public void DisplayContent() {
      if (_currentFile == null) {
        Console.WriteLine("The file is not open!");
        return;
      }

      Console.WriteLine($"\nCURRENT FILE:{_currentFile.FileName}\n" +
                        $"{new string('─', 50)}\n" +
                        $"{_currentFile.Content}\n" +
                        $"{new string('─', 50)}");
    }

    public void EditContent() {
      if (_currentFile == null) {
        Console.WriteLine("Open the file first!");
        return;
      }

      Console.WriteLine("Enter new text (blank line - end):");

      StringBuilder newContent = new StringBuilder();
      string line;
      while (!string.IsNullOrEmpty(line = Console.ReadLine())) {
        newContent.AppendLine(line);
      }
      // Save the state before the change
       _caretaker.SaveState(_currentFile);

      // Update the contents
      _currentFile.Content = newContent.ToString();
      Console.WriteLine("Text updated!");
    }

    public void SaveFile() {
      if (_currentFile == null) {
        Console.WriteLine("No open file!");
        return;
      }

      _currentFile.Save();
      Console.WriteLine($"File saved:{_currentFile.FileName}");
    }

    public void Undo() {
      if (_currentFile == null) {
        Console.WriteLine("No open file!");
        return;
      }

      if (_caretaker.Undo(_currentFile)) {
        Console.WriteLine("Rollback completed!");
      } else {
        Console.WriteLine("No changes to roll back!");
      }
    }

    public void Redo() {
      if (_currentFile == null) {
        Console.WriteLine("No open file!");
        return;
      }

      if (_caretaker.Redo(_currentFile)) {
        Console.WriteLine("Replay done!");
      } else {
        Console.WriteLine("No changes to repeat!");
      }
    }

    public void ShowHistory() {
      _caretaker.ShowHistory();
    }
  }
}
