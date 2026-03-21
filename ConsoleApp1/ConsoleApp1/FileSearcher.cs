using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextEditorApp {
  public class FileSearcher {
    private List<TextFileOriginator> _foundFiles;

    public FileSearcher() {
      _foundFiles = new List<TextFileOriginator>();
    }

    /// Search files by keywords
    public List<TextFileOriginator> SearchByKeywords(string directoryPath, List<string> keywords, bool searchSubdirectories = true) {
      _foundFiles.Clear();

      if (!Directory.Exists(directoryPath)) { 
        throw new Exception($"Directory does not exist:{directoryPath}");
      }

      string[] allFiles = Directory.GetFiles(directoryPath, "*.txt",  
        searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
      
      Console.WriteLine($"Searching in {allFiles.Length} files...");

      foreach (string filePath in allFiles) {
        try {
          string content = File.ReadAllText(filePath).ToLower();
          bool found = false;

          foreach (string keyword in keywords) {
            if (content.Contains(keyword.ToLower())) {
              found = true;
              break;
            }
          }

          if (found) {
            _foundFiles.Add(new TextFileOriginator(filePath));
          }
        } catch (Exception ex) {
          Console.WriteLine($"Reading error {filePath}: {ex.Message}");
        }
      }

      Console.WriteLine($"Files found:{_foundFiles.Count}");
      return _foundFiles;
    }

    public void DisplayFoundFiles() {
      if (_foundFiles.Count == 0) {
        Console.WriteLine("No files found.");
        return;
      }

      Console.WriteLine("\nFOUND FILES:");
      for (int fileIndex = 0; fileIndex < _foundFiles.Count; fileIndex++) {
        Console.WriteLine($"{fileIndex + 1}. {_foundFiles[fileIndex].FileName}");
      }
    }
  }
}
