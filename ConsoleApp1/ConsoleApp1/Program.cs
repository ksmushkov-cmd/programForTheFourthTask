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

  
