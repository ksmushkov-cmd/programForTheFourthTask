using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TextEditorApp {

  class TextEditorException : Exception {
    public TextEditorException(string message) : base(message) { }
  }

  [Serializable]
  public class TextFile {
    public string FileName { get; set; }       // File name
    public string Content { get; set; }        // Content
    public string FilePath { get; set; }       // Full path
    public DateTime LastModified { get; set; } // Change date
        
    public TextFile() { }
        
    public TextFile(string path) {
      FilePath = path;
      FileName = Path.GetFileName(path);

      if (File.Exists(path)) {
        Content = File.ReadAllText(path);
        LastModified = File.GetLastWriteTime(path);
      } else {
        Content = "";
        LastModified = DateTime.Now;
      }
    }

    public void Save() {
      File.WriteAllText(FilePath, Content);
      LastModified = DateTime.Now;
    }

    public void BinarySerialize(string savePath) {
      try {
        using (FileStream fs = new FileStream(savePath, FileMode.Create)) {
          BinaryFormatter formatter = new BinaryFormatter();
          formatter.Serialize(fs, this);
        }
        Console.WriteLine($"The file is saved as binary:{savePath}");
      } catch (Exception ex) {
        throw new TextEditorException($"Binary serialization error:{ex.Message}");
      }
    }

    public static TextFile BinaryDeserialize(string loadPath) {
      try {
        using (FileStream fs = new FileStream(loadPath, FileMode.Open)) {
          BinaryFormatter formatter = new BinaryFormatter();
          return (TextFile)formatter.Deserialize(fs);
        }
      } catch (Exception ex) {
        throw new TextEditorException($"Binary deserialization error:{ex.Message}");
      }
    }

    public void XmlSerialize(string savePath) {
      try {
        XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
        using (StreamWriter writer = new StreamWriter(savePath)) {
          serializer.Serialize(writer, this);
        } 
        Console.WriteLine($"XML file saved:{savePath}");
      } catch (Exception ex) {
        throw new TextEditorException($"XML serialization error:{ex.Message}");
      }
    }

    public static TextFile XmlDeserialize(string loadPath) {
      try {
        XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
        using (StreamReader reader = new StreamReader(loadPath)) {
          return (TextFile)serializer.Deserialize(reader);
        }
      } catch (Exception ex) {
        throw new TextEditorException($"XML deserialization error:{ex.Message}");
      }
    }

    public override string ToString() {
      return $"{FileName} (changed: {LastModified})";
    }
  }

  public class FileSearcher {
    private List<TextFile> _foundFiles;

    public FileSearcher() {
      _foundFiles = new List<TextFile>();
    }

    // Search for files in a directory by keywords
    public List<TextFile> SearchByKeywords(string directoryPath, List<string> keywords, bool searchSubdirectories = true) {
      _foundFiles.Clear();

      if (!Directory.Exists(directoryPath)) {
        throw new TextEditorException($"Directory does not exist:{directoryPath}");
      }

      // Get all text files (.txt)
      string[] allFiles;
      if (searchSubdirectories) { 
        allFiles = Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories);
      } else {
        allFiles = Directory.GetFiles(directoryPath, "*.txt", SearchOption.TopDirectoryOnly);
      }

      Console.WriteLine($"Searching in {allFiles.Length} files...");

      foreach (string filePath in allFiles) {
        try {
          string content = File.ReadAllText(filePath).ToLower();
          bool found = false;

          // Check each keyword
          foreach (string keyword in keywords) {
            if (content.Contains(keyword.ToLower())) {
              found = true;
              break;
            }
          }

          if (found) {
            _foundFiles.Add(new TextFile(filePath));
          }
        } catch (Exception ex) {
          Console.WriteLine($"Reading error {filePath}:{ex.Message}");
        }
      }

      Console.WriteLine($"Files found:{_foundFiles.Count}");
      return _foundFiles;
    }

    // Search by one keyword
    public List<TextFile> SearchByKeyword(string directoryPath, string keyword, bool searchSubdirectories = true) {
      return SearchByKeywords(directoryPath, new List<string> { keyword }, searchSubdirectories);
    }
        
    // Show found files
    public void DisplayFoundFiles() {
      if (_foundFiles.Count == 0) {
        Console.WriteLine("Files not found.");
        return;
      }

      Console.WriteLine("\nFOUND FILES:");
      for (int fileIndex = 0; fileIndex < _foundFiles.Count; ++fileIndex) {
        Console.WriteLine($"{fileIndex + 1}. {_foundFiles[fileIndex].FileName}");
      }
    }
  }

  // MEMENTO - State for rolling back changes
  public class TextMemento {
    public string Content { get; private set; }
    public DateTime Timestamp { get; private set; }

    public TextMemento(string content) {
      Content = content;
      Timestamp = DateTime.Now;
    }
  }

  public class TextEditor {
    private TextFile _currentFile;
    private Stack<TextMemento> _history;// History of changes for rollback
    private Stack<TextMemento> _redoStack; // To redo the cancellation
        
    public TextEditor() {
      _history = new Stack<TextMemento>();
      _redoStack = new Stack<TextMemento>();
    }

    public TextFile CurrentFile { 
        get { return _currentFile; } 
    }

    public void OpenFile(string filePath) {
      if (!File.Exists(filePath)) {
        Console.WriteLine($"The file does not exist. Create a new one? (y/n)");
        if (Console.ReadLine().ToLower() == "y") {
          _currentFile = new TextFile(filePath);
          _currentFile.Save();
          Console.WriteLine($"New file created:{filePath}");
        } else { 
          return;
        }
      } else {
        _currentFile = new TextFile(filePath);
      }

      // Save the initial state
      SaveToHistory();
      Console.WriteLine($"Opened file:{_currentFile.FileName}");
      Console.WriteLine($"Content:\n{_currentFile.Content}");
    }

    public void DisplayContent() {
      if (_currentFile == null) {
        Console.WriteLine("The file is not open!");
        return;
      }

      Console.WriteLine($"\nCURRENT FILE:{_currentFile.FileName}\n" +
                        $"{"─".Repeat(50)}\n" +
                        $"{_currentFile.Content}\n" +
                        $"{"─".Repeat(50)}");
    }

    public void EditContent() {
      if (_currentFile == null) {
        Console.WriteLine("Open the file first!");
        return;
      }

      Console.WriteLine("Enter new text (enter a blank line to complete):");

      StringBuilder newContent = new StringBuilder();
      string line;
      while (!string.IsNullOrEmpty(line = Console.ReadLine())) {
        newContent.AppendLine(line);
      }

      // Save the current state to history before changing
      SaveToHistory();

      // Update the contents
      _currentFile.Content = newContent.ToString();
      Console.WriteLine("Text updated!");

      // Clear the redo stack on new change
      _redoStack.Clear();
    }

    public void SaveFile() {
      if (_currentFile == null) {
        Console.WriteLine("No open file!");
        return;
      }

      _currentFile.Save();
      Console.WriteLine($"File saved:{_currentFile.FileName}");
    }

    // Save the state to history
    private void SaveToHistory() {
      if (_currentFile != null) {
        _history.Push(new TextMemento(_currentFile.Content));
      }
    }

    // ROLLBACK (Undo) - return to the previous state
    public void Undo() {
      if (_currentFile == null) {
        Console.WriteLine("No open file!");
        return;
      }

      // There must always be at least one state
      int limit;
      limit = 2;
      if (_history.Count < limit)   {
        Console.WriteLine("No changes to roll back!");
        return;
      }

      // Save the current state to the retry stack
      _redoStack.Push(new TextMemento(_currentFile.Content));

      // Remove the current state
      _history.Pop();

      // Restore the previous one
      TextMemento previous = _history.Peek();
      _currentFile.Content = previous.Content;

      Console.WriteLine($"Rollback to the state from{previous.Timestamp:HH:mm:ss}");
    }

    // REDO - redo a undone change
    public void Redo() {
      if (_currentFile == null) {
        Console.WriteLine("No open file!");
        return;
      }

      if (_redoStack.Count == 0) {
        Console.WriteLine("No changes to repeat!");
        return;
      }

      TextMemento redoState = _redoStack.Pop();
      _history.Push(new TextMemento(_currentFile.Content));
      _currentFile.Content = redoState.Content;

      Console.WriteLine($"Repeat to state from {redoState.Timestamp:HH:mm:ss}");
    }

    public void ShowHistory() {
      if (_history.Count == 0) {
        Console.WriteLine("History is empty.");
        return;
      }

      Console.WriteLine($"HISTORY OF CHANGES (total: {_history.Count}):");
      int index = 1;
      foreach (var memento in _history.Reverse()) {
        string preview = memento.Content.Length > 30 
          ? memento.Content.Substring(0, 30) + "..." 
          : memento.Content;
        Console.WriteLine($"{index++}. [{memento.Timestamp:HH:mm:ss}] {preview}");
       }
    }
  }

  // Helper class for repeating strings
  public static class StringExtensions {
    public static string Repeat(this string str, int count) {
      return string.Concat(Enumerable.Repeat(str, count));
    }
  }

  class Program {
    static void Main(string[] args) {
      Console.WriteLine("WELCOME TO THE TEXT EDITOR!" +
                        "============================\n");

      TextEditor editor = new TextEditor();
      FileSearcher searcher = new FileSearcher();

      bool isRunning = true;

      while (isRunning) {
        ShowMainMenu();
        string choice = Console.ReadLine();

        try {
          switch (choice) {
            case "1":
              OpenFileMenu(editor);
              break;

            case "2":
              editor.EditContent();
              break;

            case "3":
              editor.DisplayContent();
              break;

            case "4": 
              editor.SaveFile();
              break;

            case "5": 
              editor.Undo();
              break;

            case "6": 
              editor.Redo();
              break;

            case "7":
              editor.ShowHistory();
              break;

            case "8": 
              SearchFilesMenu(searcher, editor);
              break;

            case "9":  
              SerializationMenu(editor);
              break;

            case "0":  // Выход
              isRunning = false;
              Console.WriteLine("Goodbye!");
              break;

            default:
              Console.WriteLine("Incorrect selection. Try again.");
              break;
            }
        } catch (TextEditorException ex) {
          Console.WriteLine($"Editor error:{ex.Message}");
        } catch (Exception ex) {
          Console.WriteLine($"Unexpected error:{ex.Message}");
        }

        if (isRunning) {
          Console.WriteLine("\nPress any key to continue...");
          Console.ReadKey();
          Console.Clear();
        }
      }
    }

    static void ShowMainMenu() {
      Console.WriteLine("\n MAIN MENU:" +
                              "1. Open file\n" +
                              "2. Edit text\n" +
                              "3. Show contents\n" +
                              "4. Save file\n" +
                              "5. Undo\n" +
                              "6. Redo\n" +
                              "7. Change history\n" +
                              "8. Search files by keywords\n" +
                              "9. Serialize/Deserialize\n" +
                              "0. Exit");
      Console.Write("Select action: ");
    }

    static void OpenFileMenu(TextEditor editor) {
      Console.Write("Enter the path to the file:");
      string path = Console.ReadLine();
      editor.OpenFile(path);
    }

    static void SearchFilesMenu(FileSearcher searcher, TextEditor editor) {
      Console.Write("Enter directory to search:");
      string directory = Console.ReadLine();

      Console.Write("Enter keywords (separated by commas):");
      string keywordsInput = Console.ReadLine();
      List<string> keywords = keywordsInput.Split(',').Select(k => k.Trim()).ToList();

      Console.Write("Search in subfolders? (y/n):");
      bool searchSubdirs = Console.ReadLine().ToLower() == "y";

      List<TextFile> foundFiles = searcher.SearchByKeywords(directory, keywords, searchSubdirs);
      searcher.DisplayFoundFiles();

      if (foundFiles.Count > 0) {
        Console.Write("Do you want to open one of the files? (enter a number or 0 to cancel):");
        if (int.TryParse(Console.ReadLine(), out int fileNumber) && fileNumber > 0 && fileNumber <= foundFiles.Count) {
          string selectedFilePath = foundFiles[fileNumber - 1].FilePath;
          Console.WriteLine($"Opening:{foundFiles[fileNumber - 1].FileName}");
          editor.OpenFile(selectedFilePath);
        }
      }
    }

    static void SerializationMenu(TextEditor editor) {
      Console.WriteLine("\nSERIALIZATION:!\n" +
                        "1. Binary serialization of the current file\n" +
                        "2. Binary deserialization\n" +
                        "3. XML serialization of the current file\n" +
                        "4. XML deserialization");

      Console.Write("Select: ");
      string choice = Console.ReadLine();

      switch (choice) {
        case "1":
          Console.Write("Enter a save name (without extension): ");
          string binaryFileName = Console.ReadLine();
    
          if (editor.CurrentFile == null) {
            Console.WriteLine("No file is currently open. Please open a file first.");
            break;
          }
    
          try {
            string savePath = binaryFileName + ".bin";
            editor.CurrentFile.BinarySerialize(savePath);
            Console.WriteLine($"File successfully serialized to:{savePath}");
          } catch (Exception ex) {
            Console.WriteLine($"Serialization error:{ex.Message}");
          }

          break;

        case "2": 
          Console.Write("Enter binary file name to load (with .bin extension): ");
          string binaryLoadFile = Console.ReadLine();
        
          try  {
            TextFile loadedFile = TextFile.BinaryDeserialize(binaryLoadFile);
            Console.WriteLine($"File loaded:{loadedFile.FileName}");
            Console.WriteLine($"Content preview:{loadedFile.Content.Substring(0, Math.Min(50, loadedFile.Content.Length))}...");
            Console.WriteLine($"Last modified:{loadedFile.LastModified}");
          } catch (Exception ex)  {
            Console.WriteLine($"Deserialization error: {ex.Message}");
          }

          break;

        case "3": 
          Console.Write("Enter a save name (without extension): ");
          string xmlFileName = Console.ReadLine();
        
          if (editor.CurrentFile == null)  {
            Console.WriteLine("No file is currently open. Please open a file first.");
            break;
          }
        
          try {
            string savePath = xmlFileName + ".xml";
            editor.CurrentFile.XmlSerialize(savePath);
            Console.WriteLine($"File successfully serialized to: {savePath}");
          } catch (Exception ex) {
            Console.WriteLine($"Serialization error: {ex.Message}");
          }

          break;

        case "4":
          Console.Write("Enter XML file name to load (with .xml extension): ");
          string xmlLoadFile = Console.ReadLine();
        
          try {
            TextFile loadedFile = TextFile.XmlDeserialize(xmlLoadFile);
            Console.WriteLine($"File loaded:{loadedFile.FileName}\n" +
                              $"Content preview:{loadedFile.Content.Substring(0, Math.Min(50, loadedFile.Content.Length))}...\n" +
                              $"Last modified:{loadedFile.LastModified}");
          } catch (Exception ex) {
            Console.WriteLine($"Deserialization error:{ex.Message}");
          }

          break;

        default:
          Console.WriteLine("Feature in development");
          break;
      }
    }
  }
}