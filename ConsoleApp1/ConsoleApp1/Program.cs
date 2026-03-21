using System;
using System.Collections.Generic;
using System.Linq;

namespace TextEditorApp {
  class Program {
    static void Main(string[] args) {
      Console.WriteLine("📁 WELCOME TO THE TEXT EDITOR!\n" +
                        "==============================\n");

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
            case "0":
              isRunning = false;
              Console.WriteLine("Goodbye!");
              break;
            default:
              Console.WriteLine("Invalid choice. Please try again.");
              break;
          }
        } catch (Exception ex) {
           Console.WriteLine($"Error:{ex.Message}");
        }

        if (isRunning)  {
          Console.WriteLine("\nPress any key to continue...");
          Console.ReadKey();
          Console.Clear();
        }
      }
    }

    static void ShowMainMenu() {
      Console.WriteLine("\n📋 MAIN MENU:\n" +
                        "1. Open file\n" +
                        "2. Edit text\n" +
                        "3. Show content\n" +
                        "4. Save file\n" +
                        "5. Undo\n" +
                        "6. Redo\n" +
                        "7. Change history\n" +
                        "8. Search files by keywords\n" +
                        "9. Serialization/Deserialization\n" +
                        "0. Exit");
      Console.Write("Select an action: ");
    }

    static void OpenFileMenu(TextEditor editor) {
      Console.Write("Enter file path: ");
      string path = Console.ReadLine();
      editor.OpenFile(path);
    }

    static void SearchFilesMenu(FileSearcher searcher, TextEditor editor) {
      Console.Write("Enter directory to search: ");
      string directory = Console.ReadLine();

      Console.Write("Enter keywords (comma separated): ");
      List<string> keywords = Console.ReadLine()
        .Split(',')
        .Select(k => k.Trim())
        .Where(k => !string.IsNullOrEmpty(k))
        .ToList();

      Console.Write("Search in subdirectories? (y/n): ");
      bool searchSubdirs = Console.ReadLine().ToLower() == "y";

      List<TextFileOriginator> foundFiles = searcher.SearchByKeywords(directory, keywords, searchSubdirs);
      searcher.DisplayFoundFiles();

      if (foundFiles.Count > 0) {
        Console.Write("Do you want to open one of the files? (enter number or 0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int fileNumber) && fileNumber > 0 && fileNumber <= foundFiles.Count) {
          string selectedPath = foundFiles[fileNumber - 1].FilePath;
          Console.WriteLine($"Opening:{foundFiles[fileNumber - 1].FileName}");
          editor.OpenFile(selectedPath);
        }
      }
    }

    static void SerializationMenu(TextEditor editor) {
      Console.WriteLine("\nSERIALIZATION:\n" +
                        "1. Binary serialization of current file\n" +
                        "2. Binary deserialization\n" +
                        "3. XML serialization of current file\n" +
                        "4. XML deserialization");
      Console.Write("Select: ");

      string choice = Console.ReadLine();

      switch (choice) {
        case "1":
          Console.Write("Enter save name (without extension): ");
          string binName = Console.ReadLine();

          if (editor.CurrentFile == null) {
            Console.WriteLine("No file is currently open!");
            break;
          }

          try {
            string savePath = binName + ".bin";
            editor.CurrentFile.BinarySerialize(savePath);
            Console.WriteLine($"File saved:{savePath}");
          } catch (Exception ex) {
            Console.WriteLine($"Error:{ex.Message}");
          }
          break;

        case "2":
          Console.Write("Enter binary file name to load (with .bin extension): ");
          string binLoad = Console.ReadLine();

          try {
            TextFileOriginator loaded = TextFileOriginator.BinaryDeserialize(binLoad);
            Console.WriteLine($"File loaded:{loaded.FileName}\n" +
                              $"Preview:{(loaded.Content.Length > 50 ? loaded.Content.Substring(0, 50) + "..." : loaded.Content)}\n" +
                              $"📅 Modified:{loaded.LastModified}");
            } catch (Exception ex) {
              Console.WriteLine($"❌ Error: {ex.Message}");
            }
            break;

        case "3":
          Console.Write("Enter save name (without extension): ");
          string xmlName = Console.ReadLine();

          if (editor.CurrentFile == null) {
            Console.WriteLine("No file is currently open!");
            break;
          }

          try {
            string savePath = xmlName + ".xml";
            editor.CurrentFile.XmlSerialize(savePath);
            Console.WriteLine($"File saved: {savePath}");
          } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
          }
          break;

        case "4":
          Console.Write("Enter XML file name to load (with .xml extension): ");
          string xmlLoad = Console.ReadLine();

          try {
            TextFileOriginator loaded = TextFileOriginator.XmlDeserialize(xmlLoad);
            Console.WriteLine($"File loaded:{loaded.FileName}\n" +
                              $"Preview:{(loaded.Content.Length > 50 ? loaded.Content.Substring(0, 50) + "..." : loaded.Content)}\n" +
                              $"Modified:{loaded.LastModified}");
          } catch (Exception ex) {
            Console.WriteLine($"Error:{ex.Message}");
          }
          break;

        default:
          Console.WriteLine("Invalid choice.");
          break;
      }
    }
  }
}