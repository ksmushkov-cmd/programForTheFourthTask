using System;

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
      for (int fileIndex = 0; fileIndex < _foundFiles.Count; fileIndex++) {
        Console.WriteLine($"{fileIndex + 1}. {_foundFiles[fileIndex].FileName}");
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

